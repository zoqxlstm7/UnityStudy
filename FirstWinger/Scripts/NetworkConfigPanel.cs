using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkConfigPanel : BasePanel
{
    const string DEFAULT_IP_ADDRESS = "localhost";  // 기본으로 설정될 아이피
    const string DEFAULT_PORT = "7777";             // 기본으로 설정될 포트

    [SerializeField]
    InputField ipAddressInputField;     // 아이피 인풋필드 참조
    [SerializeField]
    InputField portInputField;          // 포트 인풋필드 참조

    /// <summary>
    /// 패널 등록 및 기본정보 초기화
    /// </summary>
    public override void InitializePanel()
    {
        base.InitializePanel();
        
        // IP와 Port입력을 기본 값으로 세팅한다.
        ipAddressInputField.text = DEFAULT_IP_ADDRESS;
        portInputField.text = DEFAULT_PORT;

        Close();
    }

    /// <summary>
    /// 호스트 버튼이 눌렸을 때의 처리
    /// </summary>
    public void OnHostButton()
    {
        // 호스트로 실행되었음을 알림
        SystemManager.Instance.ConnectionInfo.host = true;

        // 씬이동
        TitleSceneMain sceneMain = SystemManager.Instance.GetCurrentSceneMain<TitleSceneMain>();
        sceneMain.GotoNextScene();
    }

    public void OnClientButton()
    {
        // 클라이언트로 실행되었음을 알림
        SystemManager.Instance.ConnectionInfo.host = false;

        // 인풋필드가 비어있지 않거나 기본 아이피와 같지 않으면 IP 입력
        if (!string.IsNullOrEmpty(ipAddressInputField.text) || ipAddressInputField.text != DEFAULT_IP_ADDRESS)
            SystemManager.Instance.ConnectionInfo.ipAdress = ipAddressInputField.text;

        // 인풋필드가 비어있지 않거나 기본 포트와 같지 않으면
        if (!string.IsNullOrEmpty(portInputField.text) || portInputField.text != DEFAULT_PORT)
        {
            int port = 0;

            // 입력된 포트번호가 int값으로 캐스팅할 수 있는지 검사 후 정상이라면 port입력
            if(int.TryParse(portInputField.text, out port))
            {
                SystemManager.Instance.ConnectionInfo.port = port;
            }
            else
            {
                Debug.Log("OnClientButton error! port: " + portInputField.text);
                return;
            }
        }

        // 씬이동
        TitleSceneMain sceneMain = SystemManager.Instance.GetCurrentSceneMain<TitleSceneMain>();
        sceneMain.GotoNextScene();
    }
}
