using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    // 등록된 패널들을 관리할 변수
    static Dictionary<Type, BasePanel> panels = new Dictionary<Type, BasePanel>();

    /// <summary>
    /// 패널 등록
    /// </summary>
    /// <param name="panelClassType">등록할 패널 클래스 타입</param>
    /// <param name="basePanel">등록할 패널 정보</param>
    /// <returns></returns>
    public static bool RegistPanel(Type panelClassType, BasePanel basePanel)
    {
        // 키값이 있는지 확인
        if (panels.ContainsKey(panelClassType))
        {
            Debug.LogError("RegistPanel Error! Already exist Type. panelClassType: " + panelClassType.ToString());
            return false;
        }

        // 패널 추가
        panels.Add(panelClassType, basePanel);
        return true;
    }

    /// <summary>
    /// 패널 등록 해제
    /// </summary>
    /// <param name="panelClassType">등록 해제 할 패널 클래스 타입</param>
    /// <returns></returns>
    public static bool UnregistPanel(Type panelClassType)
    {
        // 키값이 있는지 확인
        if (!panels.ContainsKey(panelClassType))
        {
            Debug.LogError("UnregistPanel Error! can't find type. panelClassType: " + panelClassType.ToString());
            return false;
        }

        // 패널 삭제
        panels.Remove(panelClassType);
        return true;
    }

    /// <summary>
    /// 등록된 패널 정보를 얻어오는 함수
    /// </summary>
    /// <param name="panelClassType">얻어올 패널 클래스 타입</param>
    /// <returns></returns>
    public static BasePanel GetPanel(Type panelClassType)
    {
        // 키값이 있는지 확인
        if (!panels.ContainsKey(panelClassType))
        {
            Debug.LogError("GetPanel Error! can't find type. panelClassType: " + panelClassType.ToString());
            return null;
        }

        return panels[panelClassType];
    }
}
