using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 inputVec2;
    private Rigidbody rb;
    private bool isGrounded = true;
    
    [Header ("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 5f;
    public float rotationSpeed = 10f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundMask;

    [Header("Camera")]
    [SerializeField] private Transform camTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    private void FixedUpdate()
    {
        Vector3 move = GetCameraRelativeMoveDirection();

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (move.sqrMagnitude > 0.0001f)
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
        if(isGrounded)
        {
            //rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
            Vector3 v = rb.linearVelocity;
            v.y = jumpHeight;
            rb.linearVelocity = v;
            isGrounded = false;
        }
    }

    public void OnUseSkill(InputValue value)
    {
        Debug.Log("UseSkill");
    }

}
