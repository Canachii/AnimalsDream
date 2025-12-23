using UnityEngine;

public class KillZoon : MonoBehaviour
{
    [SerializeField] private RespawnSystem respawnSystem;

    private void Awake()
    {
        if (!respawnSystem)
        { 
            respawnSystem = FindFirstObjectByType<RespawnSystem>();
            Debug.Assert(respawnSystem, "[KillZoon] RespawnSystem reference missing.");
        } 

        GetComponent<Collider>().isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
         
        var progress =  other.GetComponent<PlayerRaceProgress>();
        if (progress == null) return;
        respawnSystem.DeathAndRespawn(progress);
    }
}
