using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 5f;
    public float rotationSpeed = 10f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Camera")]
    [SerializeField] private Transform camTransform;


    private Rigidbody rb;
    private Animator animator;

    private Vector2 moveInput;
    public bool IsGrounded { get; private set; } = true;

    // Animator parameter hashes
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int JumpHash = Animator.StringToHash("jump");

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void Update()
    {
        float speed = moveInput.magnitude;
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, IsGrounded);
    }

    private void FixedUpdate()
    {
        Vector3 move = GetCameraRelativeMoveDirection(moveInput);
        float moveSqrMag = move.sqrMagnitude;

        if (moveSqrMag > 1f)
        {
            move.Normalize();
            moveSqrMag = 1f;
        }

        if (moveSqrMag > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);

            Quaternion newRot = Quaternion.Slerp(
                rb.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
            rb.MoveRotation(newRot);
        }

        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);

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
