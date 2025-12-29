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
        skillName = "Swarm Instinct";
    }

    protected override void OnUse(PlayerController user) { }

    public void OnBiteSuccess(int enemyCount)
    {
        // 패시브 활성화 확인
        Debug.Log($"[BeetlePassive] 패시브 발동 요청됨 (물린 적: {enemyCount}명)");

        if (enemyCount <= 0) return;

        float totalBonus = enemyCount * speedBonusPerHit;
        float targetMultiplier = 1.0f + totalBonus;

        if (buffCoroutine != null) StopCoroutine(buffCoroutine);

        buffCoroutine = StartCoroutine(SpeedBuffRoutine(targetMultiplier));
    }

    private IEnumerator SpeedBuffRoutine(float multiplier)
    {
        Debug.Log($"[BeetlePassive] 이동 속도 증가 (x{multiplier})");

        if (movement != null)
        {
            movement.SetMoveSpeedMultiplier(multiplier);
        }

        yield return new WaitForSeconds(speedBuffDuration);

        if (movement != null)
        {
            movement.SetMoveSpeedMultiplier(1.0f);
        }

        Debug.Log($"[BeetlePassive] 속도 정상화");
        buffCoroutine = null;
    }
}