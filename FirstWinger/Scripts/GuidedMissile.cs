using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GuidedMissile : Bullet
{
    const float CHASE_FECTOR = 1.5f;        // 타겟 추적시 방향을 전환하는 정도
    const float CHASING_START_TIME = 0.7f;  // 타겟 추적을 시작하는 시간(발사시간 기준)
    const float CHASING_END_TIME = 4.5f;    // 타겟 주적을 종료하는 시간(발사시간 기준)

    [SyncVar]
    [SerializeField]
    int targetInstanceID;                   // 목표 actor의 actorInstanceID
    [SerializeField]
    Vector3 chaseVector;                    // 이동 벡터
    [SerializeField]
    Vector3 rotateVector = Vector3.zero;    // 회전 벡터
    [SerializeField]
    bool flipDirection = true;              // 디폴트 상태가 left 방향일 경우 true

    bool needChase = true;                  // 추적이 시작된 경우 true

    /// <summary>
    /// 추적 미사일 실행 함수
    /// </summary>
    /// <param name="targetInstanceID">추적할 객체의 인스턴스 ID</param>
    /// <param name="ownerInstanceID">총알을 쏜 객체의 인스턴스 ID</param>
    /// <param name="firePosition">발사 지점</param>
    /// <param name="direction">발사 방향</param>
    /// <param name="speed">발사 속도</param>
    /// <param name="damage">총알 데미지</param>
    public void FireChase(int targetInstanceID, int ownerInstanceID, Vector3 direction, float speed, int damage)
    {
        if (!isServer)
            return;

        // host 플레이어인 경우 RPC를 보냄
        RpcSetTargetInstanceID(targetInstanceID);
        base.Fire(ownerInstanceID, direction, speed, damage);
    }

    /// <summary>
    /// 클라이언트에게 타겟 인스턴스 ID 전송
    /// </summary>
    /// <param name="targetInstanceID">유도 미사일의 타겟이 된 객체의 인스턴스 ID</param>
    [ClientRpc]
    public void RpcSetTargetInstanceID(int targetInstanceID)
    {
        this.targetInstanceID = targetInstanceID;
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// transform 관련 업데이트
    /// </summary>
    protected override void UpdateTransform()
    {
        UpdateMove();
        UpdateRotate();
    }

    /// <summary>
    /// 이동 업데이트 처리
    /// </summary>
    protected override void UpdateMove()
    {
        if (!needMove)
            return;

        Vector3 moveVector = moveDirection.normalized * speed * Time.deltaTime;
        // 타겟을 추적하기 위한 계산
        float deltaTime = Time.time - firedTime;

        if(deltaTime > CHASING_START_TIME && deltaTime < CHASING_END_TIME)
        {
            Actor target = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.GetActor(targetInstanceID);
            if(target != null)
            {
                // 현재 위치에서 타겟까지 벡터
                Vector3 targetVector = target.transform.position - transform.position;

                // 이동 벡터와 타겟 벡터의 사이의 벡터를 계산
                chaseVector = Vector3.Lerp(moveVector.normalized, targetVector.normalized, CHASE_FECTOR * Time.deltaTime);

                // 이동 벡터에 추적벡터를 더하고 스피드에 따른 길이를 다시 계산
                moveVector += chaseVector.normalized;
                moveVector = moveVector.normalized * speed * Time.deltaTime;

                // 수정 계산된 이동벡터를 필드에 저장해서 다음 UpdateMove에서 사용가능하게 한다
                moveDirection = moveVector.normalized;
            }
        }

        moveVector = AdjustMove(moveVector);
        transform.position += moveVector;

        // moveVector 방향으로 회전시키기 위한 계산
        rotateVector.z = Vector2.SignedAngle(Vector2.right, moveVector);
        if (flipDirection)
            rotateVector.z += 180.0f;
    }

    /// <summary>
    /// 회전값 업데이트 처리
    /// </summary>
    void UpdateRotate()
    {
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = rotateVector;
        transform.rotation = quat;
    }
}
