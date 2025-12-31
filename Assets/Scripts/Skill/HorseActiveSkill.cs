using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class HorseActiveSkill : Skill
{
    [Header("Horse Rush Settings")]
    public float speedMultiplier = 1.4f;

    [Tooltip("Skill duration")]
    public float duration = 2.0f;

    [Tooltip("Knockback Power (Velocity Change)")]
    public float knockbackPower = 15.0f;
    public float knockbackStunTime = 0.8f;

    public LayerMask targetLayer;

    private readonly NetworkVariable<bool> isRushingNetVar = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public bool IsRushing => isRushingNetVar.Value;

    private float lastHitTime = 0f;
    private const float HIT_COOLDOWN = 0.5f;

    private void Reset()
    {
        cooldown = 10f;
        skillName = "Wild Rush";
        description = "Gives a speed boost and knocks back enemies.";
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject hitObj)
    {
        if (!IsOwner) return;
        if (!IsRushing) return;

        if (Time.time - lastHitTime < HIT_COOLDOWN) return;

        if (((1 << hitObj.layer) & targetLayer) == 0) return;
        if (hitObj == gameObject) return;

        var targetNetObj = hitObj.GetComponentInParent<NetworkObject>();
        if (targetNetObj != null)
        {
            lastHitTime = Time.time;

            Vector3 hitDir = targetNetObj.transform.position - transform.position;
            hitDir.y = 0;
            hitDir.Normalize();

            hitDir += Vector3.up * 0.8f;
            hitDir.Normalize();

            Debug.Log($"[HorseSkill] Hit Target: {targetNetObj.name}, Requesting Knockback");
            RequestKnockbackServerRpc(targetNetObj.NetworkObjectId, hitDir);
        }
    }

    [ServerRpc]
    private void RequestKnockbackServerRpc(ulong targetId, Vector3 pushDir)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
            return;

        var zebraShield = targetNetObj.GetComponent<ZebraPsssiveSkill>();
        if (zebraShield != null && zebraShield.TryBlock(this, gameObject))
        {
            Debug.Log("[HorseSkill] Blocked by Shield");
            return;
        }

        ApplyKnockbackClientRpc(targetId, pushDir);
    }

    [ClientRpc]
    private void ApplyKnockbackClientRpc(ulong targetId, Vector3 pushDir)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
        {
            if (!targetNetObj.IsOwner) return;

            PlayerController targetController = targetNetObj.GetComponent<PlayerController>();
            PlayerMovement targetMovement = targetNetObj.GetComponent<PlayerMovement>();
            Rigidbody targetRb = targetNetObj.GetComponent<Rigidbody>();

            if (targetController != null && targetMovement != null && targetRb != null)
            {
                StartCoroutine(CoKnockbackProcess(targetController, targetMovement, targetRb, pushDir));
            }
        }
    }

    private IEnumerator CoKnockbackProcess(PlayerController controller, PlayerMovement movement, Rigidbody rb, Vector3 dir)
    {
        Debug.Log($"[HorseSkill] Applying Force to {controller.name}");

        controller.ApplyCrowdControl(knockbackStunTime);

        movement.enabled = false;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * knockbackPower, ForceMode.VelocityChange);

        yield return new WaitForSeconds(knockbackStunTime);

        rb.linearVelocity = Vector3.zero;
        movement.enabled = true;
    }
}