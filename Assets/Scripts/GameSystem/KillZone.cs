using Unity.Netcode;
using UnityEngine;

public class KillZone : MonoBehaviour
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
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        if (!other.CompareTag("Player")) return;

        var progress = other.GetComponentInParent<PlayerRaceProgress>();
        if (progress == null) return;

        if (progress.IsRespawning.Value) return;

        progress.AddDeath_Server(1);

        respawnSystem.DeathAndRespawn(progress);
    }
}
