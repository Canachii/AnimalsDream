using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 5f;
    public float rotationSpeed = 10f;
    private float baseMoveSpeed = 5f;

    public NetworkVariable<float> speedMultiplier = new NetworkVariable<float>(
        1f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Camera")]
    [SerializeField] private Transform camTransform;

    [Header("Camera Setup")]
    [SerializeField] private CinemachineCamera playerVcam;

    private Rigidbody rb;
    private Animator animator;

    private Vector2 moveInput;

    public bool IsGrounded { get; private set; } = true;
    private bool isForcedForward = false;

    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int JumpHash = Animator.StringToHash("jump");

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        baseMoveSpeed = moveSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            rb.isKinematic = false;

            if (playerVcam != null)
            {
                playerVcam.gameObject.SetActive(true);
                playerVcam.Priority = 10;

                GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCam != null)
                {
                    camTransform = mainCam.transform;
                }
            }
        }
        else
        {
            if (playerVcam != null)
            {
                playerVcam.gameObject.SetActive(false);
                playerVcam.Priority = 0;
            }

            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void SetForcedForward(bool active)
    {
        isForcedForward = active;
    }

    public void Update()
    {
        if (!IsOwner) return;

        if (camTransform == null)
        {
            if (Camera.main != null)
            {
                camTransform = Camera.main.transform;
            }
            else
            {
                return;
            }
        }

        float speed = isForcedForward ? 1f : moveInput.magnitude;

        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, IsGrounded);
    }

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        if (IsServer)
        {
            speedMultiplier.Value = multiplier;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 move;

        if (isForcedForward)
        {
            move = transform.forward;
        }
        else
        {
            move = GetCameraRelativeMoveDirection(moveInput);
        }

        float moveSqrMag = move.sqrMagnitude;

        if (moveSqrMag > 1f)
        {
            move.Normalize();
            moveSqrMag = 1f;
        }

        if (!isForcedForward && moveSqrMag > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);

            Quaternion newRot = Quaternion.Slerp(
                rb.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
            rb.MoveRotation(newRot);
        }

        float finalSpeed = baseMoveSpeed * speedMultiplier.Value;
        rb.MovePosition(rb.position + move * finalSpeed * Time.fixedDeltaTime);

        IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f,
                                 Vector3.down,
                                 groundCheckDistance,
                                 groundMask);
    }

    private Vector3 GetCameraRelativeMoveDirection(Vector2 input)
    {
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return forward * input.y + right * input.x;
    }

    public void Jump()
    {
        if (IsGrounded)
        {
            Vector3 v = rb.linearVelocity;
            v.y = jumpHeight;
            rb.linearVelocity = v;

            animator.SetTrigger(JumpHash);
        }
    }
}
