using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
// ansi 타입으로 순서대로 구조체에 넣겠다
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

// 편대 출현 관리 정보 구조체
public struct SquadronScheduleDataStruct
{
    public int index;
    public float generateTime;
    public int squadronID;  // 생성할 편대 인덱스
}

public class SquadronScheduleTable : TableLoader<SquadronScheduleDataStruct>
{
    // 편대 관리 정보를 저장할 변수
    // 순차적으로 등장할 것이므로 List를 활용
    List<SquadronScheduleDataStruct> tableDatas = new List<SquadronScheduleDataStruct>();

    /// <summary>
    /// 편대 관리 정보 저장
    /// </summary>
    /// <param name="data">저장할 편대관리정보</param>
    protected override void AddData(SquadronScheduleDataStruct data)
    {
        tableDatas.Add(data);
    }

    /// <summary>
    /// 편대 관리 정보를 반환하는 함수
    /// </summary>
    /// <param name="index">반환받을 데이터 인덱스</param>
    /// <returns>편대 관리 정보 반환</returns>
    public SquadronScheduleDataStruct GetScheduleData(int index)
    {
        if(index < 0 || index >= tableDatas.Count)
        {
            Debug.LogError("GetScheduleData Error! index: " + index);
            return default;// default(SquadronScheduleDataStruct);
        }

        return tableDatas[index];
    }

    /// <summary>
    /// 외부에서 데이터 갯수에 접근할 수 있도록 추가함
    /// </summary>
    /// <returns>테이블의 스케쥴 데이터 갯수</returns>
    public int GetDataCount()
    {
        return tableDatas.Count;
    }
}
