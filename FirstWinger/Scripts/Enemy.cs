using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : Actor
{
    // 적 상태 열거형 객체
    // switch문에서 좀 더 안전할 수 있도록 int를 상속
    public enum State : int
    {
        None = -1,  // 사용전
        Ready = 0,  // 준비 완료
        Appear,     // 등장
        Battle,     // 전투중
        Dead,       // 사망
        Disappear,  // 퇴장
    };

    [SyncVar]
    [SerializeField]
    State currentState = State.None;    // 현재 상태값

    protected const float MaxSpeed = 10.0f;       // 최고 속도 제한
    const float MaxSpeedTime = 0.5f;    // 최고 속도까지 걸리는 시간 정의

    [SyncVar]
    [SerializeField]
    protected Vector3 targetPosition;             // 이동할 위치
    [SyncVar]
    [SerializeField]
    protected float currentSpeed;                 // 현재 속도

    [SyncVar]
    protected Vector3 currentVelocity;            // 방향 벡터

    [SyncVar]
    protected float moveStartTime = 0.0f;         // 움직이기 시작한 시간 체크

    [SerializeField]
    protected Transform fireTransform;            // 발사 위치
    [SyncVar]
    [SerializeField]
    float bulletSpeed = 1;              // 총알 속도

    [SyncVar]
    protected float lastActionUpdateTime = 0.0f;  // 총알을 언제 발사했는지 체크
    [SyncVar]
    [SerializeField]
    protected int fireRemainCount = 1;            // 남아있는 총알

    [SyncVar]
    [SerializeField]
    int gamePoint = 10;                 // 파괴 시 획득할 게임 점수

    [SyncVar]
    [SerializeField]
    string filePath;                    // 로드될 파일 경로
    public string FilePath
    {
        get { return filePath; }
        set { filePath = value; }
    }

    [SyncVar]
    Vector3 appearPoint;                // 입장시 도착 위치
    [SyncVar]
    Vector3 disappearPoint;             // 퇴장시 목표 위치

    [SyncVar]
    [SerializeField]
    float itemDropRate;                 // 아이템 생성 확률
    [SyncVar]
    [SerializeField]
    int itemDropID;                     // 아이템 생성 시 참조할 아이템 드랍 테이블의 인덱스

    // 총알 인덱스
    protected virtual int BulletIndex
    {
        get => BulletManager.ENEMY_BULLET_INDEX;
    }

    /// <summary>
    /// 초기화 작업
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        // singleton이 NetworkManager형을 리턴하므로 형변환 후 처리
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {
            // EnemyManager를 부모로 지정
            transform.SetParent(inGameSceneMain.EnemyManager.transform);
            // EnemyCacheSystem에 현재 오브젝트와 파일경로 추가
            inGameSceneMain.EnemyCacheSystem.Add(filePath, gameObject);
            gameObject.SetActive(false);
        }

        // 인스턴스 ID가 있다면 ActorManager클래스에 본 객체 등록
        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);
    }

    protected override void UpdateActor()
    {
        // 상태 처리
        switch (currentState)
        {
            case State.None:
                break;
            case State.Ready:
                UpdateReady();
                break;
            case State.Dead:
                break;
            // 출현하거나 사라질 때 실행
            case State.Appear:
            case State.Disappear:
                UpdateSpeed();
                UpdateMove();
                break;
            case State.Battle:
                UpdateBattle();
                break;
        }
    }

    /// <summary>
    /// 가속을 할 때 사용할 함수
    /// </summary>
    protected void UpdateSpeed()
    {
        // currentSpeed에서 MaxSpeed에 도달하는 비율을 흐른 시간만큼 계산
        currentSpeed = Mathf.Lerp(currentSpeed, MaxSpeed, (Time.time - moveStartTime) / MaxSpeed);
    }

    // 이동을 할 때 사용할 함수
    void UpdateMove()
    {
        float distance = Vector3.Distance(targetPosition, transform.position);
        // 지정한 지점에 도착했다면
        if(distance == 0)
        {
            Arrived();
            return;
        }

        // 단위벡터로 변환 후 현재의 속도값을 곱해주어 초당 이동거리를 구한다.
        currentVelocity = (targetPosition - transform.position).normalized * currentSpeed;

        // currentVelocity : 3차원에 대한 가속도값(사실상 이동벡터)
        // smoothTime : 어느정도 시간만에 자연스럽게 이동할건지 설정
        // 속도 = 거리 / 시간 이므로 시간 = 거리 / 속도
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, distance / currentSpeed, MaxSpeed);
    }

    // 도착했는지 확인
    void Arrived()
    {
        currentSpeed = 0.0f;

        if(currentState == State.Appear)
        {
            SetBattleState();
        }
        else // if(currentState == State.Disappear)
        {
            currentState = State.None;
            // 캐시로 반환
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveEnemy(this);
        }
    }

    /// <summary>
    /// 배틀 상태 설정
    /// </summary>
    protected virtual void SetBattleState()
    {
        currentState = State.Battle;
        lastActionUpdateTime = Time.time;    // 출현하는 시간이 전투시작 시점
    }

    /// <summary>
    /// 에너미 객체 정보 초기화
    /// </summary>
    /// <param name="data">초기화에 사용될 편대 정보</param>
    public void Reset(SquadronMemberStruct data)
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을 때
        //CmdReset(data);

        // MonoBehaviour 인스턴스의 Update로 호출되므로 서버인지를 확인하여 처리
        if (isServer)
        {
            // 호스트 플레이어인 경우 RPC를 이용하여 클라이언트에게 정보 전송
            RpcReset(data);
        }
        else
        {
            // 클라이언트 플레이어인 경우 Cmd를 이용하여 호스트에게 정보 전송
            CmdReset(data);
            if (isLocalPlayer)
                ResetData(data);
        }
    }

    void ResetData(SquadronMemberStruct data)
    {
        // 에너미ID를 통해 생성한 테이블에서 에너미 정보를 가져옴
        EnemyStruct enemyStruct = SystemManager.Instance.EnemyTable.GetEnemy(data.enemyID);

        currentHP = maxHP = enemyStruct.maxHP;
        damage = enemyStruct.damage;
        crashDamage = enemyStruct.crashDamage;
        bulletSpeed = enemyStruct.bulletSpeed;
        fireRemainCount = enemyStruct.fireRemainCount;
        gamePoint = enemyStruct.gamePoint;

        appearPoint = new Vector3(data.appearPointX, data.appearPointY, 0);
        disappearPoint = new Vector3(data.disappearPointX, data.disappearPointY, 0);

        itemDropRate = enemyStruct.itemDropRate;    // 아이템 생성 확률
        itemDropID = enemyStruct.itemDropID;        // 아이템 드랍 테이블 참조 인덱스

        currentState = State.Ready;
        lastActionUpdateTime = Time.time;

        isDead = false; // enemy는 재사용이므로 초기화
    }

    /// <summary>
    /// 호스트에게 리셋 정보를 전달
    /// </summary>
    /// <param name="data">리셋에 사용될 데이터</param>
    [Command]
    public void CmdReset(SquadronMemberStruct data)
    {
        ResetData(data);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 클라이언트에게 리셋 정보를 전달
    /// </summary>
    /// <param name="data">리셋에 사용될 데이터</param>
    [ClientRpc]
    public void RpcReset(SquadronMemberStruct data)
    {
        ResetData(data);
        base.SetDirtyBit(1);
    }

    // 어디로 나타날지
    public void Appear(Vector3 targetPos)
    {
        targetPosition = targetPos;
        currentSpeed = MaxSpeed;

        currentState = State.Appear;

        moveStartTime = Time.time;
    }

    // 어디로 사라질건지
    void Disappear(Vector3 targetPos)
    {
        targetPosition = targetPos;
        currentSpeed = 0;

        currentState = State.Disappear;
        moveStartTime = Time.time;
    }

    /// <summary>
    /// 준비되었다면 입장
    /// </summary>
    void UpdateReady()
    {
        if(Time.time - lastActionUpdateTime > 1.0f)
        {
            Appear(appearPoint);
        }
    }

    /// <summary>
    /// 전투 시작 시 처리
    /// </summary>
    protected virtual void UpdateBattle()
    {
        // 1초 단위로 총알 발사
        if(Time.time - lastActionUpdateTime > 1f)
        {
            // 총알이 남아있다면 발사 
            if(fireRemainCount > 0)
            {
                Fire();
                fireRemainCount--;
            }
            else // 총알 소진시 퇴장
            {
                Disappear(disappearPoint);
            }

            lastActionUpdateTime = Time.time;
        }
    }

    // 콜라이더에 들어왔을 때 충돌 체크
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();

        // 플레이어 객체에 충돌되었다고 전달
        if (player)
        {
            if(!player.IsDead)
            {
                BoxCollider box = (BoxCollider)other;
                // 박스 콜라이더가 얼마나 떨어져있을지 모르니 더해줌
                Vector3 crashPos = player.transform.position + box.center;
                // 구한 중점좌표에서 박스 콜라이더의 x사이즈의 절반을 더해 충돌지점(앞쪽)을 산출
                crashPos.x += box.size.x * 0.5f;

                player.OnCrash(crashDamage, crashPos);
            }
        }
    }

    // 발사 처리
    public void Fire()
    {
        // 총알매니저 클래스를 통해 캐싱된 총알오브젝트 사용
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletIndex, fireTransform.position);
        if(bullet)
            bullet.Fire(actorInstanceID, -fireTransform.right, bulletSpeed, damage);
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected override void OnDead()
    {
        base.OnDead();

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        // 점수 획득
        inGameSceneMain.GamePointAccumulator.Accumulate(gamePoint);
        // 에너미 제거
        inGameSceneMain.EnemyManager.RemoveEnemy(this);
        // 아이템 박스 생성
        GenerateItem();
        //inGameSceneMain.ItemBoxManager.Generate(0, transform.position);
        //inGameSceneMain.ItemBoxManager.Generate(1, transform.position);
        //inGameSceneMain.ItemBoxManager.Generate(2, transform.position);

        currentState = State.Dead;
    }

    /// <summary>
    /// 체력 감소 처리 함수
    /// </summary>
    /// <param name="value">감소될 체력 수치</param>
    /// <param name="damagePos">데미지를 입은 위치</param>
    protected override void DecreaseHP(int value, Vector3 damagePos)
    {
        base.DecreaseHP(value, damagePos);

        // 주변에 1m 어딘가에 데미지텍스트 생성
        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Generate(DamageManager.ENEMY_DAMAGE_INDEX, damagePoint, value, Color.magenta);
    }

    /// <summary>
    /// 아이템 생성 처리
    /// </summary>
    void GenerateItem()
    {
        if (!isServer)
            return;

        // 아이템 생성 확률을 검사
        float itemGen = Random.Range(0.0f, 100.0f);
        if (itemGen > itemDropRate)
            return;

        ItemDropTable itemDropTable = SystemManager.Instance.ItemDropTable;
        ItemDropStruct dropStruct = itemDropTable.GetDropData(itemDropID);

        // 어느 아이템을 생성할 것인지 확률 검사
        itemGen = Random.Range(0, dropStruct.rate1 + dropStruct.rate2 + dropStruct.rate3);
        int itemIndex = -1;

        if (itemGen <= dropStruct.rate1)    // 1번 아이템 비율보다 작은 경우
            itemIndex = dropStruct.itemID1;
        else if (itemGen <= (dropStruct.rate1 + dropStruct.rate2))  // 2번 아이템 비율보다 작은 경우
            itemIndex = dropStruct.itemID2;
        else //if (itemGen <= (dropStruct.rate1 + dropStruct.rate2 + dropStruct.rate3)) // 3번 아이템 비율인 경우
            itemIndex = dropStruct.itemID3;

        // 아이템 매니저를 통해 아이템 생성
        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        inGameSceneMain.ItemBoxManager.Generate(itemIndex, transform.position);
    }

    /// <summary>
    /// 에너미 추가
    /// </summary>
    public void AddList()
    {
        // host 플레이어인 경우 RPC로 보냄
        if (isServer)
            RpcAddList();
    }

    /// <summary>
    /// 클라이언트에게 정보 전송
    /// </summary>
    [ClientRpc]
    public void RpcAddList()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.AddList(this);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 에너미 삭제
    /// </summary>
    public void RemoveList()
    {
        // host 플레이어인 경우 RPC로 보냄
        if (isServer)
            RpcRemoveList();
    }

    /// <summary>
    /// 클라이언트에게 정보 전송
    /// </summary>
    [ClientRpc]
    public void RpcRemoveList()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveList(this);
        base.SetDirtyBit(1);
    }
}
