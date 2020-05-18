using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorManager
{
    // 인스턴스ID를 키값으로 actor 클래스 저장
    Dictionary<int, Actor> actors = new Dictionary<int, Actor>();

    /// <summary>
    /// 인스턴스 ID 등록
    /// </summary>
    /// <param name="actorInstanceID">등록할 인스턴스 ID</param>
    /// <param name="actor">인스턴스 ID를 소유한 Actor 객체</param>
    /// <returns></returns>
    public bool Regist(int actorInstanceID, Actor actor)
    {
        // ID 값이 있는지 검사
        if(actorInstanceID == 0)
        {
            Debug.LogError("Resist Error! ActorInstanceID is not set! ActorInstanceID: " + actorInstanceID);
            return false;
        }

        // 키값이이 존재하는지 검사
        if (actors.ContainsKey(actorInstanceID))
        {
            // 등록된 키값과 실제 인스턴스값이 다르다면 에러 처리
            if(actor.GetInstanceID() != actors[actorInstanceID].GetInstanceID())
            {
                Debug.LogError("Resist Error! already exist! ActorInstanceID: " + actorInstanceID);
                return false;
            }

            return true;
        }

        // 적재
        actors.Add(actorInstanceID, actor);
        return true;
    }

    /// <summary>
    /// 적재된 Actor 객체 반환
    /// </summary>
    /// <param name="actorInstanceID">반환받을 Actor 객체 인스턴스 ID</param>
    /// <returns></returns>
    public Actor GetActor(int actorInstanceID)
    {
        // 키값이 존재하는지 확인
        if (!actors.ContainsKey(actorInstanceID))
        {
            Debug.LogError("GetActor Error! no exist. ActorInstanceID: " + actorInstanceID);
            return null;
        }

        // 반환 처리
        return actors[actorInstanceID];
    }
}
