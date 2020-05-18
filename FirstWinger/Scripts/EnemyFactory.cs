using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    // 에너미가 있는 파일 경로를 키값으로 사용
    // Resources 폴더 하위 경로를 지정
    public const string EnemyPath = "Prefabs/Enemy";

    // 캐싱을 하기 위해서 Dictionary 자료구조 사용
    Dictionary<string, GameObject> enemyFileCache = new Dictionary<string, GameObject>();

    // 게임오브젝트 로드
    public GameObject Load(string resourcePath)
    {
        GameObject go = null;

        // key값을 확인하여 캐싱이 되었다면 캐싱되어진 게임오브젝트를 가져옴
        if (enemyFileCache.ContainsKey(resourcePath))
        {
            go = enemyFileCache[resourcePath];
        }// 로드가 되지 않은 경우
        else
        {
            // Resources 클래스를 이용하여 메모리 로드
            go = Resources.Load<GameObject>(resourcePath);
            if (!go)
            {
                Debug.LogError("Load Error! path = " + resourcePath);
                return null;
            }

            // 로드가 정상적으로 이루어진 경우 캐시에 넣어준다
            enemyFileCache.Add(resourcePath, go);
        }

        return go;
    }
}
