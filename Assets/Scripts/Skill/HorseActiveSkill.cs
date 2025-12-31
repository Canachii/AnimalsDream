using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class HorseActiveSkill : Skill
{
    [Header("Horse Rush Settings")]
    public float speedMultiplier = 1.4f;

    [Tooltip("Skill duration")]
    public float duration = 2.0f;

    public float knockbackForce = 10.0f;

    public LayerMask targetLayer;

    private readonly NetworkVariable<bool> isRushingNetVar = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public bool IsRushing => isRushingNetVar.Value;

    private void Reset()
    {
        cooldown = 10f;
        skillName = "skillName";
        description = "description";
    }

    protected override void OnUse(PlayerController user)
    {
        if (!user.IsOwner) return;

        PlayerMovement movement = user.GetComponent<PlayerMovement>();

        if (!movement.IsGrounded)
        {
            Debug.Log($"[{skillName}] !isGrounded");
            return;
        }

        RequestRushServerRpc();
    }

    [ServerRpc]
    private void RequestRushServerRpc()
    {
        isRushingNetVar.Value = true;

        StartRushClientRpc();

        StartCoroutine(ResetRushingStateCo());
    }

    private IEnumerator ResetRushingStateCo()
    {
        yield return new WaitForSeconds(duration);
        isRushingNetVar.Value = false;
    }

    [ClientRpc]
    private void StartRushClientRpc()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            StartCoroutine(RushRoutine(movement));
        }
    }

    private IEnumerator RushRoutine(PlayerMovement movement)
    {
        AudioManager.Instance?.PlayAtPoint(SoundId.Event_HorseSkill, transform.position);

        Debug.Log($"[{skillName}] rush start ( speed : {speedMultiplier}, duration : {duration}s)");

        if (IsOwner)
        {
            movement.SetMoveSpeedMultiplier(speedMultiplier);
            movement.SetForcedForward(true);
        }

        yield return new WaitForSeconds(duration);

        if (movement != null && IsOwner)
        {
            movement.SetMoveSpeedMultiplier(1.0f);
            movement.SetForcedForward(false);
        }


        Debug.Log($"[{skillName}] Skill Done");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        if (!IsRushing) return;

        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            if (collision.gameObject == gameObject) return;

            var targetNetObj = collision.gameObject.GetComponentInParent<NetworkObject>();
            if (targetNetObj != null)
            {
                RequestKnockbackServerRpc(targetNetObj.NetworkObjectId);
            }
        }
    }

    [ServerRpc]
    private void RequestKnockbackServerRpc(ulong targetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
            return;

        GameObject targetObj = targetNetObj.gameObject;

        var zebraShield = targetObj.GetComponentInParent<ZebraPsssiveSkill>();
        if (zebraShield != null && zebraShield.TryBlock(this, gameObject))
        {
            Debug.Log("Horse Rush Blocked by Zebra Shield");
            return;
        }

        ApplyKnockbackClientRpc(targetId);
    }

    [ClientRpc]
    private void ApplyKnockbackClientRpc(ulong targetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
        {
            Rigidbody targetRb = targetNetObj.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 knockbackDir = targetNetObj.transform.position - transform.position;
                knockbackDir.y = 0.2f;
                knockbackDir.Normalize();

                targetRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                Debug.Log($"[{skillName}] : {targetNetObj.name} -> knockback applied!");
            }
        }
    }
}