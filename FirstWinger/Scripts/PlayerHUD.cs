using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField]
    Gauge hpGauge;      // 체력바 프리팹
    [SerializeField]
    Player ownerPlayer; // 체력바를 할당받을 플레이어

    Transform ownerTransform;   // onwer player의 위치
    Transform selfTransform;    // 객체 자체의 위치

    private void Start()
    {
        // 본인 transform 초기화
        selfTransform = transform;
    }

    /// <summary>
    /// 외부에서 호출되어 ownerPlayer를 초기화
    /// </summary>
    /// <param name="player">초기화할 owner player</param>
    public void Initialize(Player player)
    {
        ownerPlayer = player;
        ownerTransform = player.transform;
    }

    private void Update()
    {
        UpdatePosition();
        UpdateHP();
    }

    /// <summary>
    /// owner player의 위치를 2d위치로 갱신처리
    /// </summary>
    void UpdatePosition()
    {
        if (ownerTransform != null)
            selfTransform.position = Camera.main.WorldToScreenPoint(ownerTransform.position);
    }

    void UpdateHP()
    {
        // owner player가 있는 경우
        if (ownerPlayer != null)
        {
            // owner player가 꺼질 때 같이 꺼질수 있도록
            if (!ownerPlayer.gameObject.activeSelf)
                gameObject.SetActive(ownerPlayer.gameObject.activeSelf);

            // HP 설정
            hpGauge.SetHP(ownerPlayer.CurrentHP, ownerPlayer.MaxHP);
        }
    }
}
