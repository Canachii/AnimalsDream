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

        // 필터 없이 일단 다 감지
        Collider[] allColliders = Physics.OverlapBox(center, biteBoxSize / 2, transform.rotation);

        int hitCount = 0;

        foreach (var col in allColliders)
        {
            // 시전자 본인 무시
            if (col.transform.root == user.transform.root) continue;

            //. 레이어 체크 (벽이나 바닥 무시용)
            if (((1 << col.gameObject.layer) & targetLayer) == 0) continue;

            bool isHit = false;

            PlayerController targetController = col.GetComponentInParent<PlayerController>();
            if (targetController != null)
            {
                targetController.ApplyCrowdControl(stunDuration);
                Debug.Log($"[BeetleSkill] 플레이어 제어({targetController.name}) 차단 성공");
                isHit = true;
            }
            //더미감지용(나중에지워도됨)
            DummyAutoMove targetDummy = col.GetComponentInParent<DummyAutoMove>();
            if (targetDummy != null)
            {
                targetDummy.ApplyStun(stunDuration);
                Debug.Log($"[BeetleSkill] 더미 이동({targetDummy.name}) 차단 성공");
                isHit = true;
            }

            // 둘 중 하나라도 걸렸으면 성공 카운트 증가
            if (isHit)
            {
                hitCount++;
            }
        }

        if (hitCount > 0 && connectedPassive != null)
        {
            connectedPassive.OnBiteSuccess(hitCount);
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