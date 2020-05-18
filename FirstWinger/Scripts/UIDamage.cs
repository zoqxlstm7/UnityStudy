using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDamage : MonoBehaviour
{
    // 데미지 텍스트의 상태를 표현할 열거형 객체
    enum DamageState : int
    {
        None = 0,
        SizeUP,
        Display,
        FadeOut
    }

    [SerializeField]
    DamageState damageState = DamageState.None;

    const float SIZE_UP_DURATION = 0.1f;    // 커질때에 걸리는 시간
    const float DISPLAY_DURATION = 0.5f;    // 보여지는 시간
    const float FADE_OUT_DURATION = 0.2f;   // 사라지는데 걸리는 시간

    [SerializeField]
    Text damageText;                        // 데미지를 표시할 text 객체

    Vector3 currentVelocity;                //smoothDamp에서 사용할 방향벡터

    float displayStartTime;                 // 보여지기 시작한 시간
    float fadeOutStartTime;                 // 사라지기 시작된 시간

    public string FilePath { get; set; }    // 캐싱하기 위한 파일 경로

    private void Update()
    {
        UpdateDamage();
    }

    /// <summary>
    /// 데미지를 화면에 보여줌
    /// </summary>
    /// <param name="damage">입력될 데미지값</param>
    /// <param name="textColor">데미지 텍스트 색상</param>
    public void ShowDamage(int damage, Color textColor)
    {
        damageText.text = damage.ToString();
        damageText.color = textColor;
        Reset();
        damageState = DamageState.SizeUP;
    }

    /// <summary>
    /// 데미지텍스트 스케일 조정 및 알파값 조정
    /// </summary>
    private void Reset()
    {
        transform.localScale = Vector3.zero;
        Color newColor = damageText.color;
        newColor.a = 1.0f;
        damageText.color = newColor;
    }

    /// <summary>
    /// 상태에 따른 데미지텍스트 처리
    /// </summary>
    void UpdateDamage()
    {
        // 아무것도 하지 않으면 리턴
        if (damageState == DamageState.None)
            return;

        switch (damageState)
        {
            // 데미지텍스트가 커지는 효과
            case DamageState.SizeUP:    
                transform.localScale = Vector3.SmoothDamp(transform.localScale, Vector3.one, ref currentVelocity, SIZE_UP_DURATION);

                // 스케일이 (1,1,1) 이면
                if(transform.localScale == Vector3.one)
                {
                    // display 상태로 전환하고 시작시간을 저장
                    damageState = DamageState.Display;
                    displayStartTime = Time.time;
                }
                break;
            // 데미지텍스트를 화면에 보여지는 중
            case DamageState.Display:   
                // 설정한 시간동안 데미지텍스트를 보여주고
                if(Time.time - displayStartTime > DISPLAY_DURATION)
                {
                    // fadeout 상태로 전환하고 시작시간을 저장
                    damageState = DamageState.FadeOut;
                    fadeOutStartTime = Time.time;
                }
                break;
            // 데미지텍스트 사라지는 중
            case DamageState.FadeOut:   
                // 설정한 시간에 맞게 사라지는 효과를 위해 알파값을 조정해 줌
                Color newColor = damageText.color;
                newColor.a = Mathf.Lerp(1, 0, (Time.time - fadeOutStartTime) / FADE_OUT_DURATION);
                damageText.color = newColor;

                // 알파값이 0이라면 none상태로 전환 후 데미지텍스트 제거
                if(newColor.a == 0)
                {
                    damageState = DamageState.None;
                    SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Remove(this);
                }
                break;
        }
    }
}
