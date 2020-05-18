using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    const float LifeTime = 15f;             // 생존시간

    // NetworkBehaviour 상속 클래스라 SyncVar가 안됨
    //Actor owner;                          // 총알을 발사한 객체 정보
    [SyncVar]
    [SerializeField]
    int ownerInstanceID;                    // 총알을 발사한 객체의 인스턴스ID

    [SyncVar]
    [SerializeField]
    protected Vector3 moveDirection = Vector3.zero;   // 이동방향
    [SyncVar]
    [SerializeField]
    protected float speed = 0.0f;           // 속도

    [SyncVar]
    protected bool needMove = false;        // 이동 플래그

    [SyncVar]
    protected float firedTime;              // 발사 시간
    [SyncVar]
    bool hited = false;                     // 충돌이 되었는지

    [SyncVar]
    [SerializeField]
    protected int damage = 1;               // 총알 데미지

    [SyncVar]
    [SerializeField]
    string filePath;                        // 총알 프리팹 파일 경로
    public string FilePath
    {
        get { return filePath; }
        set { filePath = value; }
    }

    private void Start()
    {
        // 클라이언트 쪽이라면
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {
            InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
            // 부모 설정
            transform.SetParent(inGameSceneMain.BulletManager.transform);
            // 캐시 추가 
            inGameSceneMain.BulletCacheSystem.Add(filePath, gameObject);
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 총알 생존 상태 체크
        if (ProcessDisappearCondition())
            return;

        UpdateTransform();
    }

    /// <summary>
    /// 트랜스폼 변경 처리 함수
    /// </summary>
    protected virtual void UpdateTransform()
    {
        UpdateMove();
    }

    /// <summary>
    /// 이동 처리
    /// </summary>
    protected virtual void UpdateMove()
    {
        if (!needMove)
            return;

        // 프레임당 이동할 거리
        Vector3 moveVector = moveDirection.normalized * speed * Time.deltaTime;
        moveVector = AdjustMove(moveVector);
        transform.position += moveVector;
    }

    /// <summary>
    /// 발사 처리 정보 초기화
    /// </summary>
    /// <param name="ownerInstanceID">총알을 쏜 객체의 InstanceID</param>
    /// <param name="firePosition">발사지점이 어딘지</param>
    /// <param name="direction">발사된 방향이 어딘지</param>
    /// <param name="speed">발사 속도는 어떤지</param>
    /// <param name="damage">총알 데미지</param>
    void InternalFire(int ownerInstanceID, Vector3 direction, float speed, int damage)
    {
        //this.owner = owner;
        //transform.position = firePosition;
        this.ownerInstanceID = ownerInstanceID;
        moveDirection = direction;
        this.speed = speed;
        this.damage = damage;

        needMove = true;
        firedTime = Time.time;
    }

    public virtual void Fire(int ownerInstanceID, Vector3 direction, float speed, int damage)
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdFire(ownerInstanceID, direction, speed, damage);

        // MonoBehaviour 인스턴스의 Update로 호출되므로 서버인지를 확인하여 처리
        if (isServer)
        {
            // Host 플레이어인 경우 RPC로 보내고
            RpcFire(ownerInstanceID, direction, speed, damage);
        }
        else
        {
            // Client 플레이어인 경우 Cmd를 호스트로 보낸 후 자신은 셀프 지정
            CmdFire(ownerInstanceID, direction, speed, damage);
            if (isLocalPlayer)
                InternalFire(ownerInstanceID, direction, speed, damage);
        }
    }

    /// <summary>
    /// 서버에게 발사 처리 정보 전송
    /// </summary>
    [Command]
    public void CmdFire(int ownerInstanceID, Vector3 direction, float speed, int damage)
    {
        InternalFire(ownerInstanceID, direction, speed, damage);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 클라이언트에게 발사 처리 정보 전송
    /// </summary>
    [ClientRpc]
    public void RpcFire(int ownerInstanceID, Vector3 direction, float speed, int damage)
    {
        InternalFire(ownerInstanceID, direction, speed, damage);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 총알이 너무 빨리 움직였을 때를 대비한 moveVector 수정 함수
    /// </summary>
    /// <param name="moveVector">이동할 벡터</param>
    /// <returns></returns>
    protected Vector3 AdjustMove(Vector3 moveVector)
    {
        RaycastHit hit;

        // Raycast: 시점은 있고 종점은 없다. 무한히 뻗어감
        // Linecast: 시점과 종점이 있다.
        // 부딪힌 경우
        if (Physics.Linecast(transform.position, transform.position + moveVector, out hit))
        {
            // 충돌한 객체가 플레이어나 에너미 객체인지 확인
            int colliderLayer = hit.collider.gameObject.layer;
            if (colliderLayer != LayerMask.NameToLayer("Enemy") && colliderLayer != LayerMask.NameToLayer("Player"))
                return moveVector;

            // moveVector를 충돌지점의 거리만큼으로 수정해준다.
            moveVector = hit.point - transform.position;
            // 충돌 처리 함수 호출
            OnBulletCollision(hit.collider);
        }

        return moveVector;
    }

    /// <summary>
    /// 총알에 오브젝트가 부딪혔을 때의 처리
    /// </summary>
    /// <param name="collider">충돌한 콜라이더</param>
    /// <returns></returns>
    protected virtual bool OnBulletCollision(Collider collider)
    {
        // 두번 호출되지 않도록 리턴
        if (hited)
            return false;

        // 총알끼리 충돌하는 것을 코드로서 한번더 방지
        if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyBullet")
            || collider.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            return false;

        // 인스턴스 ID로 총알 오너 객체를 반환받음
        Actor owner = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.GetActor(ownerInstanceID);
        // 호스트나 클라이언트 중 한쪽이 접속이 끊어졌을 때 발생할 수 있는 오류 검사
        if (owner == null)
            return false;

        Actor actor = collider.GetComponentInParent<Actor>();
        if (actor == null)
            return false;

        // 죽었을 경우나 내가 쏜 총알인 경우 리턴
        if (actor.IsDead || actor.gameObject.layer == owner.gameObject.layer)
            return false;

        // 불렛 데미지 처리
        actor.OnBulletHited(damage, transform.position);

        // 충돌했으므로 콜라이더를 꺼준다.
        //Collider myCollider = GetComponentInChildren<Collider>();
        //myCollider.enabled = false;

        hited = true;
        needMove = false;   // 충돌했으므로 이동 종료

        // 부딪힌 곳에 폭발 이펙트 생성 후 총알 삭제
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.BULLET_DISAPPEAR_FX_INDEX, transform.position);
        go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Disapear();

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 객체가 플레이어나 에너미 객체인지 확인
        int colliderLayer = other.gameObject.layer;
        if (colliderLayer != LayerMask.NameToLayer("Enemy") && colliderLayer != LayerMask.NameToLayer("Player"))
            return;

        // 충돌 처리 함수 호출
        OnBulletCollision(other);
    }

    /// <summary>
    /// 총알 생존 상태 처리
    /// </summary>
    /// <returns></returns>
    bool ProcessDisappearCondition()
    {
        // 범위를 벗어날 때 파괴 처리
        if(transform.position.x > 15f || transform.position.x < -15f
            || transform.position.y > 15f || transform.position.y < -15)
        {
            Disapear();
            return true;
        }// 생존시간이 끝난 경우 파괴 처리
        else if(Time.time - firedTime > LifeTime)
        {
            Disapear();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 총알 파괴
    /// </summary>
    protected void Disapear()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Remove(this);
    }

    /// <summary>
    /// 클라이언트에게 Active 정보 전송
    /// </summary>
    /// <param name="value">액티브 여부</param>
    [ClientRpc]
    public void RpcSetActive(bool value)
    {
        gameObject.SetActive(value);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 시작 위치 설정
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Vector3 position)
    {
        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdSetPosition(position);

        // MonoBehaviour 인스턴스의 Update로 호출되므로 서버인지를 확인하여 처리
        if (isServer)
        {
            // Host 플레이어인 경우 RPC로 보내고
            RpcSetPosition(position);
        }
        else
        {
            // Client 플레이어인 경우 Cmd를 호스트로 보낸 후 자신은 셀프 지정
            CmdSetPosition(position);
            if (isLocalPlayer)
                transform.position = position;
        }
    }

    /// <summary>
    /// 호스트에게 위치 전송
    /// </summary>
    /// <param name="position">이동 위치</param>
    [Command]
    public void CmdSetPosition(Vector3 position)
    {
        transform.position = position;
        // 변수의 값이 바뀌었다고 서버에 통보
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 클라이언트에게 위치 전송
    /// </summary>
    /// <param name="position">이동 위치</param>
    [ClientRpc]
    public void RpcSetPosition(Vector3 position)
    {
        transform.position = position;
        // 변수의 값이 바뀌었다고 서버에 통보
        base.SetDirtyBit(1);
    }
}
