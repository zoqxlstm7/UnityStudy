using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    EnemyFactory enemyFactory;

    [SerializeField]
    List<Enemy> enemies = new List<Enemy>();    // 에너미를 관리할 리스트
    public List<Enemy> Enemies
    {
        get => enemies;
    }

    [SerializeField]
    PrefabCacheData[] enemyFiles;   // 캐싱할 데이터 정보

    private void Start()
    {
        // 클라이언트로 들어온 플레이어는 Prepare() 함수가 호출되지 않고
        // 서버로 부터 전송을 받음
        //Prepare();
    }

    /// <summary>
    /// 에너미 편대 생성
    /// </summary>
    /// <param name="data">생성할 때 사용할 편대 정보</param>
    /// <returns>생성되었는지 여부</returns>
    public bool GenerateEnemy(SquadronMemberStruct data)
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return true;

        // 테이블에 저장된 에너미 파일경로 반환
        string filePath = SystemManager.Instance.EnemyTable.GetEnemy(data.enemyID).filePath;
        // 캐싱되어 있는 에너미 오브젝트를 얻어옴
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.Archive(filePath, new Vector3(data.generatePointX, data.generatePointY, 0));

        Enemy enemy = go.GetComponent<Enemy>();
        enemy.Reset(data);
        enemy.AddList();
        //enemies.Add(enemy);
        return true;
    }

    /// <summary>
    /// 에너미 반환 처리
    /// </summary>
    /// <param name="enemy">반환할 에너미 객체</param>
    /// <returns></returns>
    public bool RemoveEnemy(Enemy enemy)
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return true;

        // 리스트에 존재하는 객체인지 확인
        if (!enemies.Contains(enemy))
        {
            Debug.LogError("No exist enemy.");
            return false;
        }

        // 리스트에서 삭제 후 캐시로 반환
        //enemies.Remove(enemy);
        enemy.RemoveList();
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.Restore(enemy.FilePath, enemy.gameObject);

        return true;
    }

    /// <summary>
    /// 미리 캐시를 만듦
    /// </summary>
    public void Prepare()
    {
        // 서버가 아니면 접근하지 못하도록
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return;

        for (int i = 0; i < enemyFiles.Length; i++)
        {
            GameObject go = enemyFactory.Load(enemyFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyCacheSystem.GenerateCache(enemyFiles[i].filePath, go, enemyFiles[i].cacheCount, this.transform);
        }
    }

    /// <summary>
    /// 외부에서 에너미 리스트에 추가
    /// </summary>
    /// <param name="enemy">추가할 에너미 객체</param>
    /// <returns>추가 여부</returns>
    public bool AddList(Enemy enemy)
    {
        // 같은 객체가 있는지 검사
        if (enemies.Contains(enemy))
            return false;

        enemies.Add(enemy);
        return true;
    }

    /// <summary>
    /// 외부에서 에너미 리스트에서 삭제
    /// </summary>
    /// <param name="enemy">삭제할 에너미 객체</param>
    /// <returns>삭제 여부</returns>
    public bool RemoveList(Enemy enemy)
    {
        // 같은 객체가 있는지 검사
        if (!enemies.Contains(enemy))
            return false;

        enemies.Remove(enemy);
        return true;
    }

    /// <summary>
    /// 콜라이더에 들어온 에너미 리스트를 반환
    /// </summary>
    /// <param name="collider">콜라이더 범위 객체</param>
    /// <returns>콜라이더에 들어온 에너미 리스트</returns>
    public List<Enemy> GetContainEnemies(Collider collider)
    {
        // 들어온 객체를 담을 리스트
        List<Enemy> contains = new List<Enemy>();

        Collider enemyCollider;
        for (int i = 0; i < enemies.Count; i++)
        {
            enemyCollider = enemies[i].GetComponentInChildren<Collider>();

            if(enemyCollider == null)
            {
                Debug.LogError(enemies[i] + name + " model is not correct!");
                continue;
            }

            // 바운즈와 바운즈가 겹쳤는지 검사
            if (collider.bounds.Intersects(enemyCollider.bounds))
                contains.Add(enemies[i]);
        }

        return contains;
    }

    /// <summary>
    /// 현재 리스트에 있는 에너미 count 반환
    /// </summary>
    /// <returns></returns>
    public int GetEnemyListCount()
    {
        return enemies.Count;
    }
}
