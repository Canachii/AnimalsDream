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
    [Min(0f)]
    public float duration = 4f;

    public GameObject slowEffectPrefab;
    public float effectScale = 2.0f;

    private void OnValidate()
    {
        if (cooldown <= duration)
        {
            cooldown = duration + 5.0f;
        }
    }

    protected override void OnUse(PlayerController user)
    {
        if (!user.IsOwner) return;

        Debug.Log($"Skill Activated: {skillName}");

        RequestSkillServerRpc(user.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ServerRpc]
    private void RequestSkillServerRpc(ulong userId)
    {
        PlayerMovement[] allMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement target in allMovements)
        {
            var targetNetObj = target.GetComponent<NetworkObject>();
            if (targetNetObj == null) continue;

            if (targetNetObj.NetworkObjectId == userId) continue;

            var zebraShield = target.GetComponentInParent<ZebraPsssiveSkill>();
            if (zebraShield != null && zebraShield.TryBlock(this, gameObject))
                continue;

            ApplySkillClientRpc(targetNetObj.NetworkObjectId);
        }
    }

    [ClientRpc]
    private void ApplySkillClientRpc(ulong targetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
        {
            PlayerMovement target = targetNetObj.GetComponent<PlayerMovement>();
            if (target != null)
            {
                StartCoroutine(ApplySlowSafely(target));
            }
        }
    }

    private IEnumerator ApplySlowSafely(PlayerMovement target)
    {
        if (target.GetComponent<NetworkObject>().IsOwner)
        {
            target.SetMoveSpeedMultiplier(slowRatio);
        }

        GameObject activeEffect = null;
        if (slowEffectPrefab != null)
        {
            activeEffect = Instantiate(slowEffectPrefab, target.transform.position, Quaternion.identity, target.transform);

            activeEffect.transform.localPosition = Vector3.zero;
            activeEffect.transform.localScale = Vector3.one * effectScale;

            AudioManager.Instance?.PlayAtPoint(SoundId.Event_PenguinSkill, activeEffect.transform.position);
        }

        yield return new WaitForSeconds(duration);

        if (target != null && target.GetComponent<NetworkObject>().IsOwner)
        {
            target.SetMoveSpeedMultiplier(1.0f);
        }

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