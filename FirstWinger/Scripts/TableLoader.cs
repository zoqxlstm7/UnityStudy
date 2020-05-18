using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TableLoader<TMarshalStruct> : MonoBehaviour
{
    [SerializeField]
    protected string filePath;  // 파일 경로

    TableRecordParser<TMarshalStruct> tableRecordParser = new TableRecordParser<TMarshalStruct>();

    /// <summary>
    /// 텍스트 에셋형태로 로드하는 함수
    /// </summary>
    /// <returns0>성공 여부</returns>
    public bool Load()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(filePath);
        if(textAsset == null)
        {
            Debug.LogError("Load Failed! filePath: " + filePath);
            return false;
        }

        ParseTable(textAsset.text);

        return true;
    }

    /// <summary>
    /// 테이블에 있는 레코드를 하나하나 읽어 파싱함수 호출
    /// </summary>
    /// <param name="text">파싱에 사용할 텍스트</param>
    void ParseTable(string text)
    {
        StringReader reader = new StringReader(text);   // System.IO.StringReader;

        string line = string.Empty;
        bool fieldRead = false;

        // 파일이 끝날 때까지 레코드 파싱
        while ((line = reader.ReadLine()) != null)
        {
            // 첫번째 레코드는 제목형태이므로 패스
            if (!fieldRead)
            {
                fieldRead = true;
                continue;
            }

            TMarshalStruct data = tableRecordParser.ParseRecordLine(line);
            AddData(data);
        }
    }

    /// <summary>
    /// 데이터를 저장하는 함수
    /// </summary>
    /// <param name="data">저장할 데이터</param>
    protected virtual void AddData(TMarshalStruct data)
    {

    }
}
