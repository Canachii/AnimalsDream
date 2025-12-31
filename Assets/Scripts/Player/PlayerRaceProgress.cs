using Unity.Netcode;
using UnityEngine;

public class PlayerRaceProgress : NetworkBehaviour 
{
    [SerializeField] private RaceManager raceManager;

    // 체크포인트 Transform은 "서버 리스폰"에서만 쓰면 되므로 로컬 참조로 유지
    public Transform lastCheckpointTransform; // 기존 유지

    // ---- Networked state (서버만 Write) ----
    public readonly NetworkVariable<int> netLastCheckpointIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public readonly NetworkVariable<Vector3> netLastCheckpointPos =
        new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); 

    public readonly NetworkVariable<Quaternion> netLastCheckpointRot =
        new(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); 

    private readonly NetworkVariable<float> netFinishTime =
        new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<bool> netFinished =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> netRank =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> netDeathCount =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public readonly NetworkVariable<bool> IsRespawning =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int lastCheckpointIndex => netLastCheckpointIndex.Value;
    public float finishTime
    {
        get => netFinishTime.Value;
        set { if (IsServer) netFinishTime.Value = value; }
    }
    public bool finished
    {
        get => netFinished.Value;
        set { if (IsServer) netFinished.Value = value; }   
    }

    public int Rank => netRank.Value; 
    public int DeathCount => netDeathCount.Value;

    private void Awake()
    {
        if (!raceManager)
        {
            raceManager = FindFirstObjectByType<RaceManager>();
            Debug.Assert(raceManager, "[PlayerRaceProgress] RaceManager reference missing.");
        }

    }

    public override void OnNetworkSpawn()
    {
        raceManager?.RegisterPlayer(this);

        if (IsServer)
        {
            netDeathCount.Value = 0;
            netRank.Value = 0;
            netFinishTime.Value = 0f;
            netFinished.Value = false;
            netLastCheckpointIndex.Value = -1;
            IsRespawning.Value = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        raceManager?.UnregisterPlayer(this);
    }


    public void UpdateCheckpoint(int index, Transform checkpointTransform)
    {
        if (checkpointTransform == null) return;

        // 서버라면 바로 기록
        if (IsServer)
        {
            SetCheckpoint_Server(index, checkpointTransform.position, checkpointTransform.rotation);
            return;
        }

        // 클라는 "자기(Owner)만" 서버에 보고
        if (IsOwner)
            ReportCheckpointServerRpc(index, checkpointTransform.position, checkpointTransform.rotation); // [CHANGED]
    }

    private void SetCheckpoint_Server(int index, Vector3 pos, Quaternion rot)
    {
        if (!IsServer) return;
        netLastCheckpointIndex.Value = index;
        netLastCheckpointPos.Value = pos;
        netLastCheckpointRot.Value = rot;
    }

    [ServerRpc]
    private void ReportCheckpointServerRpc(int index, Vector3 pos, Quaternion rot)
    {
        SetCheckpoint_Server(index, pos, rot);
    }
    public void SetRank(int rank)
    {
        if (!IsServer) return;
        netRank.Value = rank;              
    }

    // KillZone에서 쓰기 좋게 별칭도 제공
    public void AddDeath_Server(int delta = 1) 
    {
        if (!IsServer) return;
        netDeathCount.Value += delta;
    }
    [ServerRpc] // [CHANGED] 클라 -> 서버 죽음 요청
    public void RequestDeathServerRpc()
    {
        if (RespawnSystem.Instance == null) return;
        RespawnSystem.Instance.ServerDeathAndRespawn(this, 1);
    }
}
