using UnityEngine;
using System.Collections;

public class PenguinSkill : Skill
{
    [Header("Penguin Specific Settings")]
    public float slowRatio = 0.5f;
    public float duration = 4f;
    public GameObject blizzardEffectPrefab;
    public float effectScale = 2.0f;

    protected override void OnUse(PlayerController user)
    {
        Debug.Log("Skill Activated");

        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController target in allPlayers)
        {
            if (target != user)
            {
                StartCoroutine(ApplySlowAndEffect(target));
            }
        }
    }

    private IEnumerator ApplySlowAndEffect(PlayerController target)
    {
        PlayerMovement movement = target.GetComponent<PlayerMovement>();

        // player speed save
        float originalSpeed = movement.moveSpeed;

        // slow activating
        movement.moveSpeed = originalSpeed * slowRatio;
        Debug.Log($"Target {target.name} Speed Slowed");

        // effect on
        GameObject activeEffect = null;
        if (blizzardEffectPrefab != null)
        {
            // effect prefab Instantiate
            activeEffect = Instantiate(blizzardEffectPrefab, target.transform.position, Quaternion.identity, target.transform);
            activeEffect.transform.localScale = Vector3.one * effectScale;
        }

        yield return new WaitForSeconds(duration);

        // saved speed on
        if (target != null)
        {
            movement.moveSpeed = originalSpeed;
            Debug.Log($"Target {target.name} Speed Restored");
        }

        // effect deleted
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