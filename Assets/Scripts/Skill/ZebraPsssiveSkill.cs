using UnityEngine;
using System.Collections;

public class ZebraPsssiveSkill : Skill
{

    [Header("Shield Visual")]
    [SerializeField] private SkinnedMeshRenderer shileldRenderer;

    [Header("Shield Logic")]
    [SerializeField] private float rechargeSeconds = 5f;
    [SerializeField] private bool startActive = true;

    public bool IsShieldActive => isShieldActive;

    private bool isShieldActive;
    private Coroutine rechargeCo;
    private PlayerController owner;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();

        if (shileldRenderer == null)
            Debug.LogWarning("[ZebraShield] shileldRenderer is not assigned.");
    }

    private void OnEnable()
    {
        SetShield(startActive);
    }

    private void OnDisable()
    {
        if (rechargeCo != null) StopCoroutine(rechargeCo);
        rechargeCo = null;
    }

    public bool TryBlock(Skill incomingSkill, GameObject attacker)
    {
        if (!isShieldActive) return false;

        Consume();
        return true;
    }

    private void Consume()
    {
        SetShield(false);

        if (rechargeCo != null) StopCoroutine(rechargeCo);
        rechargeCo = StartCoroutine(CoRecharge());
    }

    private IEnumerator CoRecharge()
    {
        yield return new WaitForSeconds(rechargeSeconds);
        SetShield(true);
        rechargeCo = null;
    }

    private void SetShield(bool active)
    {
        isShieldActive = active;

        if (shileldRenderer != null)
            shileldRenderer.enabled = active;
    }


    protected override void OnUse(PlayerController user)
    {
        throw new System.NotImplementedException();
    }









}
