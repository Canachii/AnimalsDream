using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class ZebraPsssiveSkill : Skill
{
    [Header("Shield Visual")]
    [SerializeField] private SkinnedMeshRenderer shileldRenderer;

    [Header("Shield Logic")]
    [SerializeField] private float rechargeSeconds = 5f;
    [SerializeField] private bool startActive = true;

    private readonly NetworkVariable<bool> isShieldActive = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool IsShieldActive => isShieldActive.Value;

    private Coroutine rechargeCo;
    private PlayerController owner;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();

        if (shileldRenderer == null)
            Debug.LogWarning("[ZebraShield] shileldRenderer is not assigned.");
    }

    public override void OnNetworkSpawn()
    {
        isShieldActive.OnValueChanged += OnShieldStateChanged;

        if (IsServer)
        {
            isShieldActive.Value = startActive;
        }
        UpdateShieldVisual(isShieldActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isShieldActive.OnValueChanged -= OnShieldStateChanged;
    }

    private void OnDisable()
    {
        if (rechargeCo != null) StopCoroutine(rechargeCo);
        rechargeCo = null;
    }

    public bool TryBlock(Skill incomingSkill, GameObject attacker)
    {
        if (!IsServer) return false;

        if (!isShieldActive.Value) return false;

        Consume();
        return true;
    }

    private void Consume()
    {
        isShieldActive.Value = false;

        if (rechargeCo != null) StopCoroutine(rechargeCo);
        rechargeCo = StartCoroutine(CoRecharge());
    }

    private IEnumerator CoRecharge()
    {
        yield return new WaitForSeconds(rechargeSeconds);
        isShieldActive.Value = true;
        rechargeCo = null;
    }

    private void OnShieldStateChanged(bool previousValue, bool newValue)
    {
        UpdateShieldVisual(newValue);
    }

    private void UpdateShieldVisual(bool active)
    {
        if (shileldRenderer != null)
            shileldRenderer.enabled = active;
    }

    protected override void OnUse(PlayerController user)
    {
        throw new System.NotImplementedException();
    }
}