using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
// ansi 타입으로 순서대로 구조체에 넣겠다
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

public struct SquadronMemberStruct
{
    public int index;               // 인덱스 번호
    public int enemyID;             // 에너미 ID
    public float generatePointX;    // 생성 위치 X
    public float generatePointY;    // 생성 위치 y
    public float appearPointX;      // 입장 위치 x
    public float appearPointY;      // 입장 위치 y
    public float disappearPointX;   // 퇴장 위치 x
    public float disappearPointY;   // 퇴장 위치 y
}

public class SquadronTable : TableLoader<SquadronMemberStruct>
{
    // 편대 정보를 저장할 변수
    // 순차적으로 등장할 것이므로 List를 활용
    List<SquadronMemberStruct> tableDatas = new List<SquadronMemberStruct>();

    /// <summary>
    /// 편대 정보 저장
    /// </summary>
    /// <param name="data">저장할 편대 정보</param>
    protected override void AddData(SquadronMemberStruct data)
    {
        tableDatas.Add(data);
    }

    /// <summary>
    /// 편대 정보를 반환하는 함수
    /// </summary>
    /// <param name="index">반환받을 편대저장소 인덱스</param>
    /// <returns>편대 정보 반환</returns>
    public SquadronMemberStruct GetSquardronMember(int index)
    {
        if (index < 0 || index >= tableDatas.Count)
        {
            Debug.LogError("GetSquardronMember Error! index: " + index);
            return default; //default(SquadronMemberStruct);
        }

        return tableDatas[index];
    }

    /// <summary>
    /// table 길이 반환
    /// </summary>
    /// <returns></returns>
    public int GetCount()
    {
        return tableDatas.Count;
    }
}
