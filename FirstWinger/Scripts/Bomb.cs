using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bomb : Bullet
{
    const float MAX_ROTATE_TIME = 30.0f;    // 회전하는 최대 시간
    const float MAX_ROTATE_Z = 90.0f;       // 최대 회전 값

    [SerializeField]
    Rigidbody selfRigidbody;    // 오브젝트가 가지고 있는 리지바디
    [SerializeField]
    Vector3 force;              // 가할 힘의 양

    [SyncVar]
    float rotateStartTime = 0.0f;   // 회전을 시작한 시간
    [SyncVar]
    [SerializeField]
    float currentRotateZ;           // 변경되는 z축 회전값

    Vector3 currentEulerAngles = Vector3.zero;  // 현재 z축 회전값

    [SerializeField]
    SphereCollider explodeArea;     // 폭발 영역

    protected override void UpdateTransform()
    {
        if (!needMove)
            return;

        if (CheckScreenBottom())
            return;

        UpdateRotate();
    }

    /// <summary>
    /// 폭탄이 화면 밖으로 나갔을 때의 처리
    /// </summary>
    /// <returns>나갔는지 유무</returns>
    bool CheckScreenBottom()
    {
        Transform mainBGQuadTransform = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().MainBGQuadTransform;

        // 화면 하단인지 검사
        if(transform.position.y < -mainBGQuadTransform.localScale.y * 0.5f)
        {
            // 화면하단인 경우 화면하단으로 위치 고정
            Vector3 newPos = transform.position;
            newPos.y = -mainBGQuadTransform.localScale.y * 0.5f;

            StopForExplosion(newPos);
            Explode();
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// 폭탄 이동 중지 처리
    /// </summary>
    /// <param name="stopPos">중지된 지점</param>
    void StopForExplosion(Vector3 stopPos)
    {
        transform.position = stopPos;

        selfRigidbody.useGravity = false;       // 중력 사용 해제
        selfRigidbody.velocity = Vector3.zero;  // force 초기화

        needMove = false;
    }

    void UpdateRotate()
    {
        // 시간에 다른 회전값의 변화된 값을 저장
        currentRotateZ = Mathf.Lerp(currentRotateZ, MAX_ROTATE_Z, (Time.time - rotateStartTime) / MAX_ROTATE_TIME);
        currentEulerAngles.z = -currentRotateZ;

        // Quaternion 값으로 변환 후 로컬 회전값 적용
        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = currentEulerAngles;
        transform.localRotation = rot;
    }

    /// <summary>
    /// 발사 처리
    /// </summary>
    /// <param name="ownerInstanceID">총알을 쏜 객체의 InstanceID</param>
    /// <param name="firePosition">발사지점이 어딘지</param>
    /// <param name="direction">발사된 방향이 어딘지</param>
    /// <param name="speed">발사 속도는 어떤지</param>
    /// <param name="damage">총알 데미지</param>
    public override void Fire(int ownerInstanceID, Vector3 direction, float speed, int damage)
    {
        base.Fire(ownerInstanceID, direction, speed, damage);

        AddForce(force);
    }

    /// <summary>
    /// 회전값 초기화 및 AddForce 명령 처리
    /// </summary>
    /// <param name="force">가할 힘의 양</param>
    void InternalAddForce(Vector3 force)
    {
        selfRigidbody.velocity = Vector3.zero;  // force 초기화
        selfRigidbody.AddForce(force);
        rotateStartTime = Time.time;
        currentRotateZ = 0.0f;
        transform.localRotation = Quaternion.identity;
        selfRigidbody.useGravity = true;    // 중력을 다시 활성화
        explodeArea.enabled = false;        // 폭발 영역 콜라이더 비활성화
    }

    public void AddForce(Vector3 force)
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdAddForce(force);

        // MonoBehaviour 인스턴스의 Update로 호출되므로 서버인지를 확인하여 처리
        if (isServer)
        {
            // Host 플레이어인 경우 RPC로 보내고
            RpcAddForce(force);
        }
        else
        {
            // Client 플레이어인 경우 Cmd를 호스트로 보낸 후 자신은 셀프 지정
            CmdAddForce(force);
            if (isLocalPlayer)
                InternalAddForce(force);
        }
    }

    /// <summary>
    /// 서버에게 정보 전송
    /// </summary>
    /// <param name="force">가할 힘의 양</param>
    [Command]
    public void CmdAddForce(Vector3 force)
    {
        InternalAddForce(force);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 클라이언트에게 정보 전송
    /// </summary>
    /// <param name="force">가할 힘의 양</param>
    [ClientRpc]
    public void RpcAddForce(Vector3 force)
    {
        InternalAddForce(force);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 폭발 이펙트 생성 처리
    /// </summary>
    void InternalExplode()
    {
        Debug.Log("InternalExplode is called");
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.BOMB_EXPLODE_FX_INDEX, transform.position);

        explodeArea.enabled = true; // 폭발 영역 콜라이더 활성화
        List<Enemy> targetList = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.GetContainEnemies(explodeArea);

        // 폭발 영역 내의 에너미 객체에 OnBulletHited 호출
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].IsDead)
                continue;

            targetList[i].OnBulletHited(damage, targetList[i].transform.position);
        }

        // 오브젝트가 켜져있다면 꺼준다
        if (gameObject.activeSelf)
            Disapear();
    }

    /// <summary>
    /// 폭발 처리
    /// </summary>
    void Explode()
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdExplode();

        // MonoBehaviour 인스턴스의 Update로 호출되므로 서버인지를 확인하여 처리
        if (isServer)
        {
            // Host 플레이어인 경우 RPC로 보내고
            RpcExplode();
        }
        else
        {
            // Client 플레이어인 경우 Cmd를 호스트로 보낸 후 자신은 셀프 지정
            CmdExplode();
            if (isLocalPlayer)
                InternalExplode();
        }
    }

    /// <summary>
    /// 서버에게 정보 전달
    /// </summary>
    [Command]
    public void CmdExplode()
    {
        InternalExplode();
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 클라이언트에게 정보 전달
    /// </summary>
    [ClientRpc]
    public void RpcExplode()
    {
        InternalExplode();
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 충돌 처리
    /// </summary>
    /// <param name="collider">충돌한 콜라이더</param>
    /// <returns></returns>
    protected override bool OnBulletCollision(Collider collider)
    {
        if (!base.OnBulletCollision(collider))
        {
            return false;
        }

        Explode();
        return true;
    }
}
