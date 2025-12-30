using UnityEngine;

public class KillZone: MonoBehaviour
{
    [SerializeField] private RespawnSystem respawnSystem;

    private void Awake()
    {
        if (!respawnSystem)
        { 
            respawnSystem = FindFirstObjectByType<RespawnSystem>();
            Debug.Assert(respawnSystem, "[KillZone] RespawnSystem reference missing.");
        } 

        GetComponent<Collider>().isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
         
        var progress =  other.GetComponent<PlayerRaceProgress>();
        if (progress == null) return;
        progress.SetDeathCount(1);
        respawnSystem.DeathAndRespawn(progress);
    }
}
