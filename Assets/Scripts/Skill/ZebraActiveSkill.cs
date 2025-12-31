using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class ZebraActiveSkill : Skill
{
    [Tooltip("Duration (seconds)")]
    [Min(0f)]
    [SerializeField] private float duration = 2f;
    [SerializeField] private GameObject skillEffectPrefab;
    public float effectScale = 2.0f;

    protected override void OnUse(PlayerController user)
    {
        if (!user.IsOwner) return;

        Debug.Log($"Skill Activated: {skillName}");

        RequestSkillServerRpc(user.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ServerRpc]
    private void RequestSkillServerRpc(ulong userId)
    {
        PlayerInputHandler[] allInputHandler =
            FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);

        foreach (PlayerInputHandler target in allInputHandler)
        {
            var targetNetObj = target.GetComponent<NetworkObject>();
            if (targetNetObj == null) continue;

            var zebraShield = target.gameObject.GetComponentInParent<ZebraPsssiveSkill>();
            if (zebraShield != null && zebraShield.TryBlock(this, gameObject)) continue;

            ApplySkillClientRpc(targetNetObj.NetworkObjectId);
        }
    }

    [ClientRpc]
    private void ApplySkillClientRpc(ulong targetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
        {
            PlayerInputHandler target = targetNetObj.GetComponent<PlayerInputHandler>();
            if (target != null)
            {
                StartCoroutine(ApplySkill(target));
            }
        }
    }

    private IEnumerator ApplySkill(PlayerInputHandler target)
    {
        if (target.GetComponent<NetworkObject>().IsOwner)
        {
            target.ApplyMoveInvert(duration);
        }

        GameObject activeEffect = null;
        if (skillEffectPrefab != null)
        {
            activeEffect = Instantiate(skillEffectPrefab, target.transform);

            activeEffect.transform.localPosition = Vector3.zero;
            activeEffect.transform.localRotation = Quaternion.identity;

            activeEffect.transform.localScale = Vector3.one * effectScale;

            AudioManager.Instance?.PlayAtPoint(SoundId.Event_ZebraSkill, target.transform.position);
        }

        yield return new WaitForSeconds(duration);

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