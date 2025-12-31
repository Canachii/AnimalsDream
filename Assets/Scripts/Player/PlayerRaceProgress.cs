using Unity.Netcode;
using UnityEngine;

public class PlayerRaceProgress : NetworkBehaviour
{
    [Header("Checkpoint")]
    public Transform lastCheckpointTransform;

    //서버 권위 데스카운트
    private readonly NetworkVariable<int> deathCount =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //리스폰 중 플래그(중복 트리거/중복 카운트 방지)
    public readonly NetworkVariable<bool> IsRespawning =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int DeathCount => deathCount.Value;

    
    public void AddDeath_Server(int delta)
    {
        if (!IsServer) return;
        deathCount.Value += delta;
    }
}
