using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 직렬화
[System.Serializable]
public class BGScrollData
{
    public Renderer renderForScroll;    // 텍스쳐 메테리얼이 적용된 렌더러
    public float speed;                 // 오프셋 이동 속도
    public float offsetX;               // 텍스쳐 메테리얼 오프셋 값
}

public class BGScroller : MonoBehaviour
{
    [SerializeField]
    BGScrollData[] scrollDatas;         // 스크롤링 될 배경

    private void Update()
    {
        UpdateScroll();
    }

    // 스크롤 업데이트
    void UpdateScroll()
    {
        for (int i = 0; i < scrollDatas.Length; i++)
        {
            SetTextureOffset(scrollDatas[i]);
        }
    }

    // 텍스쳐 메테리얼의 오프셋을 설정
    void SetTextureOffset(BGScrollData scrollData)
    {
        // 초당 오프셋을 이동
        scrollData.offsetX += scrollData.speed * Time.deltaTime;
        if (scrollData.offsetX > 1)
            scrollData.offsetX = scrollData.offsetX % 1f;

        // 변경 오프셋을 텍스처 메테리얼에 적용
        Vector2 offset = new Vector2(scrollData.offsetX, 0);
        scrollData.renderForScroll.material.SetTextureOffset("_MainTex", offset);
    }
}
