using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : NetworkBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private GameFlow gameFlow;


    //private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction skillAction;
    private InputAction jumpAction;

    private bool jumpPressed;
    private bool skillPressed;

    public bool JumpPressed => jumpPressed;
    public bool SkillPressed => skillPressed;



    public Vector2 RawMoveInput { get; private set; }

    public Vector2 MoveInput => IsMoveInverted ? -RawMoveInput : RawMoveInput;
    private float invertMoveEndTime = -1f;
    public bool IsMoveInverted => Time.time < invertMoveEndTime;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }

            this.enabled = false;
            return;
        }

        if (IsOwner && playerInput != null)
        {
            playerInput.enabled = true;
        }
    }

    public void Awake()
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

        RawMoveInput = Vector2.zero;
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
        RawMoveInput = value.Get<Vector2>();
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


    public void ApplyMoveInvert(float duration)
    {
        invertMoveEndTime = Mathf.Max(invertMoveEndTime, Time.time + duration);
    }
}
