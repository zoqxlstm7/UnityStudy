using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 직렬화
// 캐싱할 프리팹의 정보를 담을 클래스
[System.Serializable]
public class PrefabCacheData
{
    public string filePath; // 프리팹 경로
    public int cacheCount;  // 얼마만큼 저장할지
}

public class PrefabCacheSystem
{
    // 캐싱된 정보를 담을 Dictionary
    Dictionary<string, Queue<GameObject>> caches = new Dictionary<string, Queue<GameObject>>();

    /// <summary>
    /// 캐시 생성 함수
    /// </summary>
    /// <param name="filePath">파일 위치</param>
    /// <param name="gameObject">캐싱할 게임오브젝트</param>
    /// <param name="cacheCount">몇개를 만들것인지</param>
    /// <param name="parentTransform">부모가 될 객체</param> 
    public void GenerateCache(string filePath, GameObject gameObject, int cacheCount, Transform parentTransform = null)
    {
        // 파일경로의 키값이 저장되어있다면 리턴
        if (caches.ContainsKey(filePath))
        {
            Debug.LogWarning("Already cache generated! filePath: " + filePath);
            return;
        }// 캐싱되어있지 않다면
        else
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            // cacheCount 만큼 게임 오브젝트 생성
            for (int i = 0; i < cacheCount; i++)
            {
                GameObject go = Object.Instantiate(gameObject, parentTransform);
                go.SetActive(false);
                queue.Enqueue(go);

                // 에너미 객체라면 서버에 스폰 명령 실행
                Enemy enemy = go.GetComponent<Enemy>();
                if(enemy != null)
                {
                    enemy.FilePath = filePath;
                    NetworkServer.Spawn(go);

                    //enemy.RpcSetActive(false);
                }

                // 총알 객체라면 서버에 스폰 명령 실행
                Bullet bullet = go.GetComponent<Bullet>();
                if(bullet != null)
                {
                    bullet.FilePath = filePath;
                    NetworkServer.Spawn(go);
                }

                // 아이템 박스 객체라면 서버에 스폰 명령 실행
                ItemBox item = go.GetComponent<ItemBox>();
                if(item != null)
                {
                    item.FilePath = filePath;
                    NetworkServer.Spawn(go);
                }
            }

            // 데이터를 저장
            caches.Add(filePath, queue);
        }
    }

    /// <summary>
    /// 캐싱된 데이터 사용 처리
    /// </summary>
    /// <param name="filePath">파일경로를 이용한 키값</param>
    /// <param name="position">지정할 위치값</param>
    /// <returns>캐싱된 오브젝트 반환</returns>
    public GameObject Archive(string filePath, Vector3 position)
    {
        // 키값이 있는지 검사
        if (!caches.ContainsKey(filePath))
        {
            Debug.LogError("Archive Error! no cache generated. filePath: " + filePath);
            return null;
        }

        // queue에 남아있는 오브젝트가 없다면 warning 처리
        if(caches[filePath].Count == 0)
        {
            Debug.LogWarning("Archive problem! not enough count.");
            return null;
        }

        // 키값에 해당하는 데이터 반환
        GameObject go = caches[filePath].Dequeue();
        go.SetActive(true);
        go.transform.position = position;
        // 서버인 경우
        if(((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {
            // 사용할 때 클라이언트에게 active를 Rpc를 통해 알림
            Enemy enemy = go.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetPosition(position);
                enemy.RpcSetActive(true);
            }

            // 사용할 때 클라이언트에게 active를 Rpc를 통해 알림
            Bullet bullet = go.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetPosition(position);
                bullet.RpcSetActive(true);
            }

            // 사용할 때 클라이언트에게 active를 Rpc를 통해 알림
            ItemBox item = go.GetComponent<ItemBox>();
            if (item != null)
            {
                item.RpcSetPosition(position);
                item.RpcSetActive(true);
            }
        }

        return go;
    }

    /// <summary>
    /// 사용한 오브젝트 캐시로 반환
    /// </summary>
    /// <param name="filePath">파일경로를 이용한 키값</param>
    /// <param name="gameObject">반환할 오브젝트</param>
    /// <returns>반환 성공 여부</returns>
    public bool Restore(string filePath, GameObject gameObject)
    {
        // 키값이 존재하는지 확인
        if (!caches.ContainsKey(filePath))
        {
            Debug.LogError("Restore Error! no cache generated. filePath: " + filePath);
            return false;
        }

        // 비활성화 후 caches로 오브젝트 반환 처리
        gameObject.SetActive(false);

        // 서버인 경우
        if (((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {
            // 클라이언트에게 active를 Rpc를 통해 알림
            Enemy enemy = gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.RpcSetActive(false);
            }

            // 클라이언트에게 active를 Rpc를 통해 알림
            Bullet bullet = gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.RpcSetActive(false);
            }

            // 클라이언트에게 active를 Rpc를 통해 알림
            ItemBox item = gameObject.GetComponent<ItemBox>();
            if (item != null)
            {
                item.RpcSetActive(false);
            }
        }

        caches[filePath].Enqueue(gameObject);
        return true;
    }

    /// <summary>
    /// 외부에서 cache를 추가할 수 있도록 하는 함수
    /// </summary>
    /// <param name="filePath">파일 경로</param>
    /// <param name="gameObject">추가할 게임오브젝트</param>
    public void Add(string filePath, GameObject gameObject)
    {
        Queue<GameObject> queue;

        // 키값 확인
        if (caches.ContainsKey(filePath))
        {
            // 캐시가 생성되어있다면 캐싱도어있는 오브젝트를 큐에 적재
            queue = caches[filePath];
        }
        else
        {
            // 캐시가 생성되어있지 않다면 큐를 생성 후 적재
            queue = new Queue<GameObject>();
            caches.Add(filePath, queue);
        }

        queue.Enqueue(gameObject);
    }
}
