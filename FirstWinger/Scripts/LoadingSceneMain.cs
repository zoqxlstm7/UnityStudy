using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneMain : BaseSceneMain
{
    const float NEXT_SCENE_INTERVAL = 0.5f;         // 다음 씬으로 이동하기 전까지 시간
    const float TEXT_UPDATE_INTERVAL = 0.15f;       // 텍스트 업데이트 간격
    const string LOADING_TEXT_VALUE = "Loading..."; // 보여질 텍스트 

    [SerializeField]
    Text loadingText;       // 참조할 텍스트 UI

    int textIndex = 0;      // 인덱스까지 텍스트를 보여주기 위한 변수
    float lastUpdateTime;   // 마지막 텍스트 업데이트 시간

    float sceneStartTime;       // Scene 이 시작된 시간
    bool nextSceneCall = false; // 다음 씬이 불렀는지 확인 (한번만 콜하기위한 플래그)

    protected override void OnStart()
    {
        // 시작된 시간 저장
        sceneStartTime = Time.time;
    }

    /// <summary>
    /// 씬내의 업데이트 처리
    /// </summary>
    protected override void UpdateScene()
    {
        base.UpdateScene();

        float currentTime = Time.time;
        // 인터벌 시간 간격마다 로딩텍스트를 한글자씩 처리
        if(currentTime - lastUpdateTime > TEXT_UPDATE_INTERVAL)
        {
            loadingText.text = LOADING_TEXT_VALUE.Substring(0, textIndex + 1);

            textIndex++;
            if(textIndex >= LOADING_TEXT_VALUE.Length)
            {
                textIndex = 0;
            }

            lastUpdateTime = currentTime;
        }

        // 일정시간 경과 후 다음 씬 호출
        if(currentTime - sceneStartTime > NEXT_SCENE_INTERVAL)
        {
            // 호출을 한번만 하기위한 플래그
            if (!nextSceneCall)
                GotoNextScene();
        }
    }

    /// <summary>
    /// 다음 씬으로 이동 처리
    /// </summary>
    void GotoNextScene()
    {
        NetworkConnectionInfo info = SystemManager.Instance.ConnectionInfo;
        if (info.host)
        {
            // 호스트로 시작
            FWNetworkManager.singleton.StartHost();
        }
        else
        {
            // IP, Port 등록
            if (!string.IsNullOrEmpty(info.ipAdress))
                FWNetworkManager.singleton.networkAddress = info.ipAdress;
            if (info.port != FWNetworkManager.singleton.networkPort)
                FWNetworkManager.singleton.networkPort = info.port;

            // 클라이언트로 시작
            FWNetworkManager.singleton.StartClient();
        }

        // 다음 씬을 콜
        nextSceneCall = true;
    }
}
