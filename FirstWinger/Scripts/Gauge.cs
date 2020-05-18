using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour
{
    [SerializeField]
    Slider slider;      // 게이지로 쓸 슬라이더 객체

    /// <summary>
    /// 체력 셋팅
    /// </summary>
    /// <param name="currentValue">현재 체력</param>
    /// <param name="maxValue">최대 체력</param>
    public void SetHP(float currentValue, float maxValue)
    {
        // 현재 체력이 더 크다면 최대체력으로 다시 초기화
        if (currentValue > maxValue)
            currentValue = maxValue;

        // 슬라이더 value 값 설정
        slider.value = currentValue / maxValue;
    }
}
