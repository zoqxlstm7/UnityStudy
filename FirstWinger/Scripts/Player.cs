using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : Actor
{
    const string PLAYER_HUD_PATH = "prefabs/PlayerHUD"; // 체력바 프리팹 경로

    [SerializeField]
    [SyncVar]                           // 동기화될 수 있도록 어트리뷰트 추가
    Vector3 moveVector = Vector3.zero;  // 이동 방향

    [SerializeField]
    NetworkIdentity networkIdentity = null;

    [SerializeField]
    float speed;                        // 속도

    [SerializeField]
    BoxCollider boxCollider;            // 박스 콜라이더 객체

    [SerializeField]
    Transform fireTransform;            // 발사 위치
    [SerializeField]
    float bulletSpeed = 1;              // 총알 속도    

    InputController inputController = new InputController();    // 입력 제어 클래스

    [SyncVar]
    [SerializeField]
    bool host = false;                  // Host 플레이어인지 여부

    [SerializeField]
    Material clientPlayerMaterial;      // 클라이언트 플레이어 메테리얼

    [SyncVar]
    [SerializeField]
    int usableItemCount = 0;            // 사용 가능 아이템 수
    public int ItemCount
    {
        get => usableItemCount;
    }

    /// <summary>
    /// 초기화
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        //// 형변환을 통해 등록된 PlayerStatePanel을 가져옴
        //PlayerStatePanel playerStatePanel = PanelManager.GetPanel(typeof(PlayerStatePanel)) as PlayerStatePanel;
        //// 슬라이더에 체력 정보 설정
        //playerStatePanel.SetHP(currentHP, maxHP);

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        // 로컬 플레이어 지정
        if (isLocalPlayer)
            inGameSceneMain.Hero = this;
        else// 호스트인 경우
            inGameSceneMain.OtherPlayer = this;

        // 호스트 플레이어인지 확인
        if(isServer && isLocalPlayer)
        {
            host = true;
            RpcSetHost();
        }

        // 클라이언트의 경우
        if (!host)
        {
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = clientPlayerMaterial;
        }

        // 인스턴스 ID가 있다면 ActorManager클래스에 본 객체 등록
        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);

        InitializePlayerHUD();
    }

    /// <summary>
    /// 체력바 생성 및 HUD의 owner player 초기화
    /// </summary>
    void InitializePlayerHUD()
    {
        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();

        // 리소스 로드
        GameObject go = Resources.Load<GameObject>(PLAYER_HUD_PATH);
        // 리소스 생성
        GameObject goInstance = Instantiate(go, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity, inGameSceneMain.DamageManager.CanvasTransform);

        // 체력바 초기화
        PlayerHUD playerHUD = goInstance.GetComponent<PlayerHUD>();
        playerHUD.Initialize(this);
    }

    protected override void UpdateActor()
    {
        if (!isLocalPlayer)
            return;

        UpdateInput();
        UpdateMove();
    }

    /// <summary>
    /// 본인 클라이언트에서만 작동
    /// </summary>
    [ClientCallback]
    public void UpdateInput()
    {
        inputController.UpdateInput();
    }

    /// <summary>
    /// 이동벡터에 맞게 위를 변경
    /// </summary>
    void UpdateMove()
    {
        // x, y값이 모두 0인지 간편히 확인
        if (moveVector.sqrMagnitude == 0)
            return;

        // 정상적으로 NetworkBehavior 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdMove(moveVector);

        // Monobehaviour 인스턴스의 Update로 호출되어 실행되고 있을 때의 꼼수
        // 이 경우 클라이언트로 접속하면 Command로 보내지지만 자기자신은 CmdMove를 실행 못함
        if (isServer)
        {
            RpcMove(moveVector);    // Host 플레이어인 경우 RPC로 보냄
        }
        else
        {
            // Client 플레이어인 경우 Cmd를 호스트로 보낸 후 자신을 Self 동작
            CmdMove(moveVector);    
            if(isLocalPlayer)
                transform.position += AdjustMoveVector(moveVector);
        }

        //// 화면 밖으로 나가지 못하도록 처리
        //moveVector = AdjustMoveVector(moveVector);

        ////transform.position += moveVector;
        //CmdMove(moveVector);
    }

    /// <summary>
    /// 호스트에게 위치 전송
    /// </summary>
    /// <param name="moveVector">움직일 크기를 담은 벡터</param>
    [Command]
    public void CmdMove(Vector3 moveVector)
    {
        this.moveVector = moveVector;
        transform.position += moveVector;
        // [SyncVar] 어트리뷰트를 가진 변수의 값이 바뀌었다고 서버에 통보
        base.SetDirtyBit(1);

        // 타 플레이어가 보낸 경우 Update를 통해 초기화 되지 않으므로 사용 후 바로 초기화
        this.moveVector = Vector3.zero;
    }

    /// <summary>
    /// 클라이언트에게 위치 전송
    /// </summary>
    /// <param name="moveVector"></param>
    [ClientRpc]
    public void RpcMove(Vector3 moveVector)
    {
        this.moveVector = moveVector;
        transform.position += AdjustMoveVector(this.moveVector);
        base.SetDirtyBit(1);

        // 타 플레이어가 보낸 경우 Update를 통해 초기화 되지 않으므로 사용 후 바로 초기화
        this.moveVector = Vector3.zero;
    }

    /// <summary>
    /// 이동방향에 맞게 이동벡터를 계산
    /// </summary>
    /// <param name="moveDirection">이동 방향</param>
    public void ProcessInput(Vector3 moveDirection)
    {
        moveVector = moveDirection * speed * Time.deltaTime;
    }

    /// <summary>
    /// 박스 콜라이더를 통해 화면 밖으로 나가는지 확인
    /// </summary>
    /// <param name="moveVector">이동벡터</param>
    /// <returns></returns>
    Vector3 AdjustMoveVector(Vector3 moveVector)
    {
        Transform mainBGQuadTransform = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().MainBGQuadTransform;

        Vector3 result = Vector3.zero;

        result = boxCollider.transform.position + boxCollider.center + moveVector;

        // 왼쪽
        if (result.x - boxCollider.size.x * 0.5f < -mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;
        // 오른쪽
        if (result.x + boxCollider.size.x * 0.5f > mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;
        // 아래쪽
        if (result.y - boxCollider.size.y * 0.5f < -mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;
        // 위쪽
        if (result.y + boxCollider.size.y * 0.5f > mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        return moveVector;
    }

    /// <summary>
    /// 콜라이더에 들어왔을 때 충돌 체크
    /// </summary>
    /// <param name="other">충돌체 정보를 담고 있는 객체</param>
    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();

        // 에너미 객체에 충돌되었다고 전달
        if (enemy)
        {
            if(!enemy.IsDead)
            {
                BoxCollider box = (BoxCollider)other;
                // 박스 콜라이더가 얼마나 떨어져있을지 모르니 더해줌
                Vector3 crashPos = enemy.transform.position + box.center;
                // 구한 중점좌표에서 박스 콜라이더의 x사이즈의 절반을 더해 충돌지점(앞쪽)을 산출
                crashPos.x += box.size.x * 0.5f;

                enemy.OnCrash(crashDamage, crashPos);
            }
                
        }
    }

    /// <summary>
    /// 발사 처리
    /// </summary>
    public void Fire()
    {
        if (IsDead)
            return;

        // 호스트인지 검사
        if (host)
        {
            // 총알매니저 클래스를 통해 캐싱된 총알오브젝트 사용
            Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PLAYER_BULLET_INDEX, fireTransform.position);
            bullet.Fire(actorInstanceID, fireTransform.right, bulletSpeed, damage);
        }
        else
        {
            CmdFire(actorInstanceID, fireTransform.position, fireTransform.right, bulletSpeed, damage);
        }
    }
    
    /// <summary>
    /// 클라이언트에서 총알이 발사되었다는 걸 호스트에게 전송
    /// </summary>
    /// <param name="ownerInstanceID"></param>
    /// <param name="firePosition"></param>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    /// <param name="damage"></param>
    [Command]
    public void CmdFire(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        // 총알매니저 클래스를 통해 캐싱된 총알오브젝트 사용
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PLAYER_BULLET_INDEX, firePosition);
        bullet.Fire(actorInstanceID, fireTransform.right, bulletSpeed, damage);

        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 폭탄 발사 처리
    /// </summary>
    public void FireBomb()
    {
        // 아이템 사용 카운트가 남았을 때만 처리
        if (usableItemCount <= 0)
            return;

        // 호스트인 경우
        if (host)
        {
            // 폭탄 프리팹 생성 후 발사 함수 호출
            Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PLAYER_BOMB_INDEX, fireTransform.position);
            bullet.Fire(actorInstanceID, fireTransform.right, bulletSpeed, damage);
        }
        else // 클라이언트의 경우
        {
            CmdFireBomb(actorInstanceID, fireTransform.position, fireTransform.right, bulletSpeed, damage);
        }

        // 사용 후 감소 처리
        DecreaseUsableItemCount();
    }

    /// <summary>
    /// 서버에게 발사 정보 전송
    /// </summary>
    [Command]
    public void CmdFireBomb(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        // 폭탄 프리팹 생성 후 발사 함수 호출
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PLAYER_BOMB_INDEX, firePosition);
        bullet.Fire(actorInstanceID, fireTransform.right, bulletSpeed, damage);

        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 아이템 사용 가능 횟수 차감
    /// </summary>
    void DecreaseUsableItemCount()
    {
        // 정상적으로 NetworkBehavior 인스턴스의 Update로 호출되어 실행되고 있을때
        //CmdDecreaseUsableItemCount();

        // Monobehaviour 인스턴스의 Update로 호출되어 실행되고 있을 때의 꼼수
        // 이 경우 클라이언트로 접속하면 Command로 보내지지만 자기자신은 CmdMove를 실행 못함
        if (isServer)
        {
            RpcCmdDecreaseUsableItemCount();    // Host 플레이어인 경우 RPC로 보냄
        }
        else
        {
            // Client 플레이어인 경우 Cmd를 호스트로 보낸 후 자신을 Self 동작
            CmdDecreaseUsableItemCount();
            if (isLocalPlayer)
                usableItemCount--;
        }
    }

    /// <summary>
    /// 서버에 변경 정보 전송
    /// </summary>
    [Command]
    public void CmdDecreaseUsableItemCount()
    {
        usableItemCount--;
        SetDirtyBit(1);
    }

    /// <summary>
    /// 클라이언트에 변경 정보 전송
    /// </summary>
    [ClientRpc]
    public void RpcCmdDecreaseUsableItemCount()
    {
        usableItemCount--;
        SetDirtyBit(1);
    }

    /// <summary>
    /// 체력 감소 처리
    /// </summary>
    /// <param name="attacker">공격자 정보를 담고 있는 객체</param>
    /// <param name="value">데미지값</param>
    protected override void DecreaseHP(int value, Vector3 damagePos)
    {
        base.DecreaseHP(value, damagePos);

        //// 형변환을 통해 등록된 PlayerStatePanel을 가져옴
        //PlayerStatePanel playerStatePanel = PanelManager.GetPanel(typeof(PlayerStatePanel)) as PlayerStatePanel;
        //// 슬라이더에 체력 정보 설정
        //playerStatePanel.SetHP(currentHP, maxHP);

        // 주변에 1m 어딘가에 데미지텍스트 생성
        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Generate(DamageManager.ENEMY_DAMAGE_INDEX, damagePoint, value, Color.red);
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected override void OnDead()
    {
        base.OnDead();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 호스트 동기화 관련
    /// </summary>
    [ClientRpc]
    public void RpcSetHost()
    {
        host = true;
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 체력을 증가 시켜주는 함수
    /// </summary>
    /// <param name="value">증가할 체력 수치</param>
    protected virtual void InternalIncreaseHP(int value)
    {
        if (isDead)
            return;

        currentHP += value;

        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    /// <summary>
    /// 체력을 증가시켜주는 함수
    /// </summary>
    /// <param name="value">증가할 체력</param>
    public virtual void IncreaseHP(int value)
    {
        if (isDead)
            return;

        CmdIncreaseHP(value);
    }

    /// <summary>
    /// 서버에 체력이 증가했다고 전송
    /// </summary>
    /// <param name="value">증가할 체력값</param>
    [Command]
    public void CmdIncreaseHP(int value)
    {
        InternalIncreaseHP(value);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 사용가능 아이템을 증가시켜주는 함수
    /// </summary>
    /// <param name="value"></param>
    public virtual void IncreaseUsableItem(int value = 1)
    {
        if (isDead)
            return;

        CmdIncreaseUsableItem(value);
    }

    /// <summary>
    /// 서버에 사용 가능 아이템이 증가했다고 전송
    /// </summary>
    /// <param name="value"></param>
    [Command]
    public void CmdIncreaseUsableItem(int value)
    {
        usableItemCount += value;
        base.SetDirtyBit(1);
    }
}
