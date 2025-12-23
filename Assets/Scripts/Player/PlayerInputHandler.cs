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
        if (!gameFlow)
        {
            gameFlow = FindFirstObjectByType<GameFlow>();
            Debug.Assert(gameFlow, "[PlayerInputHandler] GameFlow reference missing.");
        }

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
            gameFlow.OnReachedGoalLine += LockGameplayInput;
        }
    }

    private void OnDisable()
    {
        if (gameFlow != null)
        {
            gameFlow.OnMatchStarted -= UnlockGameplayInput;
            gameFlow.OnMatchFinished -= LockGameplayInput;
            gameFlow.OnReachedGoalLine -= LockGameplayInput;
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
        // TODO: Add camera lock feature.
    }

    public void OnRespawnFinished()
    {
        moveAction.Enable();
        skillAction.Enable();
        jumpAction.Enable();
        // TODO: Add camera lock feature.
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
