using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum GameState : int
{
    None = 0,
    Ready,
    Running,
    NoInput,
    End
};

[System.Serializable]
public class InGameNetworkTransfer : NetworkBehaviour
{
    // 게임 시작까지 걸리는 시간
    const float GAME_READY_INTERVAL = 0.5f;

    [SyncVar]
    GameState currentGameState = GameState.None;    // 게임상태
    public GameState CurrentGameState
    {
        get => currentGameState;
    }

    [SyncVar]
    float countingStartTime;    // 게임 시작 시간 카운트용

    private void Update()
    {
        // 현재 시간 저장
        float currentTime = Time.time;

        // 게임 상태가 준비 상태라면
        if(currentGameState == GameState.Ready)
        {
            // 인터벌 시간 경과 후
            if(currentTime - countingStartTime > GAME_READY_INTERVAL)
            {
                // 게임시작 함수 호출
                SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().SquadronManager.StartGame();
                currentGameState = GameState.Running;
            }
        }
    }

    /// <summary>
    /// 클라이언트 쪽에 게임 시작을 통보
    /// </summary>
    [ClientRpc]
    public void RpcGameStart()
    {
        Debug.Log("RpcGameStart.");
        countingStartTime = Time.time;
        currentGameState = GameState.Ready;

        // 클라이언트와 서버사이드 모두 호출
        //SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.Prepare();

        // 게임이 시작될 때 캐시 생성을 클라이언트와 서버사이드 모두 직접 호출
        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        inGameSceneMain.EnemyManager.Prepare();
        inGameSceneMain.BulletManager.Prepare();
        inGameSceneMain.ItemBoxManager.Prepare();
    }

    /// <summary>
    /// 클라이언트에게 워닝UI를 보여주도록 동기화
    /// </summary>
    [ClientRpc]
    public void RpcShowWarningUI()
    {
        PanelManager.GetPanel(typeof(WarningPanel)).Show();
        // 입력 제한
        currentGameState = GameState.NoInput;
    }

    /// <summary>
    /// 보스와 전투가 시작될 때 러닝상태로 전환시키기 위한 함수
    /// </summary>
    [ClientRpc]
    public void RpcSetRunningState()
    {
        currentGameState = GameState.Running;
    }

    /// <summary>
    /// 클라이언트에게 게임 종료 유무를 전송
    /// </summary>
    /// <param name="success">종료 유무</param>
    [ClientRpc]
    public void RpcGameEnd(bool success)
    {
        // 게임을 종료상태로 만들어 입력을 막는다
        currentGameState = GameState.End;

        // 게임종료 함수 호출
        GameEndPanel gameEndPanel = PanelManager.GetPanel(typeof(GameEndPanel)) as GameEndPanel;
        gameEndPanel.ShowGameEnd(success);
    }

    /// <summary>
    /// 게임 종료시 상태 변경
    /// </summary>
    public void SetGameStateEnd()
    {
        currentGameState = GameState.End;
    }
}
