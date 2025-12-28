using UnityEngine;

public class BeetleActiveSkill : Skill
{
    [Header("Bite Settings")]
    public float stunDuration = 2.0f;

    [Header("Hitbox Settings")]
    public Vector3 biteBoxSize = new Vector3(2f, 1.5f, 2f);
    public float biteForwardOffset = 1.0f;
    public float biteHeightOffset = 0.5f;

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

            if (col.gameObject == user.gameObject) continue;

            if (((1 << col.gameObject.layer) & targetLayer) == 0) continue;

            // PlayerMovement check
            PlayerMovement targetMovement = col.GetComponent<PlayerMovement>();
            if (targetMovement != null)
            {
                PlayerController targetController = col.GetComponent<PlayerController>();
                if (targetController != null)
                {
                    targetController.ApplyCrowdControl(stunDuration);
                    Debug.Log($" 플레이어({col.name}) 물기 성공!");
                }
                else
                {
                    Debug.Log($" 더미({col.name}) 물기 성공!");
                }
                hitCount++;
            }
        }

        if (hitCount > 0 && connectedPassive != null)
        {
            connectedPassive.OnBiteSuccess(hitCount);
        }
    }

    // editor hitbox view
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