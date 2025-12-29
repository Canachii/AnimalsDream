using UnityEngine;
using Unity.Netcode;

public class BeetleActiveSkill : Skill
{
    [Header("Bite Settings")]
    public float stunDuration = 5.0f;

    [Header("Hitbox Settings")]
    public Vector3 biteBoxSize = new Vector3(2f, 1.5f, 2f);
    public float biteForwardOffset = 1.0f;
    public float biteHeightOffset = 0.5f;

    [Header("VFX")]
    public GameObject stunEffectPrefab;
    public float effectHeight = 2.0f;
    public float effectDuration = 5.0f;

    public LayerMask targetLayer;

    private BeetlePassiveSkill connectedPassive;

    private void Awake()
    {
        connectedPassive = GetComponent<BeetlePassiveSkill>();
    }

    protected override void OnUse(PlayerController user)
    {
        PerformBite(user);
    }

    private void PerformBite(PlayerController user)
    {
        Vector3 center = transform.position
                         + (transform.forward * biteForwardOffset)
                         + (Vector3.up * biteHeightOffset);

        Collider[] allColliders = Physics.OverlapBox(center, biteBoxSize / 2, transform.rotation);

        int hitCount = 0;

        foreach (var col in allColliders)
        {
            if (col.transform.root == user.transform.root) continue;
            if (((1 << col.gameObject.layer) & targetLayer) == 0) continue;

            bool isHit = false;
            Vector3 spawnPos = col.transform.position + Vector3.up * effectHeight;

            PlayerController targetController = col.GetComponentInParent<PlayerController>();
            if (targetController != null)
            {
                targetController.ApplyCrowdControl(stunDuration);

                SpawnEffectServerRpc(spawnPos);

                Debug.Log($"[BeetleSkill] 플레이어({targetController.name}) 명중");
                isHit = true;
            }

            // 더미 체크용 (나중에 삭제해도 됨)
            DummyAutoMove targetDummy = col.GetComponentInParent<DummyAutoMove>();
            if (targetDummy != null)
            {
                targetDummy.ApplyStun(stunDuration);

                if (stunEffectPrefab != null)
                {
                    GameObject effect = Instantiate(stunEffectPrefab, spawnPos, Quaternion.identity);
                    Destroy(effect, effectDuration);
                }

                Debug.Log($"[BeetleSkill] 더미({targetDummy.name}) 명중");
                isHit = true;
            }

            if (isHit) hitCount++;
        }

        if (hitCount > 0 && connectedPassive != null)
        {
            connectedPassive.OnBiteSuccess(hitCount);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnEffectServerRpc(Vector3 position)
    {
        SpawnEffectClientRpc(position);
    }

    [ClientRpc]
    private void SpawnEffectClientRpc(Vector3 position)
    {
        if (stunEffectPrefab != null)
        {
            GameObject effect = Instantiate(stunEffectPrefab, position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 center = transform.position
                         + (transform.forward * biteForwardOffset)
                         + (Vector3.up * biteHeightOffset);

        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, biteBoxSize);
        Gizmos.DrawWireCube(Vector3.zero, biteBoxSize);
    }
}