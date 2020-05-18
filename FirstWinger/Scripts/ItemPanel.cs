using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanel : BasePanel
{
    [SerializeField]
    Text itemCountText;     // 사용 가능 아이템 표시

    Player hero = null;     // 플레이어 객체
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
    /// 패널 업데이트 처리
    /// </summary>
    public override void UpdatePanel()
    {
        base.UpdatePanel();
        // 플레이어 객체가 있다면 현재 사용가능 아이템수 표시
        if(Hero != null)
        {
            itemCountText.text = hero.ItemCount.ToString();
        }
    }
}
