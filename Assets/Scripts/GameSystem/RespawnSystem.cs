using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnSystem : NetworkBehaviour
{
    public static RespawnSystem Instance { get; private set; }

    [SerializeField] private int invincibleLayer = 0;

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    // [CHANGED] 서버에서만 호출되는 "통합" 함수 (데스 증가 포함)
    public void ServerDeathAndRespawn(PlayerRaceProgress p, int addDeath)
    {
        if (!IsServer) return;
        if (p == null) return;

        if (p.IsRespawning.Value) return;
        p.IsRespawning.Value = true;

        // 서버에서 데스 증가
        p.AddDeath_Server(addDeath);

        // 서버가 알고 있는 체크포인트 좌표/회전 사용
        Vector3 targetPos = p.netLastCheckpointPos.Value;
        Quaternion targetRot = p.netLastCheckpointRot.Value;

        // 체크포인트가 아직 한번도 안 잡혔으면(기본값) 안전장치
        if (p.netLastCheckpointIndex.Value < 0)
        {
            Debug.LogWarning("[RespawnSystem] No checkpoint recorded on server for this player.");
            p.IsRespawning.Value = false;
            return;
        }

        int originalLayer = p.gameObject.layer;

        // 서버도 위치 갱신 (서버 권위 환경이면 이게 전파됨)
        p.transform.SetPositionAndRotation(targetPos, targetRot);

        if (p.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 무적 레이어
        p.gameObject.layer = invincibleLayer;

        // 연출/입력 잠금
        NetworkObjectReference playerRef = p.NetworkObject;
        BeginRespawnClientRpc(playerRef, invincibleLayer, originalLayer);

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { p.OwnerClientId }
            }
        };
        TeleportOwnerClientRpc(playerRef, targetPos, targetRot, rpcParams);

        StartCoroutine(RespawnSequence(playerRef, originalLayer));
    }

    private IEnumerator RespawnSequence(NetworkObjectReference playerRef, int originalLayer)
    {
        yield return new WaitForSeconds(1f);
        EndRespawnVisualClientRpc(playerRef);

        yield return new WaitForSeconds(0.5f);
        RestoreLayerClientRpc(playerRef, originalLayer);

        if (playerRef.TryGet(out var no) && no != null)
        {
            var progress = no.GetComponent<PlayerRaceProgress>();
            if (progress != null) progress.IsRespawning.Value = false;
        }
    }

    [ClientRpc] // [CHANGED] 소유자에게 텔레포트(클라 권위 대비)
    private void TeleportOwnerClientRpc(NetworkObjectReference playerRef, Vector3 pos, Quaternion rot, ClientRpcParams rpcParams = default)
    {
        if (!playerRef.TryGet(out var no) || no == null) return;

        no.transform.SetPositionAndRotation(pos, rot);

        if (no.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    [ClientRpc]
    private void BeginRespawnClientRpc(NetworkObjectReference playerRef, int invLayer, int originalLayer)
    {
        if (!playerRef.TryGet(out var no) || no == null) return;

        no.gameObject.layer = invLayer;

        var matHandler = no.GetComponent<MaterialHandler>();
        matHandler?.ApplyTranslucent();

        if (no.IsOwner)
        {
            var input = no.GetComponent<PlayerInputHandler>();
            input?.OnRespawnStarted();
        }
    }

    [ClientRpc]
    private void EndRespawnVisualClientRpc(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out var no) || no == null) return;

        var matHandler = no.GetComponent<MaterialHandler>();
        matHandler?.ApplyOriginal();

        if (no.IsOwner)
        {
            var input = no.GetComponent<PlayerInputHandler>();
            input?.OnRespawnFinished();
        }
    }

    [ClientRpc]
    private void RestoreLayerClientRpc(NetworkObjectReference playerRef, int originalLayer)
    {
        if (!playerRef.TryGet(out var no) || no == null) return;
        no.gameObject.layer = originalLayer;
    }
}
