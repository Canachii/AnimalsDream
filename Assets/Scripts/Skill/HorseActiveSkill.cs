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

    private bool isRushing = false;
    public bool IsRushing => isRushing;

    protected override void OnUse(PlayerController user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();
        if (!movement.IsGrounded)
        {
            Debug.Log($"[{skillName}] !isGrounded");
            return;
        }
        StartCoroutine(RushRoutine(movement));
    }

    private IEnumerator RushRoutine(PlayerMovement movement)
    {
        AudioManager.Instance?.PlayAtPoint(SoundId.Event_HorseSkill, transform.position);

        isRushing = true;

        Debug.Log($"[{skillName}] rush start");

        movement.SetMoveSpeedMultiplier(speedMultiplier);
        movement.SetForcedForward(true);

        yield return new WaitForSeconds(duration);

        if (movement != null)
        {
            movement.SetMoveSpeedMultiplier(1.0f);
            movement.SetForcedForward(false);
        }
        isRushing = false;

        Debug.Log($"[{skillName}] Skill Done");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner || !isRushing) return;

        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            if (collision.gameObject == gameObject) return;

            var zebraShield = collision.gameObject.GetComponentInParent<ZebraPsssiveSkill>();

            if (zebraShield != null && zebraShield.TryBlock(this, gameObject))
                return;

            NetworkObject targetNetObj = collision.gameObject.GetComponentInParent<NetworkObject>();

            if (targetNetObj != null)
            {
                Debug.Log($"[{skillName}] : {collision.gameObject.name} -> knockback!");

                ApplyKnockbackServerRpc(targetNetObj.NetworkObjectId, collision.transform.position);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void ApplyKnockbackServerRpc(ulong targetObjId, Vector3 hitPos)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjId, out NetworkObject targetObj))
        {
            Vector3 knockbackDir = targetObj.transform.position - transform.position;

            knockbackDir.y = 0.2f;
            knockbackDir.Normalize();

            ApplyKnockbackClientRpc(targetObjId, knockbackDir);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ApplyKnockbackClientRpc(ulong targetObjId, Vector3 dir)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjId, out NetworkObject targetObj))
        {
            Rigidbody targetRb = targetObj.GetComponent<Rigidbody>();

            if (targetRb != null)
            {
                targetRb.AddForce(dir * knockbackForce, ForceMode.Impulse);
            }
        }
    }
}