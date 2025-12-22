using UnityEngine;

public class KillZoon : MonoBehaviour
{
    [SerializeField] RespawnSystem respawnSystem;

    private void Awake()
    {
        if (respawnSystem == null) respawnSystem = FindFirstObjectByType<RespawnSystem>();

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
