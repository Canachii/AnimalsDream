using System;
using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    //죽음처리 + 리스폰
    //리스폰 연출 1초 투명(움직임 잠금) -> 등장
    //죽었을때 스킬 쿨타임 초기화 X
    // 1.5초 무적 제공
    
    public void DeathAndRespawn(PlayerRaceProgress p)
    {
        if (p == null) return;
        //위치 이동
        p.transform.position = p.lastCheckointTransform.position;

        //물리 리셋
        var rb = p.GetComponent<Rigidbody>();
        if(rb != null ) rb.linearVelocity = Vector3.zero;

        //입력 잠금
        var input = p.GetComponent<PlayerInputHandler>();
        input.OnRespawnStarted();   

        // 반투명
        // 1초 딜레이

        // 입력 잠금 해제
        input.OnRespawnFinished();

        // 반투명 해제

        // 무적 부여
        // 1.5초후 무적 해제
    }
}
