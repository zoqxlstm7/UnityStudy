using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController
{
    /// <summary>
    /// 인풋관련 메소드 실시간 처리
    /// </summary>
    public void UpdateInput()
    {
        // 플레이어가 없는 상태에서 동작할 수 있기때문에 게임이 Running 상태가 아니면 동작제어를 못하게
        if (SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().CurrentGameState != GameState.Running)
            return;

        UpdateKeyboard();
        UpdateMouse();
    }

    /// <summary>
    /// 입력값으로 이동 제어
    /// </summary>
    void UpdateKeyboard()
    {
        Vector3 moveDirection = Vector3.zero;
         // 상
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection.y = 1;
        }// 하
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection.y = -1;
        }// 좌
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection.x = -1;
        }// 우
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection.x = 1;
        }

        // 플레이어 객체에 입력값 전달
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero.ProcessInput(moveDirection);
    }

    /// <summary>
    /// 마우스 입력 감지 처리
    /// </summary>
    void UpdateMouse()
    {
        // 총알 발사 처리
        if (Input.GetMouseButtonDown(0))
        {
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero.Fire();
        }

        // 폭탄 발사 처리
        if (Input.GetMouseButtonDown(1))
        {
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().Hero.FireBomb();
        }
    }
}
