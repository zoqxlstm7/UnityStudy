using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 플레이어와 에너미 모두 상속하여 사용
public class Actor : NetworkBehaviour
{
    [SyncVar]
    [SerializeField]
    protected int maxHP = 100;  // 최대 체력
    public int MaxHP
    {
        get => maxHP;
    }

    [SyncVar]
    [SerializeField]
    protected int currentHP;    // 현재 체력
    public int CurrentHP
    {
        get => currentHP;
    }

    [SyncVar]
    [SerializeField]
    protected int damage = 1;           // 데미지값
    [SyncVar]
    [SerializeField]
    protected int crashDamage = 100;    // 충돌 데미지
    [SyncVar]
    [SerializeField]
    protected bool isDead = false;      // 사망 여부

    // isDead 프로퍼티
    public bool IsDead
    {
        get => isDead;
    }

    [SyncVar]
    protected int actorInstanceID = 0;  // 플레이어인지 적인지 식별할때 사용
    public int ActorInstanceID
    {
        get => actorInstanceID;
    }

    private void Start()
    {
        Initialize();
    }

    // 초기화
    protected virtual void Initialize()
    {
        currentHP = maxHP;

        // 호스트인 경우 
        if (isServer)
        {
            // 인스턴스 아이디를 구하고 클라이언트에 전송
            actorInstanceID = GetInstanceID();
            RpcSetActorInstanceID(actorInstanceID);
        }
    }

    private void Update()
    {
        UpdateActor();
    }

    // 오버라이드해서 사용할 수 있게끔 virtual로 선언
    protected virtual void UpdateActor()
    {

    }

    /// <summary>
    /// 총알 적중 시 데미지 처리
    /// </summary>
    /// <param name="damage">데미지값</param>
    /// <param name="hitPos">맞은 위치</param>
    public virtual void OnBulletHited(int damage, Vector3 hitPos)
    {
        Debug.Log("OnBulletHited damage: " + damage);
        DecreaseHP(damage, hitPos);
    }

    /// <summary>
    /// 비행체 충돌 시 데미지 처리
    /// </summary>
    /// <param name="damage">데미지값</param>
    /// <param name="crashPos">충돌 위치</param>
    public virtual void OnCrash(int damage, Vector3 crashPos)
    {
        DecreaseHP(damage, crashPos);
    }

    /// <summary>
    /// 체력 감소 처리
    /// </summary>
    /// <param name="value">데미지값</param>
    /// <param name="value">데미지를 받는 위치</param>
    protected virtual void DecreaseHP(int value, Vector3 damagePos)
    {
        if (isDead)
            return;

        // 정상적으로 NetworkBehaviour 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdDecreaseHP(position);

        // MonoBehaviour 인스턴스의 Update로 호출되므로 서버인지를 확인하여 처리
        if (isServer)
        {
            // Host 플레이어인 경우 RPC로 보내고
            RpcDecreaseHP(value, damagePos);
        }
        //else
        //{
        //    // Client 플레이어인 경우 Cmd를 호스트로 보낸 후 자신은 셀프 지정
        //    CmdDecreaseHP(value, damagePos);
        //    if (isLocalPlayer)
        //        InternalDecreaseHP(value, damagePos);
        //}
    }

    /// <summary>
    /// 공통적인 체력감소 처리 함수
    /// </summary>
    /// <param name="value"></param>
    /// <param name="damagePos"></param>
    protected virtual void InternalDecreaseHP(int value, Vector3 damagePos)
    {
        if (isDead)
            return;

        currentHP -= value;

        if (currentHP < 0)
            currentHP = 0;

        if(currentHP == 0)
        {
            OnDead();
        }
    }

    /// <summary>
    /// 호스트에게 정보 전달
    /// </summary>
    /// <param name="value">받은 데미지</param>
    /// <param name="damagePos">데미지를 받은 위치</param>
    [Command]
    public void CmdDecreaseHP(int value, Vector3 damagePos)
    {
        InternalDecreaseHP(value, damagePos);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 클라이언트에게 정보 전달
    /// </summary>
    /// <param name="value">받은 데미지</param>
    /// <param name="damagePos">데미지를 받은 위치</param>
    [ClientRpc]
    public void RpcDecreaseHP(int value, Vector3 damagePos)
    {
        InternalDecreaseHP(value, damagePos);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void OnDead()
    {
        Debug.Log(name + "OnDead");
        isDead = true;

        // 기체 폭발 이펙트 생성
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.ACTOR_DEAD_FX_INDEX, transform.position);
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

    /// <summary>
    /// 클라이언트에게 Active 정보 전송
    /// </summary>
    /// <param name="value">액티브 여부</param>
    [ClientRpc]
    public void RpcSetActive(bool value)
    {
        gameObject.SetActive(value);
        // 변수의 값이 바뀌었다고 서버에 통보
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 인스턴스 ID를 클라이언트에게 전송
    /// </summary>
    /// <param name="instID"></param>
    [ClientRpc]
    public void RpcSetActorInstanceID(int instID)
    {
        actorInstanceID = instID;

        // 값이 있다면 ActorManager 클래스에 ID값을 등록
        if (actorInstanceID != 0)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.Regist(actorInstanceID, this);

        base.SetDirtyBit(1);
    }
}
