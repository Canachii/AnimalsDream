using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class ZebraPsssiveSkill : Skill
{
    [Header("Shield Visual")]
    [SerializeField] private SkinnedMeshRenderer shieldRenderer;

    [Header("Shield Logic")]
    [SerializeField] private float rechargeSeconds = 5f;
    [SerializeField] private bool startActive = true;

    private readonly NetworkVariable<bool> isShieldActive = new NetworkVariable<bool>(
        true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public bool IsShieldActive => isShieldActive.Value;

    private Coroutine rechargeCo;
    private PlayerController owner;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();
        if (shieldRenderer == null) Debug.LogWarning("[ZebraShield] shieldRenderer is not assigned.");
    }

    public override void OnNetworkSpawn()
    {
        isShieldActive.OnValueChanged += (prev, current) =>
        {
            if (shieldRenderer != null) shieldRenderer.enabled = current;
        };

        if (shieldRenderer != null) shieldRenderer.enabled = isShieldActive.Value;

        if (IsServer && startActive) isShieldActive.Value = true;
    }

    private void OnDisable()
    {
        if (rechargeCo != null) StopCoroutine(rechargeCo);
        rechargeCo = null;
    }

    public bool TryBlock(Skill incomingSkill, GameObject attacker)
    {
        if (!isShieldActive.Value) return false;

        ConsumeServerRpc();
        return true;
    }

    [Rpc(SendTo.Server)]
    private void ConsumeServerRpc()
    {
        if (!isShieldActive.Value) return;

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

    protected override void OnUse(PlayerController user)
    {
        throw new System.NotImplementedException();
    }
}