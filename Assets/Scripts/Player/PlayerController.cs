using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private Vector2 inputVec2;
    private Rigidbody rb;
    private bool isGrounded = true;
    private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int JumpHash = Animator.StringToHash("jump");

    [Header ("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 5f;
    public float rotationSpeed = 10f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    private Skill skill;

    [Header("Camera")]
    [SerializeField] private Transform camTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        skill = GetComponent<Skill>();
        animator = GetComponent<Animator>();    
    }

    private void Update()
    {
        float speed = inputVec2.magnitude;
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, isGrounded);
    }

    private void FixedUpdate()
    {
        Vector3 move = GetCameraRelativeMoveDirection();
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

        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f,
                                 Vector3.down,
                                 groundCheckDistance,
                                 groundMask);
    }


    private Vector3 GetCameraRelativeMoveDirection()
    {
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return forward * inputVec2.y + right * inputVec2.x;
    }

    public void OnMove(InputValue value)
    {
        inputVec2 = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        //if (!value.isPressed) return;

        if (isGrounded)
        {
            //rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
            Vector3 v = rb.linearVelocity;
            v.y = jumpHeight;
            rb.linearVelocity = v;

            animator.SetTrigger(JumpHash);
        }
    }

    public void OnUseSkill(InputValue value)
    {
        if (!value.isPressed) return;
        skill.TryUse(this);
    }

}
