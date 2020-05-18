using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoCacheableEffect : MonoBehaviour
{
    public string FilePath { get; set; }    // 캐시 파일 경로

    private void OnEnable()
    {
        StartCoroutine("CheckIfAlive");
    }

    // 파티클이 살아있는지 확인
    IEnumerator CheckIfAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            // 파티클 실행이 종료가 되었다면
            if (!GetComponent<ParticleSystem>().IsAlive(true))
            {
                // 파티클 삭제 함수 호출
                SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.RemoveEffect(this);
                break;
            }
        }
    }
}
