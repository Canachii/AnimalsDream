using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnSystem : NetworkBehaviour 
{
    [SerializeField] private int invincibleLayer = 0; // 무적 레이어 인덱스

    public void DeathAndRespawn(PlayerRaceProgress p)
    {
        if (!IsServer) return; 
        if (p == null) return;

        // 중복 리스폰 방지
        if (p.IsRespawning.Value) return;
        p.IsRespawning.Value = true;     

        // 체크포인트 유효성
        if (p.lastCheckpointTransform == null)
        {
            Debug.LogWarning("[RespawnSystem] lastCheckpointTransform is null.");
            p.IsRespawning.Value = false;
            return;
        }

        // 원래 레이어 저장(클라에도 동일 적용하기 위해 전달)
        int originalLayer = p.gameObject.layer;

        // 1) 서버에서 텔레포트/물리 리셋 (권위)
        Vector3 targetPos = p.lastCheckpointTransform.position;
        Quaternion targetRot = p.lastCheckpointTransform.rotation;
        p.transform.SetPositionAndRotation(targetPos, targetRot); 

        if (p.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero; 
        }

        // 2) 서버 물리용 레이어 변경
        p.gameObject.layer = invincibleLayer; 

        // 3) 모든 클라에 "연출/레이어" 동기화 + 소유자 클라에 입력잠금
        BeginRespawnClientRpc(p.NetworkObject, invincibleLayer, originalLayer); 

        StartCoroutine(RespawnSequence(p.NetworkObject, originalLayer));
    }

    private IEnumerator RespawnSequence(NetworkObject playerNo, int originalLayer)
    {
        // 1초간 무적/입력잠금/반투명 유지
        yield return new WaitForSeconds(1f);

        if (playerNo != null)
        {
            // 입력 해제 + 머티리얼 원복 (클라에서)
            EndRespawnVisualClientRpc(playerNo); 
        }

        // 레이어 복구는 조금 더 뒤에
        yield return new WaitForSeconds(0.5f);

        if (playerNo != null)
        {
            // 서버 물리 레이어 복구
            playerNo.gameObject.layer = originalLayer;

            // 모든 클라 레이어 복구
            RestoreLayerClientRpc(playerNo, originalLayer); 

            // 리스폰 종료 플래그
            var progress = playerNo.GetComponent<PlayerRaceProgress>();
            if (progress != null) progress.IsRespawning.Value = false; 
        }
    }

    // ---- Client RPCs ----

    [ClientRpc]
    private void BeginRespawnClientRpc(NetworkObjectReference playerRef, int invLayer, int originalLayer)
    {
        if (!playerRef.TryGet(out var no) || no == null) return;

        // 모든 클라에서 레이어/반투명 연출 동기화
        no.gameObject.layer = invLayer;

        var matHandler = no.GetComponent<MaterialHandler>();
        matHandler?.ApplyTranslucent();

        // "소유자" 클라에서만 입력 잠금
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

        // 모든 클라에서 머티리얼 원복
        var matHandler = no.GetComponent<MaterialHandler>();
        matHandler?.ApplyOriginal();

        // 소유자만 입력 해제
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
