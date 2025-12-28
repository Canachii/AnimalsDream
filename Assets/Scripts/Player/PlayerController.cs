using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerSkillController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : NetworkBehaviour
{
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerInputHandler input;

    // cc skill (for beetle)
    private bool isCrowdControlled = false;

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
        if (!IsOwner) return; // 내 캐릭터가 아니면 아래 로직 실행 안함

        if (isCrowdControlled)
        {
            movement.SetMoveInput(Vector2.zero); // CC
            return; // CC 걸리면 점프, 스킬 실행 안하고 리턴
        }
        movement.SetMoveInput(input.MoveInput);

        if (input.JumpPressed)
            movement.Jump();

        if (input.SkillPressed)
            skillController.UseSkill(0);
    }
    // [추가] 외부(스킬)에서 호출할 함수
    public void ApplyCrowdControl(float duration)
    {
        // 코루틴 충돌 방지를 위해 기존 것 끄고 새로 시작
        StopCoroutine("CrowdControlRoutine");
        StartCoroutine(CrowdControlRoutine(duration));
    }

    private IEnumerator CrowdControlRoutine(float duration)
    {
        isCrowdControlled = true;
        // Debug.Log($"[{name}] 행동 불가 ({duration}초)");

        yield return new WaitForSeconds(duration);

        isCrowdControlled = false;
        // Debug.Log($"[{name}] 풀려남.");
    }
}
