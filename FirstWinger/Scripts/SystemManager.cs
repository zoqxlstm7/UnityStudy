using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    #region Singletone
    // 프로퍼티로 처리
    public static SystemManager Instance { get; private set; } = null;

    private void Awake()
    {
        // 이후에 생기는 시스템매니저 객체는 제거
        if (Instance != null)
        {
            Debug.LogError("SystemManager Error! Singleton error.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Scene 이동간에 사라지지 않도록 처리
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        // 현재 씬 관리 객체 초기화
        BaseSceneMain baseSceneMain = GameObject.FindObjectOfType<BaseSceneMain>();
        currentSceneMain = baseSceneMain;
    }


    [SerializeField]
    EnemyTable enemyTable;
    // 프로퍼티로 에너미 테이블 객체 반환
    public EnemyTable EnemyTable
    {
        get => enemyTable;
    }

    // 아이템 테이블 객체
    [SerializeField]
    ItemTable itemTable;
    public ItemTable ItemTable
    {
        get => itemTable;
    }

    // 아이템 드랍 확률 테이블 객체
    [SerializeField]
    ItemDropTable itemDropTable;
    public ItemDropTable ItemDropTable
    {
        get => itemDropTable;
    }

    // 현재 씬의 정보를 담고 있는 Scene클래스
    BaseSceneMain currentSceneMain;
    public BaseSceneMain CurrentSceneMain
    {
        set
        {
            currentSceneMain = value;
        }
    }

    // 클라이언트의 네트워크를 담고 있는 클래스
    [SerializeField]
    NetworkConnectionInfo connectionInfo = new NetworkConnectionInfo();
    public NetworkConnectionInfo ConnectionInfo
    {
        get => connectionInfo;
    }

    /// <summary>
    /// 현재 씬을 반환하는 함수
    /// </summary>
    /// <typeparam name="T">BaseSceneMain을 상속받은 Scene클래스</typeparam>
    /// <returns>현재 씬정보를 담고 있는 클래스</returns>
    public T GetCurrentSceneMain<T>() where T : BaseSceneMain
    {
        return currentSceneMain as T;
    }
}
