using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    // 데미지텍스트 인덱스
    public const int ENEMY_DAMAGE_INDEX = 0;
    public const int PLAYER_DAMAGE_INDEX = 0;

    [SerializeField]
    Transform canvasTranform;   // 부모로 넘겨줄 객체
    public Transform CanvasTransform
    {
        get => canvasTranform;
    }

    [SerializeField]
    PrefabCacheData[] files;    // 데미지텍스트 파일 정보

    // 캐싱된 오브젝트를 담을 변수
    Dictionary<string, GameObject> fileCache = new Dictionary<string, GameObject>();

    private void Start()
    {
        Prepare();
    }

    /// <summary>
    /// 캐싱할 오브젝트 로드
    /// </summary>
    /// <param name="filePath">로드할 오브젝트 파일경로</param>
    /// <returns></returns>
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
                Debug.LogError("Load Error. filePath: " + filePath);
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
        for (int i = 0; i < files.Length; i++)
        {
            GameObject go = Load(files[i].filePath);
            //생성과 동시에 canvas 밑에 들어가야 오류가 적을므로 canvas 객체를 부모할 수 있도록 매개변수를 넘겨준다.
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.GenerateCache(files[i].filePath, go, files[i].cacheCount, canvasTranform);
        }
    }

    /// <summary>
    /// 데미지텍스트 생성
    /// </summary>
    /// <param name="index">사용할 프리팹 인덱스</param>
    /// <param name="position">생성될 위치</param>
    /// <param name="damageValue">데미지 정보</param>
    /// <param name="textColor">데미지 텍스트 색상</param>
    /// <returns>캐싱된 오브젝트 반환</returns>
    public GameObject Generate(int index, Vector3 position, int damageValue, Color textColor)
    {
        // 인덱스 검사
        if(index <0 || index >= files.Length)
        {
            Debug.LogError("Generate Error! out of range. index: " + index);
            return null;
        }

        string filePath = files[index].filePath;
        // 월드좌표에서 스크린 좌표로 변환
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.Archive(filePath, Camera.main.WorldToScreenPoint(position));

        UIDamage damage = go.GetComponent<UIDamage>();
        damage.FilePath = filePath;
        // 데미지 출력
        damage.ShowDamage(damageValue, textColor);

        return go;
    }

    /// <summary>
    /// 데미지텍스트 제거
    /// </summary>
    /// <param name="damage">제거될 데미지텍스트 객체</param>
    /// <returns>삭제 여부 반환</returns>
    public bool Remove(UIDamage damage)
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageCacheSystem.Restore(damage.FilePath, damage.gameObject);
        return true;
    }
}
