using UnityEngine;

public class BeetleActiveSkill : Skill
{
    [Header("Bite Settings")]
    public float stunDuration = 2.0f;

    [Header("Hitbox Settings")]
    public Vector3 biteBoxSize = new Vector3(2f, 1.5f, 2f);
    public float biteOffset = 1.0f;
    public LayerMask targetLayer;

    private BeetlePassiveSkill connectedPassive;

    private void Awake()
    {
        connectedPassive = GetComponent<BeetlePassiveSkill>();
    }

    private void Reset()
    {
        cooldown = 8.0f;
        skillName = "Vicious Bite";
        description = "Bites enemies in front, immobilizing them directly.";
    }

    protected override void OnUse(PlayerController user)
    {
        PerformBite(user);
    }

    private void PerformBite(PlayerController user)
    {
        // 공격 범위 계산 (내 앞쪽)
        Vector3 center = transform.position + transform.forward * biteOffset;

        // 범위 내의 모든 콜라이더 검출
        Collider[] hitColliders = Physics.OverlapBox(center, biteBoxSize / 2, transform.rotation, targetLayer);

        int hitCount = 0;

        foreach (var col in hitColliders)
        {
            // 나 자신은 물면 안 됨
            if (col.gameObject == user.gameObject) continue;

            // 상대방의 PlayerController 가져오기
            PlayerController target = col.GetComponent<PlayerController>();

            if (target != null)
            {
                // 함수 직접 호출 (상대방을 멈춤)
                target.ApplyCrowdControl(stunDuration);

                hitCount++;
                Debug.Log($"[{skillName}] {target.name} 물기 성공!");
            }
        }

        // 한 명이라도 물었다면 패시브(이속 증가) 발동
        if (hitCount > 0 && connectedPassive != null)
        {
            connectedPassive.OnBiteSuccess(hitCount);
        }
        else
        {
            Debug.Log($"[{skillName}] 빗나감 (No targets hit)");
        }
    }

    // 에디터에서 공격 범위를 눈으로 확인하기 위한 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 center = transform.position + transform.forward * biteOffset;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, biteBoxSize);
        Gizmos.DrawWireCube(Vector3.zero, biteBoxSize);
    }
}