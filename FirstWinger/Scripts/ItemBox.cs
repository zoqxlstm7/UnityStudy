using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemBox : NetworkBehaviour
{
    const int HP_RECOVERY_VALUE = 20;   // 체력 회복 수치
    const int SCORE_ADD_VALUE = 100;    // 점수 추가 수치

    // 아이템 상태 
    public enum ItemEffect : int
    {
        HpRecovery = 0,
        ScoreAdd,
        UsableItemAdd
    };

    [SerializeField]
    ItemEffect itemEffect = ItemEffect.HpRecovery;  // 아이템 상태 처리 변수

    [SerializeField]
    Transform selfTransform;    // 자기 자신의 transform
    [SerializeField]
    Vector3 rotateAngle = new Vector3(0.0f, 0.5f, 0.0f);    // 더해질 회전 값

    [SyncVar]
    [SerializeField]
    string filePath;                // 아이템 박스 파일 경로
    public string FilePath
    {
        get => filePath;
        set => filePath = value;
    }

    [SerializeField]
    Vector3 moveVector = Vector3.zero;  // 이동할 벡터

    private void Start()
    {
        // 서버가 아니라면
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {
            InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
            // 부모 오브젝트 지정
            transform.SetParent(inGameSceneMain.ItemBoxManager.transform);
            // 생성된 캐쉬 적재
            inGameSceneMain.ItemBoxCacheSystem.Add(filePath, gameObject);
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateRotate();
        UpdateMove();
    }

    /// <summary>
    /// 이동 처리
    /// </summary>
    void UpdateMove()
    {
        selfTransform.position += moveVector * Time.deltaTime;
    }

    /// <summary>
    /// active 상황을 클라이언트에 전달
    /// </summary>
    /// <param name="value">active 유무</param>
    [ClientRpc]
    public void RpcSetActive(bool value)
    {
        gameObject.SetActive(value);
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 위치값을 클라이언트에 전달
    /// </summary>
    /// <param name="position">위치 값</param>
    [ClientRpc]
    public void RpcSetPosition(Vector3 position)
    {
        transform.position = position;
        base.SetDirtyBit(1);
    }

    /// <summary>
    /// 회전값 업데이트
    /// </summary>
    void UpdateRotate()
    {
        // 로컬 회전값을 회전값만큼 더해 자기 자신을 중심으로 회전
        Vector3 eulerAngles = selfTransform.localRotation.eulerAngles;
        eulerAngles += rotateAngle;
        selfTransform.Rotate(eulerAngles, Space.Self);
    }

    /// <summary>
    /// 충돌 처리 이벤트 함수
    /// </summary>
    /// <param name="other">충돌한 객체</param>
    private void OnTriggerEnter(Collider other)
    {
        // 충돌 객체가 플레이어가 아니라면 리턴
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        OnItemCollision(other);
    }

    /// <summary>
    /// 아이템 충돌 처리 함수
    /// </summary>
    /// <param name="other">충돌한 객체</param>
    void OnItemCollision(Collider other)
    {
        // 플레이어 객체가 null이거나 죽었다면 리턴 처리
        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        if (player.IsDead)
            return;

        // 로컬 플레이어인 경우
        if (player.isLocalPlayer)
        {
            // 아이템 효과에 따른 능력 적용
            switch (itemEffect)
            {
                case ItemEffect.HpRecovery: // 체력 회복
                    player.IncreaseHP(HP_RECOVERY_VALUE);
                    break;
                case ItemEffect.ScoreAdd:   // 점수 증가
                    InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
                    inGameSceneMain.GamePointAccumulator.Accumulate(SCORE_ADD_VALUE);
                    break;
                case ItemEffect.UsableItemAdd:  // 사용 가능 아이템 증가
                    player.IncreaseUsableItem();
                    break;
            }
        }

        Disappear();
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    void Disappear()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ItemBoxManager.Remove(this);
    }
}
