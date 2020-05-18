using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndPanel : BasePanel
{
    [SerializeField]
    GameObject success;
    [SerializeField]
    GameObject fail;

    public override void InitializePanel()
    {
        base.InitializePanel();
        Close();
    }

    /// <summary>
    /// 게임 종료시 텍스트 표시
    /// </summary>
    /// <param name="success">미션 성공 여부</param>
    public void ShowGameEnd(bool success)
    {
        base.Show();

        // 미션 성공시 success UI 출력
        if (success)
        {
            this.success.SetActive(true);
            fail.SetActive(false);
        }// 미션 실패시 fail UI 출력
        else
        {
            this.success.SetActive(false);
            fail.SetActive(true);
        }
    }

    /// <summary>
    /// ok버튼 이벤트
    /// </summary>
    public void OnOk()
    {
        // 타이틀 씬으로 이동 함수 호출
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().GotoTitleScene();
    }
}
