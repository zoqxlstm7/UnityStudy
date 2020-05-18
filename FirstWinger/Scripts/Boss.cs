using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Boss : Enemy
{
    const float FIRE_TRANSFORM_FOTATION_START = -30.0f;     // 발사방향 회전시 초기값
    const float FIRE_TRANSFORM_ROTATION_INTERVAL = 15.0f;   // 발사방향 회전시 간격
    const float ACTION_UPDATE_INTERVAL = 1.0f;              // 행동 업데이트 주기

    [SyncVar]
    bool needBattleMove = false;    // 배틀시 움직임 플래그
    [SerializeField]
    float battleMoveMax;            // 최대 이동거리

    Vector3 battleMoveStartPos;     // 배틀 시작 지점
    int fireRemainCountPerOnetime;  // 한타임당 남아있는 총알

    [SyncVar]
    float battleMoveLength;         // 배틀시 움직일 거리

    [SyncVar]
    [SerializeField]
    Vector3 currentFireTransformRotation;   // 현재 발사 방향

    // 보스 총알을 반환하도록 오버라이딩
    protected override int BulletIndex => BulletManager.BOSS_BULLET_INDEX;

    [SerializeField]
    Transform[] missileFireTransform;       // 미사일 발사 위치

    // 접속한 플레이어 정보
    Player[] players;
    Player[] Players
    {
        get
        {
            if (players == null)
                players = FindObjectsOfType<Player>();
            return players;
        }
    }

    bool specialAttack = false;             // 미사일을 발사하기 위한 플래그

    [SerializeField]
    float missileSpeed = 1;                 // 미사일 발사 시 사용할 속도

    /// <summary>
    /// 배틀 시작시 초기화
    /// </summary>
    protected override void SetBattleState()
    {
        base.SetBattleState();

        // 위치와 남은 총알 초기화
        battleMoveStartPos = transform.position;
        fireRemainCountPerOnetime = fireRemainCount;

        // 회전값 초기화
        currentFireTransformRotation.z = FIRE_TRANSFORM_FOTATION_START;
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = currentFireTransformRotation;
        fireTransform.localRotation = quat;
    }

    /// <summary>
    /// 전투 처리
    /// </summary>
    protected override void UpdateBattle()
    {
        // 움직임이 필요한 경우
        if (needBattleMove)
        {
            UpdateBattleMove();
        }
        else // 이동이 끝났다면 총알 발사 처리
        {
            if(Time.time - lastActionUpdateTime > ACTION_UPDATE_INTERVAL)
            {
                // 남아있는 총알을 모두 발사하면 움직임 시작
                if(fireRemainCountPerOnetime > 0)
                {
                    // 특수 공격인 경우
                    if(specialAttack)
                    {
                        // 유도 미사일 발사
                        FireChase();
                    }
                    else
                    {
                        // 일반 총알 발사
                        Fire();
                        RotateFireTransform();
                    }
                    fireRemainCountPerOnetime--;
                }
                else
                {
                    SetBattleMove();
                }

                lastActionUpdateTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 이동할 방향과 거리를 세팅
    /// </summary>
    void SetBattleMove()
    {
        if (!isServer)
            return;

        // 랜덤한 방향으로 이동을 시작하기 위한 부분
        float halfPingpongHeight = 0.0f;
        float rand = Random.value;
        if (rand < 0.5f)
            halfPingpongHeight = battleMoveMax * 0.5f;
        else
            halfPingpongHeight = -battleMoveMax * 0.5f;

        // 랜덤한 거리를 이동하기 위한 부분
        // battleMoveMax의 1~3배 사이의 거리를 이동
        float newBattleMoveLength = Random.Range(battleMoveMax, battleMoveMax * 3.0f);

        // host 플레이어인 경우 RPC를 보낸다
        RpcSetBattleMove(halfPingpongHeight, newBattleMoveLength);
    }

    /// <summary>
    /// 클라이언트에게 이동방향과 거리를 전송
    /// </summary>
    /// <param name="halfPingpongHeight">위아래로 이동할 방향</param>
    /// <param name="newBattleMoveLength">이동할 거리</param>
    [ClientRpc]
    public void RpcSetBattleMove(float halfPingpongHeight, float newBattleMoveLength)
    {
        needBattleMove = true;
        targetPosition = battleMoveStartPos;
        targetPosition.y += halfPingpongHeight;

        // 사라질 때는 0부터 속도 증가
        currentSpeed = 0.0f;
        moveStartTime = Time.time;

        battleMoveLength = newBattleMoveLength;

        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 전투시 움직임 처리
    /// </summary>
    void UpdateBattleMove()
    {
        UpdateSpeed();

        // 상하 움직임 처리 부분
        Vector3 oldPosition = transform.position;
        float distance = Vector3.Distance(targetPosition, transform.position);
        if(distance == 0)
        {
            // host 플레이어인 경우 RPC를 보낸다
            if (isServer)
                RpcChangeBattleMoveTarget();
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, distance / currentSpeed, MaxSpeed * 0.2f);

        // 지정한 이동길이만큼 움직였다면 발사함수 호출
        battleMoveLength -= Vector3.Distance(oldPosition, transform.position);
        if (battleMoveLength <= 0)
            SetBattleFire();
    }

    /// <summary>
    /// 클라이언트에게 상하 위치 전달
    /// </summary>
    [ClientRpc]
    public void RpcChangeBattleMoveTarget()
    {
        // 위쪽에 도착한 경우 타격포지션을 아래로 바꾸면서 핑퐁효과를 준다
        if (targetPosition.y > battleMoveStartPos.y)
            targetPosition.y = battleMoveStartPos.y - battleMoveMax * 0.5f;
        else
            targetPosition.y = battleMoveStartPos.y + battleMoveMax * 0.5f;

        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 보스 발사 준비
    /// </summary>
    void SetBattleFire()
    {
        // host 플레이어인 경우 RPC를 보낸다
        if (isServer)
            RpcSetBattleFire();
    }

    /// <summary>
    /// 클라이언트에게 전투시작 준비 정보를 보냄
    /// </summary>
    [ClientRpc]
    public void RpcSetBattleFire()
    {
        needBattleMove = false;
        moveStartTime = Time.time;
        fireRemainCountPerOnetime = fireRemainCount;

        // 회전값 초기화
        currentFireTransformRotation.z = FIRE_TRANSFORM_FOTATION_START;
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = currentFireTransformRotation;
        fireTransform.localRotation = quat;

        // 일반 공격과 미사일 공격을 번갈아 할 수 있도록
        specialAttack = !specialAttack;

        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 발사 지점 회전 정보를 RPC로 보냄
    /// </summary>
    void RotateFireTransform()
    {
        // host 플레이어인 경우 RPC를 보낸다
        if (isServer)
            RpcRotateFireTransform();
    }

    /// <summary>
    /// 클라이언트에게 총알을 발사할때마다 변경된 회전정보를 전달
    /// </summary>
    [ClientRpc]
    public void RpcRotateFireTransform()
    {
        currentFireTransformRotation.z += FIRE_TRANSFORM_ROTATION_INTERVAL;
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = currentFireTransformRotation;
        fireTransform.localRotation = quat;

        base.SetDirtyBit(1);
    }

    public void FireChase()
    {
        // 살아있는 플레이어만 리스트에 삽입
        List<Player> alivePlayer = new List<Player>();
        for (int i = 0; i < Players.Length; i++)
        {
            if (!Players[i].IsDead)
            {
                alivePlayer.Add(Players[i]);
            }
        }

        // 플레이어 중 랜덤한 타겟을 선택
        int index = Random.Range(0, alivePlayer.Count);
        int targetInstanceID = alivePlayer[index].ActorInstanceID;

        // 지정된 지점에서 순차적으로 발사할 수 있도록 남은 총알을 빼서 인덱스를 설정
        Transform fireTransform = missileFireTransform[missileFireTransform.Length - fireRemainCountPerOnetime];
        // 미사일을 추적모드로 발사
        GuidedMissile missile = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.GUIDED_MISSILE_INDEX, fireTransform.position) as GuidedMissile;
        if (missile)
        {
            missile.FireChase(targetInstanceID, actorInstanceID, fireTransform.right, missileSpeed, damage);
        }
    }

    /// <summary>
    /// 보스가 죽었을 때 게임종료 선언
    /// </summary>
    protected override void OnDead()
    {
        base.OnDead();

        // 서버인 경우 게임 종료함수 호출
        if (isServer)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().OnGameEnd(true);
    }
}
