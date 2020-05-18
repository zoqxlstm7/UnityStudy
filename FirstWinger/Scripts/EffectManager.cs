using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public const int BULLET_DISAPPEAR_FX_INDEX = 0; // 총알 이펙트 인덱스
    public const int ACTOR_DEAD_FX_INDEX = 1;       // 기체 폭발 이펙트 인덱스
    public const int BOMB_EXPLODE_FX_INDEX = 2;     // 폭탄 이펙트 인덱스

    [SerializeField]
    PrefabCacheData[] effectFiles;  // 이펙트 프리팹 파일 정보를 담을 배열

    // 캐싱된 데이터를 담을 변수
    Dictionary<string, GameObject> fileCache = new Dictionary<string, GameObject>();

    private void Start()
    {
        Prepare();
    }

    /// <summary>
    /// 이펙트 생성 처리
    /// </summary>
    /// <param name="index">꺼내올 이펙트 인덱스</param>
    /// <param name="position">이펙트가 생성될 위치</param>
    /// <returns>생성된 이펙트 오브젝트 반환</returns>
    public GameObject GenerateEffect(int index, Vector3 position)
    {
        // 인덱스 범위를 벗어나는지 체크
        if(index < 0 || index >= effectFiles.Length)
        {
            Debug.LogError("GenerateEffect error! out of range. index = " + index);
            return null;
        }

        // 이펙트 생성
        string filePath = effectFiles[index].filePath;
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.Archive(filePath, position);

        // 이펙트 삭제시 사용할 파일 경로를 넣어줌
        AutoCacheableEffect effect = go.GetComponent<AutoCacheableEffect>();
        effect.FilePath = filePath;

        return go;
    }

    /// <summary>
    /// 캐시를 확인하여 이펙트 오브젝트 로드
    /// </summary>
    /// <param name="filePath">불러올 프리팹 파일 경로</param>
    /// <returns>로드된 게임오브젝트 반환</returns>
    public GameObject Load(string filePath)
    {
        GameObject go = null;

        // 캐시 확인
        if (fileCache.ContainsKey(filePath))
        {
            go = fileCache[filePath];
        }
        else
        {
            // 캐시에 없으므로 로드
            go = Resources.Load<GameObject>(filePath);
            if (!go)
            {
                Debug.LogError("Load error! path: " + filePath);
                return null;
            }

            // 로드 후 캐시에 적재
            fileCache.Add(filePath, go);
        }

        return go;
    }

    /// <summary>
    /// 미리 캐시를 만듦
    /// </summary>
    public void Prepare()
    {
        for (int i = 0; i < effectFiles.Length; i++)
        {
            GameObject go = Load(effectFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.GenerateCache(effectFiles[i].filePath, go, effectFiles[i].cacheCount, this.transform);
        }
    }

    /// <summary>
    /// 이펙트 삭제
    /// </summary>
    /// <param name="effect">삭제할 이펙트 객체</param>
    /// <returns>삭제 유무 반환</returns>
    public bool RemoveEffect(AutoCacheableEffect effect)
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectCacheSystem.Restore(effect.FilePath, effect.gameObject);
        return true;
    }
}
