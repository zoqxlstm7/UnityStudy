using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FWNetworkManager : NetworkManager
{
    public const int WAITNG_PLAYER_COUNT = 2;   // 기다릴 플레이어 숫자
    int playerCount = 0;

    // 서버인지 정보반환
    public bool isServer { get; private set; }

    #region SERVER SIDE EVENT

    /// <summary>
    /// 서버가 연결되었을 때의 콜백 메소드
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("OnServerConnect Call: " + conn.address + ", " + conn.connectionId);
        base.OnServerConnect(conn);
    }

    /// <summary>
    /// 서버에서 씬이 바뀌었을 때 호출되는 메소드
    /// </summary>
    /// <param name="sceneName"></param>
    public override void OnServerSceneChanged(string sceneName)
    {
        Debug.Log("OnServerSceneChanged: " + sceneName);
        base.OnServerSceneChanged(sceneName);
    }

    /// <summary>
    /// 서버가 준비 되었을 때 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerReady(NetworkConnection conn)
    {
        Debug.Log("OnServerReady: " + conn.address + ", " + conn.connectionId);
        base.OnServerReady(conn);

        // 클라이언트가 연결될 때마다 플레이어수 증가
        playerCount++;

        // 플레이어 수가 충족되었다면 
        if(playerCount >= WAITNG_PLAYER_COUNT)
        {
            // 인게임씬 게임 시작
            InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
            inGameSceneMain.GameStart();
        }
    }

    /// <summary>
    /// 서버에서 에러가 난 경우 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="errorCode"></param>
    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("OnServerError: errorCode: " + errorCode);
        base.OnServerError(conn, errorCode);
    }

    /// <summary>
    /// 서버와 연결이 끊어졌을 때 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnServerDisconnect: " + conn.address);
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// 서버가 시작될 때 호출되는 함수
    /// 호스트롤 들어올 때만 호출
    /// </summary>
    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");
        base.OnStartServer();
        // 서버로 지정
        isServer = true;
    }

    #endregion

    #region CLIENT SIDE EVENT

    /// <summary>
    /// 클라이언트가 시작될 때 호출되는 함수
    /// </summary>
    /// <param name="client"></param>
    public override void OnStartClient(NetworkClient client)
    {
        Debug.Log("OnStartClient: " + client.serverIp);
        base.OnStartClient(client);
    }

    /// <summary>
    /// 클라이언트가 접속했을 때 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect. connectionId: " + conn.connectionId
            + ", hostID: " + conn.hostId);
        base.OnClientConnect(conn);
    }

    /// <summary>
    /// 클라이언트 쪽의 씬이 바뀌었을 때 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        Debug.Log("OnClientSceneChanged: " + conn.hostId);
        base.OnClientSceneChanged(conn);
    }

    /// <summary>
    /// 클라이언트 쪽에 에러가 난 경우 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="errorCode"></param>
    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("OnClientError: " + errorCode);
        base.OnClientError(conn, errorCode);
    }

    /// <summary>
    /// 클라이언트와 접속이 끊겼을 때 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnClientDisconnect: " + conn.hostId);

        if (!isServer)
        {
            InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
            if(inGameSceneMain.CurrentGameState == GameState.End)
            {
                inGameSceneMain.GotoTitleScene();
                return;
            }
        }

        base.OnClientDisconnect(conn);
    }

    /// <summary>
    /// 클라이언트가 준비가 되지 않았을 때 호출되는 함수
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientNotReady(NetworkConnection conn)
    {
        Debug.Log("OnClientNotReady: " + conn.hostId);
        base.OnClientNotReady(conn);
    }

    /// <summary>
    /// 클라이언트 접속을 강제로 끊을 때 호출되는 함수
    /// </summary>
    /// <param name="success"></param>
    /// <param name="extendedInfo"></param>
    public override void OnDropConnection(bool success, string extendedInfo)
    {
        Debug.Log("OnDropConnection: " + extendedInfo);
        base.OnDropConnection(success, extendedInfo);
    }

    #endregion
}
