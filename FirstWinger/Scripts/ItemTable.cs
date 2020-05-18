using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

// 아이템 테이블 정보를 담을 구조체
public struct ItemStruct
{
    public int index;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MarshalTableConstant.charBufferSize)]
    public string filePath;
    public int value;
}

public class ItemTable : TableLoader<ItemStruct>
{
    // 아이템 테이블 정보를 담고 있을 변수
    Dictionary<int, ItemStruct> tableDatas = new Dictionary<int, ItemStruct>();

    private void Start()
    {
        Load();
    }

    /// <summary>
    /// 데이터 추가
    /// </summary>
    /// <param name="data">추가할 데이터</param>
    protected override void AddData(ItemStruct data)
    {
        tableDatas.Add(data.index, data);
    }

    /// <summary>
    /// 저장된 데이터를 반환하는 함수
    /// </summary>
    /// <param name="index">반환받을 키값</param>
    /// <returns>아이템 테이블 정보</returns>
    public ItemStruct GetItem(int index)
    {
        if (!tableDatas.ContainsKey(index))
        {
            Debug.LogError("GetItem Error! index: " + index);
            return default;
        }

        return tableDatas[index];
    }
}
