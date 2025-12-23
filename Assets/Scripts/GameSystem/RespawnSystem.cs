using System;
using System.Collections;
using UnityEngine;

public class RespawnSystem : MonoBehaviour
{

    [SerializeField] int invincibleLayer;
    public void DeathAndRespawn(PlayerRaceProgress p)
    {
        if (p == null) return;
        p.transform.position = p.lastCheckpointTransform.position;

        var rb = p.GetComponent<Rigidbody>();
        if(rb != null ) rb.linearVelocity = Vector3.zero;

        var input = p.GetComponent<PlayerInputHandler>();
        input.OnRespawnStarted();

        var matHandler = p.GetComponent<MaterialHandler>();
        matHandler.ApplyTranslucent();

        int playerLayer = p.gameObject.layer;
        p.gameObject.layer = invincibleLayer;

        StartCoroutine(RespawnSequence(p,input, matHandler, playerLayer));
    }

    private IEnumerator RespawnSequence(PlayerRaceProgress p, PlayerInputHandler input, MaterialHandler matHandler, int playerLayer)
    {
        yield return new WaitForSeconds(1f);
        input.OnRespawnFinished();
        matHandler.ApplyOriginal();

        yield return new WaitForSeconds(0.5f);
        p.gameObject.layer = playerLayer;
    }



}
