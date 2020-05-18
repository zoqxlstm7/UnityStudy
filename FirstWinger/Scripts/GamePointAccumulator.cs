using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePointAccumulator
{
    int gamePoint = 0;  // 게임 점수

    public int GamePoint
    {
        get => gamePoint;
    }

    /// <summary>
    /// 게임 점수 추가 처리
    /// </summary>
    /// <param name="value">획득한 게임 점수</param>
    public void Accumulate(int value)
    {
        gamePoint += value;

        // 형변환을 통해 등록된 PlayerStatePanel을 가져옴
        PlayerStatePanel playerStatePanel = PanelManager.GetPanel(typeof(PlayerStatePanel)) as PlayerStatePanel;
        // 게임 점수 세팅
        playerStatePanel.SetScore(gamePoint);
    }

    // 게임 점수 리셋 처리
    public void Reset()
    {
        gamePoint = 0;
    }
}
