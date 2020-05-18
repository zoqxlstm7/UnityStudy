using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    private void Awake()
    {
        InitializePanel();
    }

    private void Update()
    {
        UpdatePanel();
    }

    /// <summary>
    /// 오브젝트가 파괴될 떄 실행
    /// </summary>
    private void OnDestroy()
    {
        DestroyPanel();
    }

    /// <summary>
    /// 초기화
    /// </summary>
    public virtual void InitializePanel()
    {
        // 패널매니저에 등록 처리
        PanelManager.RegistPanel(GetType(), this);
    }

    /// <summary>
    /// 업데이트 처리
    /// </summary>
    public virtual void UpdatePanel()
    {

    }

    /// <summary>
    /// 오브젝트 파괴시 필요한 동작 처리
    /// </summary>
    public virtual void DestroyPanel()
    {
        PanelManager.UnregistPanel(GetType());
    }

    /// <summary>
    /// 패널을 보여주는 함수
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 패널을 닫는 함수
    /// </summary>
    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}
