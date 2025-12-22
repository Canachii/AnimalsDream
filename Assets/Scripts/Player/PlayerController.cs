using System;
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

        movement.SetMoveInput(input.MoveInput);

        if (input.JumpPressed)
            movement.Jump();

        if (input.Skill1Pressed)
            skillController.UseSkill(0);

        if(input.Skill2Pressed)
            skillController.UseSkill(1);
    }


}
