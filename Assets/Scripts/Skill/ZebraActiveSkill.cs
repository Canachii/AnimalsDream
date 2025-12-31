using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ZebraActiveSkill : Skill
{
    [Tooltip("Duration (seconds)")]
    [Min(0f)]
    [SerializeField] private float duration = 2f;
    [SerializeField] private GameObject skillEffectPrefab;
    public float effectScale = 2.0f;


    protected override void OnUse(PlayerController user)
    {
        Debug.Log($"Skill Activated: {skillName}");

        PlayerInputHandler[] allInputHandler = 
            FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);

        foreach (PlayerInputHandler target in allInputHandler)
        {
            var zebraShield = target.gameObject.GetComponentInParent<ZebraPsssiveSkill>();
            if (zebraShield != null && zebraShield.TryBlock(this, gameObject))
                continue;
            StartCoroutine(ApplySkill(target));

            SpawnEffectServerRpc(target.transform.position);
        }
    }

    private IEnumerator ApplySkill(PlayerInputHandler target)
    {
        target.ApplyMoveInvert(duration);

        GameObject activeEffect = null;
        if (skillEffectPrefab != null)
        {
            activeEffect = Instantiate(skillEffectPrefab, target.transform);
            activeEffect.transform.localScale = Vector3.one * effectScale;
            AudioManager.Instance?.PlayAtPoint(SoundId.Event_ZebraSkill, activeEffect.transform.position);
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

    [Rpc(SendTo.Server)]
    private void SpawnEffectServerRpc(Vector3 position)
    {
        SpawnEffectClientRpc(position);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnEffectClientRpc(Vector3 position)
    {
        if (skillEffectPrefab != null)
        {
            GameObject effect = Instantiate(skillEffectPrefab, position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * effectScale;
            Destroy(effect, 2.0f);
        }
    }



}
