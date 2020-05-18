using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InGameSceneMain : BaseSceneMain
{
    //const float GAME_READY_INTERVAL = 3.0f; // 게임 시작까지의 간격

    //// 게임 상태 정의
    //public enum GameState : int
    //{
    //    Ready = 0,
    //    Running,
    //    End
    //}

    // 게임 상태 정의 변수
    public GameState CurrentGameState
    {
        get => NetworkTransfer.CurrentGameState;
    }

    // 플레이어 객체
    [SerializeField]
    Player player;      
    public Player Hero
    {
        get => player;
        set => player = value;
    }

    Player otherPlayer;
    public Player OtherPlayer
    {
        get => otherPlayer;
        set => otherPlayer = value;
    }

    // 점수 관리 객체
    GamePointAccumulator gamePointAccumulator = new GamePointAccumulator();
    public GamePointAccumulator GamePointAccumulator
    {
        get => gamePointAccumulator;
    }

    // 이펙트 관리 객체
    [SerializeField]
    EffectManager effectManager;    
    public EffectManager EffectManager
    {
        get => effectManager;
    }

    // 에너미 관리 객체
    [SerializeField]
    EnemyManager enemyManager;
    public EnemyManager EnemyManager
    {
        get => enemyManager;
    }

    // 총알 관리 객체
    [SerializeField]
    BulletManager bulletManager;
    public BulletManager BulletManager
    {
        get => bulletManager;
    }

    // 데미지 텍스트 관리 객체
    [SerializeField]
    DamageManager damageManager;
    public DamageManager DamageManager
    {
        get => damageManager;
    }

    // 아이템 박스 관리 객체
    [SerializeField]
    ItemBoxManager itemBoxManager;
    public ItemBoxManager ItemBoxManager
    {
        get => itemBoxManager;
    }

    // 에너미 캐시 데이터 관리 객체
    PrefabCacheSystem enemyCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem EnemyCacheSystem
    {
        get => enemyCacheSystem;
    }

    // 총알 캐시 데이터 관리 객체
    PrefabCacheSystem bulletCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem BulletCacheSystem
    {
        get => bulletCacheSystem;
    }

    // 이펙트 캐시 데이터 관리 객체
    PrefabCacheSystem effectCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem EffectCacheSystem
    {
        get => effectCacheSystem;
    }

    // 데미지 텍스트 캐시 데이터 관리 객체
    PrefabCacheSystem damageCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem DamageCacheSystem
    {
        get => damageCacheSystem;
    }

    // 아이템 박스 캐시 데이터 관리 객체
    PrefabCacheSystem itemBoxCacheSystem = new PrefabCacheSystem();
    public PrefabCacheSystem ItemBoxCacheSystem
    {
        get => itemBoxCacheSystem;
    }

    // 편대 관리 객체 
    [SerializeField]
    SquadronManager squadronManager;
    public SquadronManager SquadronManager
    {
        get => squadronManager;
    }

    //// 씬 시작 시간
    //float sceneStartTime;

    [SerializeField]
    Transform mainBGQuadTransform;          // 기준이 될 배경 오브젝트
    public Transform MainBGQuadTransform
    {
        get => mainBGQuadTransform;
    }

    //[SerializeField]
    //Transform playerStartTransform1;        // 플레이어1 시작 위치
    //public Transform PlayerStartTransform1
    //{
    //    get => playerStartTransform1;
    //}

    //[SerializeField]
    //Transform playerStartTransform2;        // 플레이어2 시작 위치
    //public Transform PlayerStartTransform2
    //{
    //    get => playerStartTransform2;
    //}

    [SerializeField]
    InGameNetworkTransfer inGameNetworkTransfer;    // 인게임 씬 네트워크 관리 클래스
    InGameNetworkTransfer NetworkTransfer
    {
        get => inGameNetworkTransfer;
    }

    ActorManager actorManager = new ActorManager(); // Actor 객체 인스턴스ID 관리 클래스
    public ActorManager ActorManager
    {
        get => actorManager;
    }

    [SerializeField]
    int bossEnemyID;            // 에너미 테이블 ID값 참조
    [SerializeField]    
    Vector3 bossGeneratePos;    // 보스 생성 위치
    [SerializeField]
    Vector3 bossAppearPos;      // 보스 출현 위치

    protected override void UpdateScene()
    {
        base.UpdateScene();

        if(CurrentGameState == GameState.Running)
        {
            if(Hero != null && OtherPlayer != null)
            {
                // 플레이어 둘다 죽었을 때 실패 처리
                if(Hero.IsDead && OtherPlayer.IsDead)
                {
                    // 두번 진입하지 않도록 강제로 게임종료 셋팅
                    NetworkTransfer.SetGameStateEnd();
                    OnGameEnd(false);
                }
            }
        }
    }

    /// <summary>
    /// 네트워크용 게임 시작함수
    /// </summary>
    public void GameStart()
    {
        NetworkTransfer.RpcGameStart();
    }

    /// <summary>
    /// 워닝 UI 노출
    /// </summary>
    public void ShowWarningUI()
    {
        NetworkTransfer.RpcShowWarningUI();
    }

    /// <summary>
    /// 러닝 상태로 전환
    /// </summary>
    public void SetRunningState()
    {
        NetworkTransfer.RpcSetRunningState();
    }

    /// <summary>
    /// 보스 생성에 필요한 정보 초기화
    /// </summary>
    public void GenerateBoss()
    {
        SquadronMemberStruct data = new SquadronMemberStruct();
        data.enemyID = bossEnemyID;
        data.generatePointX = bossGeneratePos.x;
        data.generatePointY = bossGeneratePos.y;
        data.appearPointX = bossAppearPos.x;
        data.appearPointY = bossAppearPos.y;
        data.disappearPointX = -15.0f;
        data.disappearPointY = 0.0f;

        // 생성 함수 호출
        enemyManager.GenerateEnemy(data);
    }

    /// <summary>
    /// 게임 종료 조건일 때 호출
    /// </summary>
    /// <param name="success">종료 유무</param>
    public void OnGameEnd(bool success)
    {
        // 서버인경우
        if(((FWNetworkManager)FWNetworkManager.singleton).isServer)
            NetworkTransfer.RpcGameEnd(success);
    }

    /// <summary>
    /// 게임 종료시 관련 객체를 마무리하고 타이틀 씬으로 이동 
    /// </summary>
    public void GotoTitleScene()
    {
        // 네트워크를 끝낸다
        FWNetworkManager.Shutdown();
        // 시스템 매니저를 파괴
        DestroyImmediate(SystemManager.Instance.gameObject);
        SceneController.Instance.LoadSceneImmediate(SceneNameConstants.titleScene);
    }

    /// <summary>
    /// 게임 시작 메소드
    /// </summary>
    //protected override void OnStart()
    //{
    //    // 씬 시작 시간 저장
    //    sceneStartTime = Time.time;
    //}

    ///// <summary>
    ///// 씬내의 업데이트 관련 처리
    ///// </summary>
    //protected override void UpdateScene()
    //{
    //    base.UpdateScene();

    //    float currentTime = Time.time;

    //    // 게임 준비 상태라면
    //    if(currentGameState == GameState.Ready)
    //    {
    //        // 일정 시간 후 게임 시작
    //        if (currentTime - sceneStartTime > GAME_READY_INTERVAL)
    //        {
    //            //squadronManager.StartGame();
    //            currentGameState = GameState.Running;
    //        }
    //    }
    //}
}
