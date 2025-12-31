using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class BeetlePassiveSkill : Skill
{
    [Header("Adrenaline Settings")]
    public float speedBuffDuration = 1.5f;
    public float speedBonusPerHit = 0.15f;

    private PlayerMovement movement;
    private Coroutine buffCoroutine;

    [Header("VFX")]
    [SerializeField] private GameObject speedEffectObject;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        skillName = " ";
        if (speedEffectObject != null) speedEffectObject.SetActive(false);
    }

    protected override void OnUse(PlayerController user) { }

    public void OnBiteSuccess(int enemyCount)
    {
        Debug.Log($"[BeetlePassive] 패시브 발동 요청됨 (물린 적: {enemyCount}명)");

        if (enemyCount <= 0) return;

        TriggerBuffRpc(enemyCount);
    }

    [Rpc(SendTo.Everyone)]
    private void TriggerBuffRpc(int enemyCount)
    {
        float totalBonus = enemyCount * speedBonusPerHit;
        float targetMultiplier = 1.0f + totalBonus;

        if (buffCoroutine != null) StopCoroutine(buffCoroutine);

        buffCoroutine = StartCoroutine(SpeedBuffRoutine(targetMultiplier));
    }

    private IEnumerator SpeedBuffRoutine(float multiplier)
    {
        if (speedEffectObject != null) speedEffectObject.SetActive(true);

        if (IsOwner && movement != null)
        {
            movement.SetMoveSpeedMultiplier(multiplier);

            Debug.Log($"[BeetlePassive] 이동 속도 증가 (x{multiplier})");
        }

        yield return new WaitForSeconds(speedBuffDuration);

        if (IsOwner && movement != null) movement.SetMoveSpeedMultiplier(1.0f);

        if (speedEffectObject != null) speedEffectObject.SetActive(false);

        Debug.Log($"[BeetlePassive] 속도 정상화");

        buffCoroutine = null;
    }
}