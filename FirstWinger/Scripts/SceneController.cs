using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 씬의 이름을 상수화 하기 위한 클래스
public class SceneNameConstants
{
    public const string titleScene = "TitleScene";
    public const string loadingScene = "LoadingScene";
    public const string inGame = "InGame";
}

public class SceneController : MonoBehaviour
{
    #region Singletone
    private static SceneController instance = null;
    // 따로 인스펙터에서 참조하는 값이 없으므로 프로퍼티에서 게임오브젝트를 생성할 수있도록 구현
    public static SceneController Instance
    {
        get
        {
            if(instance == null)
            {
                // 최초 사용 시 클래스명과 같은 이름의 게임오브젝트를 만들어서 어태치하여 사용
                GameObject go = GameObject.Find("SceneController");
                if(go == null)
                {
                    go = new GameObject("SceneController");

                    SceneController sceneController = go.AddComponent<SceneController>();
                    return sceneController;
                }
                else
                {
                    instance = go.GetComponent<SceneController>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("Can't have two instance of singletone.");
            DestroyImmediate(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);    // 씬이동시에도 파괴 되지 않도록
    }
    #endregion

    private void Start()
    {
        /// Scene 변화에 따른 이벤트 메소드를 매핑
        // 현재 활성화된 씬이 바뀔 때 받는 이벤트 메소드
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        // 씬이 로드되었을 때 받는 이벤트 메소드
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 씬이 언로드되었을 때 받는 이벤트 메소드
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    /// <summary>
    /// 동기 로드
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadSceneImmediate(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 이전 Scene을 Unload하고 로딩
    /// </summary>
    /// <param name="sceneName">로딩할 씬이름</param>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, LoadSceneMode.Single));
    }

    /// <summary>
    /// 이전 Scene의 Unload없이 로딩
    /// </summary>
    /// <param name="sceneName">로딩할 씬이름</param>
    public void LoadSceneAdditive(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, LoadSceneMode.Additive));
    }

    /// <summary>
    /// 비동기 로드
    /// </summary>
    /// <param name="sceneName">비동기 로드할 씬이름</param>
    /// <param name="loadSceneMode">씬을 어떻게 로드할지 모드 지정</param>
    IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

        // 씬로드가 끝나면 isDone이 True로 바뀜
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        Debug.Log("LoadSceneAsync is complete.");
    }

    /// <summary>
    /// 현재 활성화된 씬이 바뀔 때 받는 메소드
    /// </summary>
    /// <param name="scene0">LoadSceneMode.Additive로 동작할 때의 현재 씬</param>
    /// <param name="scene1">로드된 씬</param>
    public void OnActiveSceneChanged(Scene scene0, Scene scene1)
    {
        Debug.Log("OnActiveSceneChanged is called! scene0: " + scene0.name + " scene1: " + scene1.name);
    }

    /// <summary>
    /// 씬이 로드되었을 때 받는 메소드
    /// </summary>
    /// <param name="scene">로드된 씬</param>
    /// <param name="loadSceneMode">로드 모드</param>
    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("OnSceneLoaded is called! scene: " + scene.name + " LoadSceneMode: " + loadSceneMode.ToString());

        // 불려온 씬으로 현재 씬 객체 초기화
        BaseSceneMain baseSceneMain = GameObject.FindObjectOfType<BaseSceneMain>();
        Debug.Log("OnSceneLoaded! baseSceneMain.name: " + baseSceneMain.name);
        SystemManager.Instance.CurrentSceneMain = baseSceneMain;
    }

    /// <summary>
    /// 씬이 언로드되었을 때 받는 이벤트 메소드
    /// </summary>
    /// <param name="scene">언로드된 씬</param>
    public void OnSceneUnloaded(Scene scene)
    {
        Debug.Log("OnSceneUnloaded is called! scene: " + scene.name);
    }
}
