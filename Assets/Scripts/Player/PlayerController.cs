using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerSkillController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : NetworkBehaviour
{
    public Sprite playerIcon;
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerInputHandler input;

    private bool isCrowdControlled = false;
    private Coroutine ccCoroutine;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        skillController = GetComponent<PlayerSkillController>();
        input = GetComponent<PlayerInputHandler>();

        if (movement == null || skillController == null || input == null)
        {
            Debug.LogError($"[{name}] PlayerController is missing required components.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (isCrowdControlled)
        {
            movement.SetMoveInput(Vector2.zero);
            return;
        }

        movement.SetMoveInput(input.MoveInput);

        if (input.JumpPressed) movement.Jump();

        if (input.SkillPressed) skillController.UseSkill(0);
    }

    public void ApplyCrowdControl(float duration)
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            ApplyCrowdControlServerRpc(duration);
        }
        else
        {
            HandleCrowdControlLocally(duration);
        }
    }

    [Rpc(SendTo.Server)]
    private void ApplyCrowdControlServerRpc(float duration)
    {
        ApplyCrowdControlClientRpc(duration);
    }

    [ClientRpc]
    private void ApplyCrowdControlClientRpc(float duration)
    {
        HandleCrowdControlLocally(duration);
    }

    private void HandleCrowdControlLocally(float duration)
    {
        if (ccCoroutine != null) StopCoroutine(ccCoroutine);
        ccCoroutine = StartCoroutine(CrowdControlRoutine(duration));
    }

    private IEnumerator CrowdControlRoutine(float duration)
    {
        isCrowdControlled = true;
        yield return new WaitForSeconds(duration);
        isCrowdControlled = false;
        ccCoroutine = null;
    }
}