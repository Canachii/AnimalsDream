using Unity.Netcode;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var progress = other.GetComponentInParent<PlayerRaceProgress>();
        if (progress == null) return;

        // 서버에서 감지되면 서버가 처리
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (progress.IsRespawning.Value) return;
            RespawnSystem.Instance.ServerDeathAndRespawn(progress, 1);
            return;
        }

        // 클라에서 감지되는 경우: "자기(Owner)만" 서버에 요청
        if (progress.IsOwner)
        {
            progress.RequestDeathServerRpc();
        }
    }
}
