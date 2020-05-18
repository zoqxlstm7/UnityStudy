using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningPanel : BasePanel
{
    const float BG_COLOR_VARIATION_RATE = 0.8f; // 색상 조절 비율 값
    const float MAX_MOVE_TIME = 5.0f;           // 최대로 움직이는 시간 
    const float STAY_TIME = 2.0f;               // 유지 시간

    [SerializeField]
    Image backgoundImage;   // 배경 이미지
    float bgImageColorMax;  // 배경 이미지의 원래의 알파값을 담을 변수

    // 텍스트 페이즈 관련 상태
    enum WarningUIPhase : int
    {
        In,
        Stay,
        Out,
        End
    };

    WarningUIPhase phase = WarningUIPhase.In;   // 텍스트 페이즈 관련 상태 변수

    [SerializeField]
    RectTransform canvasRectTransform;          // 캔바스 RectTransform 참조값
    [SerializeField]
    RectTransform textBackRectTransform;        // 워닝텍스트 RectTransform 참조값

    float moveStartTime;                        // 움직인 시간 저장
    float currentPosX;                          // 워닝텍스트의 현재 x 포지션

    /// <summary>
    /// 패널 초기화 작업
    /// </summary>
    public override void InitializePanel()
    {
        base.InitializePanel();

        // 초기화
        bgImageColorMax = backgoundImage.color.a;

        // 알파값 0으로 설정
        Color color = backgoundImage.color;
        color.a = 0.0f;
        backgoundImage.color = color;

        // 캔버스 x축 끝값으로 x값 초기화
        Vector2 position = textBackRectTransform.anchoredPosition;
        currentPosX = position.x = canvasRectTransform.sizeDelta.x;
        textBackRectTransform.anchoredPosition = position;

        moveStartTime = Time.time;
        Close();
    }

    public override void UpdatePanel()
    {
        base.UpdatePanel();

        UpdateColor();
        UpdateMove();
    }

    /// <summary>
    /// 배경 이미지를 알파값을 조정하여 깜박이도록 처리
    /// </summary>
    void UpdateColor()
    {
        Color color = backgoundImage.color;
        // PingPong: 타임값에 따라 length값까지 계속 반복
        color.a = Mathf.PingPong(Time.time * BG_COLOR_VARIATION_RATE, bgImageColorMax);
        backgoundImage.color = color;
    }

    /// <summary>
    /// 워닝 텍스트 처리 함수
    /// </summary>
    void UpdateMove()
    {
        if (phase == WarningUIPhase.End)
            return;

        // 이동하기 위한 부분
        Vector2 position = textBackRectTransform.anchoredPosition;
        switch (phase)
        {
            case WarningUIPhase.In: // 텍스트 부분이 들어올떄 처리
                currentPosX = Mathf.Lerp(currentPosX, 0.0f, (Time.time - moveStartTime) / MAX_MOVE_TIME);
                position.x = currentPosX;
                textBackRectTransform.anchoredPosition = position;
                break;
            case WarningUIPhase.Out: // 텍스트 부분이 나갈 때 처리
                currentPosX = Mathf.Lerp(currentPosX, -canvasRectTransform.sizeDelta.x, (Time.time - moveStartTime) / MAX_MOVE_TIME);
                position.x = currentPosX;
                textBackRectTransform.anchoredPosition = position;
                break;
        }

        // 페이즈 전환을 위한 부분
        switch (phase)
        {
            case WarningUIPhase.In:
                // 목표지점에 거의 다온 경우 처리
                if(currentPosX < 1.0f)
                {
                    phase = WarningUIPhase.Stay;
                    moveStartTime = Time.time;
                    OnPhaseStay();
                }
                break;
            case WarningUIPhase.Stay:
                if((Time.time - moveStartTime) > STAY_TIME)
                {
                    phase = WarningUIPhase.Out;
                    moveStartTime = Time.time;
                }
                break;
            case WarningUIPhase.Out:
                // 목표지점에 거의 다온 경우 처리
                if (currentPosX < -canvasRectTransform.sizeDelta.x + 1.0f)
                {
                    phase = WarningUIPhase.End;
                    OnPhaseEnd();
                }
                break;
        }
    }

    /// <summary>
    /// 워닝 텍스트가 노출되고 있을 때 처리
    /// </summary>
    void OnPhaseStay()
    {
        // 보스 생성 함수 호출
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().GenerateBoss();
    }

    /// <summary>
    /// 워니텍스트가 끝났을 때의 처리
    /// </summary>
    void OnPhaseEnd()
    {
        Close();

        // 서버인 경우 모든 페이즈가 끝나고 러닝상태 호출
        if (((FWNetworkManager)FWNetworkManager.singleton).isServer)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().SetRunningState();
    }
}
