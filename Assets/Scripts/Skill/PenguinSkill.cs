using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PenguinSkill : Skill
{
    [Header("Penguin Settings")]
    [Tooltip("Speed multiplier (0.5 = 50% speed)")]
    [Range(0f, 1f)]
    public float slowRatio = 0.5f;

    [Tooltip("Duration (seconds)")]
    public float duration = 4f;

    public GameObject slowEffectPrefab;
    public float effectScale = 2.0f;

    private void OnValidate()
    {
        if (cooldown <= duration) cooldown = duration + 5.0f;
    }

    protected override void OnUse(PlayerController user)
    {
        Debug.Log($"Skill Activated: {skillName}");

        PlayerMovement[] allMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement target in allMovements)
        {
            if (target != user.GetComponent<PlayerMovement>())
            {
                var zebraShield = target.GetComponentInParent<ZebraPsssiveSkill>();

                if (zebraShield != null && zebraShield.TryBlock(this, user.gameObject))
                    continue;

                StartCoroutine(ApplySlowLogic(target));

                SpawnSlowEffectServerRpc(target.transform.position);
            }
        }
    }

    private IEnumerator ApplySlowLogic(PlayerMovement target)
    {
        target.SetMoveSpeedMultiplier(slowRatio);

        yield return new WaitForSeconds(duration);

        if (target != null) target.SetMoveSpeedMultiplier(1.0f);
    }

    [Rpc(SendTo.Server)]
    private void SpawnSlowEffectServerRpc(Vector3 position)
    {
        SpawnSlowEffectClientRpc(position);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnSlowEffectClientRpc(Vector3 position)
    {
        if (slowEffectPrefab != null)
        {
            GameObject activeEffect = Instantiate(slowEffectPrefab, position, Quaternion.identity);

            activeEffect.transform.localScale = Vector3.one * effectScale;

            AudioManager.Instance?.PlayAtPoint(SoundId.Event_PenguinSkill, position);

            StartCoroutine(DestroyEffectRoutine(activeEffect, duration));
        }
    }

    private IEnumerator DestroyEffectRoutine(GameObject activeEffect, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeEffect != null)
        {
            ParticleSystem ps = activeEffect.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                ps.Stop();
                Destroy(activeEffect, 2.0f);
            }
            else Destroy(activeEffect);
        }
    }
}