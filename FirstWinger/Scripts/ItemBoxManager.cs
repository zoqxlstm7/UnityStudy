using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxManager : MonoBehaviour
{
    [SerializeField]
    PrefabCacheData[] itemBoxFiles; // 아이템 박스 캐시 생성관련 데이터

    // 캐시 데이터를 담을 변수
    Dictionary<string, GameObject> fileCache = new Dictionary<string, GameObject>();

    /// <summary>
    /// 아이템 생성
    /// </summary>
    /// <param name="index">생성할 아이템 인덱스</param>
    /// <param name="position">생성될 아이템 위치</param>
    /// <returns>생성된 아이템 오브젝트</returns>
    public GameObject Generate(int index, Vector3 position)
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return null;

        // 인덱스 검사
        if (index < 0 || index >= itemBoxFiles.Length)
        {
            Debug.LogError("Generate error! out of range. index: " + index);
            return null;
        }

        string filePath = itemBoxFiles[index].filePath;
        // 저장된 캐시를 반환받음
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ItemBoxCacheSystem.Archive(filePath, position);

        // 아이템 객체에 파일 경로 저장
        ItemBox item = go.GetComponent<ItemBox>();
        item.FilePath = filePath;

        return go;
    }

    /// <summary>
    /// 캐시된 파일 및 캐시 로드
    /// </summary>
    /// <param name="resourcePath">로드할 리소스 경로</param>
    /// <returns>생성된 캐시 오브젝트</returns>
    public GameObject Load(string resourcePath)
    {
        GameObject go = null;

        // 캐시 확인
        if (fileCache.ContainsKey(resourcePath))
        {
            go = fileCache[resourcePath];
        }
        else
        {
            // 캐시에 없으므로 로드
            go = Resources.Load<GameObject>(resourcePath);
            if (!go)
            {
                Debug.LogError("Load error! path: " + resourcePath);
                return null;
            }

            // 로드 후 캐시에 적재
            fileCache.Add(resourcePath, go);
        }

        return go;
    }

    /// <summary>
    /// 캐시데이터 생성
    /// </summary>
    public void Prepare()
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return;

        for (int i = 0; i < itemBoxFiles.Length; i++)
        {
            GameObject go = Load(itemBoxFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ItemBoxCacheSystem.GenerateCache(itemBoxFiles[i].filePath, go, itemBoxFiles[i].cacheCount);
        }
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    /// <param name="item">제거할 아이템</param>
    /// <returns></returns>
    public bool Remove(ItemBox item)
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return true;

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ItemBoxCacheSystem.Restore(item.FilePath, item.gameObject);
        return true;
    }
}
