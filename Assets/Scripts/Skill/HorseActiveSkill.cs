using System.Collections;
using UnityEngine;

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

    private void Reset()
    {
        cooldown = 10f;
        skillName = "skillName";
        description = "description";
    }

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

        Debug.Log($"[{skillName}] rush start ( speed : {speedMultiplier}, duration : {duration}s)");

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
        if (!isRushing) return;

        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            if (collision.gameObject == gameObject) return;
            
            var zebraShield = collision.gameObject.GetComponentInParent<ZebraPsssiveSkill>();
            if (zebraShield != null && zebraShield.TryBlock(this, gameObject))
                return;

            Rigidbody targetRb = collision.gameObject.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Debug.Log($"[{skillName}] : {collision.gameObject.name} -> knockback!");

                ApplyKnockback(targetRb, collision);
            }
        }
    }

    private void ApplyKnockback(Rigidbody targetRb, Collision collision)
    {
        Vector3 knockbackDir = collision.transform.position - transform.position;
        knockbackDir.y = 0.2f;
        knockbackDir.Normalize();

        targetRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
    }
}