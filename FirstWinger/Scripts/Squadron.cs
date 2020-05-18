using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 생성 정보를 담을 클래스
[System.Serializable]
public class EnemyGenerateData
{
    public string filePath;     // 파일 경로
    public int maxHP;           // 최대 체력
    public int damage;          // 총알 데미지
    public int crashDamage;     // 충돌 데미지
    public float bulletSpeed;   // 총알 속도
    public int fireRemainCount; // 남은 총알
    public int gamePoint;       // 획득할 게임 점수

    public Vector3 generatePoint;   // 입장전 생성 위치
    public Vector3 appearPoint;     // 입장시 도착 위지
    public Vector3 disappearPoint;  // 퇴장시 목표 위치
}

public class Squadron : MonoBehaviour
{
    [SerializeField]
    EnemyGenerateData[] enemyGenerateDatas; // 적 정보를 담을 배열

    /// <summary>
    /// 편대 생성
    /// </summary>
    public void GenerateAllData()
    {
        for (int i = 0; i < enemyGenerateDatas.Length; i++)
        {
            // 이클래스가 현재 불필요해서 에러방지 주석처리
            //SystemManager.Instance.EnemyManager.GenerateEnemy(enemyGenerateDatas[i]);
        }
    }
}
