using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public const int PLAYER_BULLET_INDEX = 0;   // 플레이어 총알 정보 인덱스
    public const int ENEMY_BULLET_INDEX = 1;    // 에너미 총알 정보 인덱스
    public const int PLAYER_BOMB_INDEX = 2;     // 폭탄 정보 인덱스
    public const int BOSS_BULLET_INDEX = 3;     // 보스 총알 인덱스
    public const int GUIDED_MISSILE_INDEX = 4;  // 유도 미사일 인덱스

    [SerializeField]
    PrefabCacheData[] bulletFiles;  // 총알 캐시 정보

    // 캐싱된 총알 객체를 담을 변수
    Dictionary<string, GameObject> fileCache = new Dictionary<string, GameObject>();

    private void Start()
    {
        // 캐싱된 오브젝트 생성
        //Prepare();
    }

    /// <summary>
    /// 파일 경로에 따른 리소스 오브젝트 로드
    /// </summary>
    /// <param name="filePath">오브젝트 파일 경로</param>
    /// <returns>로드된 오브젝트 반환</returns>
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
                Debug.LogError("Load Error! path: " + filePath);
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
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return;

        for (int i = 0; i < bulletFiles.Length; i++)
        {
            GameObject go = Load(bulletFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.GenerateCache(bulletFiles[i].filePath, go, bulletFiles[i].cacheCount, this.transform);
        }
    }

    /// <summary>
    /// 캐싱된 총알 오브젝트를 반환받아 총알 오브젝트 생성
    /// </summary>
    /// <param name="index">반환받을 총알정보 인덱스</param>
    /// <param name="position">생성할 위치</param>
    /// <returns></returns>
    public Bullet Generate(int index, Vector3 position)
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return null;

        string filePath = bulletFiles[index].filePath;
        // 캐싱되어있는 오브젝트를 받아옴
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.Archive(filePath, position);

        // 총알 오브젝트 파일경로 저장
        Bullet bullet = go.GetComponent<Bullet>();

        // 캐시시스템에서 경로를 추가해 줄것이므로 주석처리
        //bullet.FilePath = filePath;

        return bullet;
    }

    /// <summary>
    /// 생성된 총알 오브젝트 삭제
    /// </summary>
    /// <param name="bullet">반환할 총알 오브젝트</param>
    /// <returns></returns>
    public bool Remove(Bullet bullet)
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return true;

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.Restore(bullet.FilePath, bullet.gameObject);
        return true;
    }
}
