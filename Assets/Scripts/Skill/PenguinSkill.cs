using UnityEngine;
using System.Collections;

public class PenguinSkill : Skill
{
    [Header("Penguin Settings")]
    [Tooltip("Speed multiplier (0.5 = 50% speed)")]
    [Range(0f, 1f)]
    public float slowRatio = 0.5f;

    [Tooltip("Duration (seconds)")]
    [Min(0f)]
    public float duration = 4f;

    [Tooltip("Effect prefab activation")]
    public GameObject blizzardEffectPrefab;

    protected override void OnUse(PlayerController user)
    {
        Debug.Log($"Skill Activated: {skillName}");

        PlayerMovement[] allMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement target in allMovements)
        {
            if (target != user.GetComponent<PlayerMovement>())
            {
                StartCoroutine(ApplySlowSafely(target));
            }
        }
    }

    private IEnumerator ApplySlowSafely(PlayerMovement target)
    {
        //  Apply slow effect
        target.SetMoveSpeedMultiplier(slowRatio);

        // Spawn effect
        GameObject activeEffect = null;
        if (blizzardEffectPrefab != null)
        {
            activeEffect = Instantiate(blizzardEffectPrefab, target.transform.position, Quaternion.identity, target.transform);
        }

        //Wait for duration
        yield return new WaitForSeconds(duration);

        //Reset multiplier
        if (target != null)
        {
            target.SetMoveSpeedMultiplier(1.0f);
        }

        //Remove effect
        if (activeEffect != null)
        {
            ParticleSystem ps = activeEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                Destroy(activeEffect, 2.0f);
            }
            else
            {
                Destroy(activeEffect);
            }
        }
    }
}