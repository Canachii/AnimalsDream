using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerSkillController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
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
        movement.SetMoveInput(input.MoveInput);

        if (input.JumpPressed)
            movement.Jump();

        if (input.SkillPressed)
            skillController.UseSkill(0);
    }
}
