using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneMain : BaseSceneMain
{
    protected override void OnStart()
    {
        base.OnStart();

        // 혹시 있을 네트워크 매니저 파괴
        FWNetworkManager[] fWNetworkManager = FindObjectsOfType<FWNetworkManager>();
        if(fWNetworkManager != null)
        {
            for (int i = 0; i < fWNetworkManager.Length; i++)
            {
                DestroyImmediate(fWNetworkManager[i].gameObject);
            }
        }
    }

    /// <summary>
    /// 게임 시작 버튼을 눌렀을 때 처리하는 함수
    /// </summary>
    public void OnStartButton()
    {
        // 네트워크 설정 UI를 보여주도록 설정
        PanelManager.GetPanel(typeof(NetworkConfigPanel)).Show();
    }

    /// <summary>
    /// 다음 씬으로 이동 처리
    /// </summary>
    public void GotoNextScene()
    {
        SceneController.Instance.LoadScene(SceneNameConstants.loadingScene);
    }
}
