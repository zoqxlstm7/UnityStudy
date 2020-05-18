using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : BasePanel
{
    [SerializeField]
    Text scoreValue;    // 스코어를 나타낼 텍스트 UI
    [SerializeField]
    Gauge hpGauge;      // 체력정보를 표시할 객체

    Player hero = null; // 플레이어 참조 객체
    Player Hero
    {
        get
        {
            if (hero == null)
                hero = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero;
            return hero;
        }
    }

    /// <summary>
    /// 스코어 표시
    /// </summary>
    /// <param name="value">획득한 총 점수</param>
    public void SetScore(int value)
    {
        Debug.Log("SetScore value: " + value);

        scoreValue.text = value.ToString();
    }

    /// <summary>
    /// 초기화
    /// </summary>
    public override void InitializePanel()
    {
        base.InitializePanel();
        hpGauge.SetHP(100, 100);    // 가득 찬 상태로 초기화
    }

    /// <summary>
    /// 업데이트 처리
    /// </summary>
    public override void UpdatePanel()
    {
        base.UpdatePanel();

        // HP 항목 업데이트
        if(Hero != null)
        {
            hpGauge.SetHP(Hero.CurrentHP, Hero.MaxHP);
        }
    }
}
