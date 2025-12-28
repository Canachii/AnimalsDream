using UnityEngine;
using System.Collections;

public class BeetlePassiveSkill : Skill
{
    [Header("Adrenaline Settings")]
    public float speedBuffDuration = 1.5f;
    public float speedBonusPerHit = 0.15f;

    private PlayerMovement movement;
    private Coroutine buffCoroutine;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    protected override void OnUse(PlayerController user) { }

    public void OnBiteSuccess(int enemyCount)
    {
        // 속도 증가량 계산 (3명 -> 0.45 -> 45%)
        float totalBonus = enemyCount * speedBonusPerHit;
        float targetMultiplier = 1.0f + totalBonus;

        // 이미 버프 중이면 끄고 다시 시작 (시간 갱신)
        if (buffCoroutine != null) StopCoroutine(buffCoroutine);

        buffCoroutine = StartCoroutine(SpeedBuffRoutine(targetMultiplier));
    }

    private IEnumerator SpeedBuffRoutine(float multiplier)
    {
        Debug.Log($"[Passive] 대상 추격. 속도 {multiplier}배 증가");

        // PlayerMovement에 있는 속도 배율 함수 호출
        if (movement != null) movement.SetMoveSpeedMultiplier(multiplier);

        yield return new WaitForSeconds(speedBuffDuration);

        // 원상 복구
        if (movement != null) movement.SetMoveSpeedMultiplier(1.0f);

        Debug.Log("[Passive] 추격 종료.");
        buffCoroutine = null;
    }
}