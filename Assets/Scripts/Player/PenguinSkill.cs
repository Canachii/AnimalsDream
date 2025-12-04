using UnityEngine;
using System.Collections;

public class PenguinSkill : Skill
{
    [Header("Penguin Specific Settings")]
    [Tooltip("속도가 느려지는 비율 (0.5 = 50% 속도)")]
    public float slowRatio = 0.5f;

    [Tooltip("지속 시간 (초)")]
    public float duration = 4f;

    [Tooltip("스킬 발동 시 대상에게 붙을 이펙트 프리팹")]
    public GameObject blizzardEffectPrefab;

    protected override void OnUse(PlayerController user)
    {
        Debug.Log("Skill Activated");

        // 맵의 모든 플레이어 찾기
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController target in allPlayers)
        {
            // 나(user)는 제외하고 나머지 플레이어에게만 적용
            if (target != user)
            {
                StartCoroutine(ApplySlowAndEffect(target));
            }
        }
    }

    // 각 대상에게 속도 감소와 이펙트를 적용하고, 시간이 지나면 복구하는 코루틴
    private IEnumerator ApplySlowAndEffect(PlayerController target)
    {
        // 1. 원래 속도 저장
        float originalSpeed = target.moveSpeed;

        // 2. 속도 감소 적용
        target.moveSpeed = originalSpeed * slowRatio;
        Debug.Log($"Target {target.name} Speed Slowed");

        // 3. 이펙트 생성 (타겟을 부모로 설정하여 따라다니게 함)
        GameObject activeEffect = null;
        if (blizzardEffectPrefab != null)
        {
            // Instantiate(프리팹, 위치, 회전, 부모Transform)
            activeEffect = Instantiate(blizzardEffectPrefab, target.transform.position, Quaternion.identity, target.transform);
        }

        // 4. 지속 시간 대기
        yield return new WaitForSeconds(duration);

        // 5. 속도 복구 (타겟이 존재하는 경우에만)
        if (target != null)
        {
            target.moveSpeed = originalSpeed;
            Debug.Log($"Target {target.name} Speed Restored");
        }

        // 6. 이펙트 삭제
        if (activeEffect != null)
        {
            Destroy(activeEffect);
        }
    }
}