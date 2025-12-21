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

    private bool jumpPressed;
    private bool skillPressed;

    public bool JumpPressed => jumpPressed;
    public bool SkillPressed => skillPressed;

    private void Awake()
    {
        if (gameFlow == null)
            gameFlow = FindFirstObjectByType<GameFlow>();

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        skillAction = playerInput.actions["UseSkill"];
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
    }
    private void LockGameplayInput()
    {
        moveAction.Disable();
        skillAction.Disable();
    }

    public void ResetFrameInputFlags()
    {
        jumpPressed = false;
        skillPressed = false;  
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

    //input Key: LeftShift
    public void OnUseSkill(InputValue value)
    {
        if (!value.isPressed) return;
        skillPressed = true;
    }
}
