using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
// ansi 타입으로 순서대로 구조체에 넣겠다
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

// 로드된 에너미 정보를 담을 구조체
public struct EnemyStruct
{
    public int index;
    // string값 사이즈 고정
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MarshalTableConstant.charBufferSize)]
    public string filePath;
    public int maxHP;
    public int damage;
    public int crashDamage;
    public int bulletSpeed;
    public int fireRemainCount;
    public int gamePoint;
    public float itemDropRate;  // 드랍 확률 참조
    public int itemDropID;      // 아이템 드랍 테이블에 있는 ID 참조
}

public class EnemyTable : TableLoader<EnemyStruct>
{
    // 읽어들인 데이터를 저장하기 위한 변수
    Dictionary<int, EnemyStruct> tableDatas = new Dictionary<int, EnemyStruct>();

    private void Start()
    {
        Load();
    }

    /// <summary>
    /// 로드한 적정보 데이터 저장
    /// </summary>
    /// <param name="data">저장할 적정보 데이터</param>
    protected override void AddData(EnemyStruct data)
    {
        //Debug.Log("data.index: " + data.index);
        //Debug.Log("data.filePath: " + data.filePath);
        //Debug.Log("data.maxHP: " + data.maxHP);
        //Debug.Log("data.damage: " + data.damage);
        //Debug.Log("data.crashDamage: " + data.crashDamage);
        //Debug.Log("data.bulletSpeed: " + data.bulletSpeed);
        //Debug.Log("data.fireRemainCount: " + data.fireRemainCount);
        //Debug.Log("data.gamePoint: " + data.gamePoint);

        tableDatas.Add(data.index, data);
    }

    /// <summary>
    /// 저장소에 저장된 적 데이터 반환
    /// </summary>
    /// <param name="index">반환 받을 적 데이터 인덱스</param>
    /// <returns></returns>
    public EnemyStruct GetEnemy(int index)
    {
        // 데이터가 있는지 확인
        if (!tableDatas.ContainsKey(index))
        {
            Debug.LogError("GetEnemy Error! index: " + index);
            return default; // default(EnemyStruct);
        }

        return tableDatas[index];
    }
}
