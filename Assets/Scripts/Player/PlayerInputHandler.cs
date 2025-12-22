using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{

    [SerializeField] private GameFlow gameFlow;

    public Vector2 MoveInput {  get; private set; }

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction skillAction;
    private InputAction jumpAction;

    private bool jumpPressed;
    private bool skillPressed;

    public bool JumpPressed => jumpPressed;
    public bool SkillPressed => skillPressed;

    private void Awake()
    {
        if (gameFlow == null) gameFlow = FindFirstObjectByType<GameFlow>();        

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        skillAction = playerInput.actions["UseSkill"];
        jumpAction = playerInput.actions["Jump"];

        LockGameplayInput();
    }


    private void OnEnable()
    {
        if (gameFlow != null)
        {
            gameFlow.OnMatchStarted += UnlockGameplayInput;
            gameFlow.OnMatchFinished += LockGameplayInput;
            gameFlow.OnReachedGollLine += LockGameplayInput;
        }
    }

    private void OnDisable()
    {
        if (gameFlow != null)
        {
            gameFlow.OnMatchStarted -= UnlockGameplayInput;
            gameFlow.OnMatchFinished -= LockGameplayInput;
            gameFlow.OnReachedGollLine -= LockGameplayInput;
        }
    }

    private void UnlockGameplayInput()
    {
        moveAction.Enable();
        skillAction.Enable();
        jumpAction.Enable();
    }
    private void LockGameplayInput()
    {
        moveAction.Disable();
        skillAction.Disable();
        jumpAction.Disable();
    }

    public void OnRespawnStarted()
    {
        moveAction.Disable();
        skillAction.Disable();
        jumpAction.Disable();
        //카메라 확정되면 카메라 잠금기능 추가 예정
    }    
    public void OnRespawnFinished()
    {
        moveAction.Enable();
        skillAction.Enable();
        jumpAction.Enable();
        //카메라 확정되면 카메라 잠금기능 해제 추가 예정
    }



    private void LateUpdate()
    {
        ResetFrameInputFlags();
    }
    public void ResetFrameInputFlags()
    {
        jumpPressed = false;
        skillPressed = false;  
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

    //input Key: LeftShift
    public void OnUseSkill(InputValue value)
    {
        if (!value.isPressed) return;
        skillPressed = true;
    }
}
