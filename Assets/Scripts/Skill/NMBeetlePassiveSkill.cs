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

    private readonly NetworkVariable<bool> isBuffActive = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        skillName = " ";

        if (speedEffectObject != null)
        {
            speedEffectObject.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        isBuffActive.OnValueChanged += OnBuffStateChanged;

        if (speedEffectObject != null) speedEffectObject.SetActive(isBuffActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isBuffActive.OnValueChanged -= OnBuffStateChanged;
    }

    protected override void OnUse(PlayerController user) { }

    public void OnBiteSuccess(int enemyCount)
    {
        Debug.Log($"[BeetlePassive] 패시브 발동 요청됨 (물린 적: {enemyCount}명)");

        if (enemyCount <= 0) return;
        if (!IsServer) return;

        isBuffActive.Value = true;

        ApplyBuffClientRpc(enemyCount);
    }

    [ClientRpc]
    private void ApplyBuffClientRpc(int enemyCount)
    {
        if (!IsOwner) return;

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

        RequestEndBuffServerRpc();
    }

    [ServerRpc]
    private void RequestEndBuffServerRpc()
    {
        isBuffActive.Value = false;
    }

    private void OnBuffStateChanged(bool prev, bool current)
    {
        if (speedEffectObject != null)
            speedEffectObject.SetActive(current);
    }
}