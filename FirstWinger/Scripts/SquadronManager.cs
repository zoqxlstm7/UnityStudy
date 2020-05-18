using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//// 스쿼드론 정보를 담을 클래스
//[System.Serializable]
//public class SquadronData
//{
//    public float squadronGenerateTime;  // 생성 시간
//    public Squadron squadron;           // 편대
//}

public class SquadronManager : MonoBehaviour
{
    float gameStartTime;    // 게임 시작 시간
    int scheduleIndex;      // 생성할 편대 인덱스

    [SerializeField]
    SquadronTable[] squadronDatas;    // 테이블에 담긴 편대 정보
    //SquadronData[] squadronDatas;   // 편대 정보
    [SerializeField]
    SquadronScheduleTable SquadronScheduleTable;    // 편대 출현 정보를 관리하는 클래스

    bool running = false;           // 게임이 가동중인지 검사

    bool allSquadronGenerated = false;  // 모든 편대가 생성되었는지 유무
    bool showWarningUICalled = false;   // 워닝 UI가 호출되었는지

    private void Start()
    {
        squadronDatas = GetComponentsInChildren<SquadronTable>();
        // csv 파일 정보 로드
        for (int i = 0; i < squadronDatas.Length; i++)
        {
            squadronDatas[i].Load();
        }

        SquadronScheduleTable.Load();
    }

    private void Update()
    {
        // 모든 편대가 생성되지 않았다면
        if (!allSquadronGenerated)
        {
            // 편대를 생성할지 검사
            CheckSquadronGeneratings();
        }// 모든 편대가 생성되고 워닝 텍스트가 호출되지 않았다면
        else if (!showWarningUICalled)
        {
            InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
            // 에너미 객체가 모두 사라졌거나 파괴되었다면 워닝UI 호출
            if (inGameSceneMain.EnemyManager.GetEnemyListCount() == 0)
            {
                inGameSceneMain.ShowWarningUI();
                showWarningUICalled = true;
            }
        }
        
    }

    /// <summary>
    /// 게임시작 시 초기화 작업
    /// </summary>
    public void StartGame()
    {
        gameStartTime = Time.time;  // 게임 시작 시간 기록
        scheduleIndex = 0;          // 생성할 편대 인덱스 지정
        running = true;             // 게임 가동 중 지정 

        Debug.Log("Game Started!");
    }

    /// <summary>
    /// 편대를 생성할지 검사 후 처리
    /// </summary>
    void CheckSquadronGeneratings()
    {
        // 게임이 가동 중이 아니라면 리턴
        if (!running)
            return;

        // 편대 출현 정보를 반환받음
        SquadronScheduleDataStruct data = SquadronScheduleTable.GetScheduleData(scheduleIndex);

        // 게임 시작 후 생성시간이 되면
        //if(Time.time - gameStartTime >= squadronDatas[squadronIndex].squadronGenerateTime)
        if (Time.time - gameStartTime >= data.generateTime)
        {
            // ID값을 통해 편대 출현
            GenerateSquadron(squadronDatas[data.squadronID]);
            scheduleIndex++;

            // 인덱스가 길이와 같아지거나 초과하면 모든 편대 생성
            if(scheduleIndex >= SquadronScheduleTable.GetDataCount())
            {
                OnAllSquadronGenerated();
                return;
            }
        }
    }

    /// <summary>
    /// 편대 생성
    /// </summary>
    /// <param name="data">생성할 편대 정보</param>
    void GenerateSquadron(SquadronTable table/*SquadronData data*/)
    {
        Debug.Log("GenerateSquadron: " + scheduleIndex);

        //data.squadron.GenerateAllData();
        // 캐싱되어있는 편대 생성
        for (int i = 0; i < table.GetCount(); i++)
        {
            SquadronMemberStruct squadronMember = table.GetSquardronMember(i);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.GenerateEnemy(squadronMember);
        }
    }

    /// <summary>
    /// 모든 편대가 생성되었을 때의 처리
    /// </summary>
    void OnAllSquadronGenerated()
    {
        Debug.Log("AllSquadronGenerated");
        running = false;

        allSquadronGenerated = true;
    }
}
