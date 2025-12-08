using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput {  get; private set; }

    private bool jumpPressed;
    private bool skill1Pressed;
    private bool skill2Pressed;

    public bool JumpPressed => jumpPressed;
    public bool Skill1Pressed => skill1Pressed;
    public bool Skill2Pressed => skill2Pressed;

    public void ResetFrameInputFlags()
    {
        jumpPressed = false;
        skill1Pressed = false;  
        skill2Pressed = false;
    }
    private void LateUpdate()
    {
        ResetFrameInputFlags();
    }

    //input Key: 
    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    //input Key: Space
    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;
        jumpPressed = true;
    }

    //input Key: Q
    public void OnUseSkill1(InputValue value)
    {
        if (!value.isPressed) return;
        skill1Pressed = true;
    }

    //input Key: LeftShift
    public void OnUseSkill2(InputValue value)
    {
        if (!value.isPressed) return;
        skill2Pressed = true;
    }





}
