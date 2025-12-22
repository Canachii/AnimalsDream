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

    // [추가] 강제 전진 모드 플래그 (스킬 사용 시 true)
    private bool isForcedForward = false;

    // Animator parameter hashes
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
            if (playerVcam != null)
            {
                playerVcam.gameObject.SetActive(true);
                playerVcam.Priority = 10; // 메인 카메라에 붙어있는 Brain이 얘를 선택하도록..

                GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCam != null)
                {
                    camTransform = mainCam.transform;
                }
            }
        }
        else
        {
            // 내꺼 아니면 카메라 끄기
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

    // [추가] 외부(스킬)에서 강제 전진 상태를 제어하는 함수
    public void SetForcedForward(bool active)
    {
        isForcedForward = active;
    }

    public void Update()
    {
        if (!IsOwner) return;
        
        // [수정] 강제 전진 중이면 입력이 없어도 속도를 1(최대)로 처리해 달리기 애니메이션 재생
        float speed = isForcedForward ? 1f : moveInput.magnitude;

        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, IsGrounded);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 move;

        // [수정] 강제 전진 모드일 때는 '현재 내 정면'으로 이동 방향 고정
        if (isForcedForward)
        {
            move = transform.forward;
        }
        else
        {
            // 평소에는 카메라 기준 입력 방향으로 이동
            move = GetCameraRelativeMoveDirection(moveInput);
        }

        float moveSqrMag = move.sqrMagnitude;

        if (moveSqrMag > 1f)
        {
            move.Normalize();
            moveSqrMag = 1f;
        }

        // [수정] 강제 전진 중이 아닐 때만 회전 처리 (돌진 중에는 방향 전환 불가)
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

        // 이동 처리
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);

        // 바닥 체크
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
            // Unity 6 등 최신 버전 대응 (구버전이면 rb.velocity 사용)
            Vector3 v = rb.linearVelocity;
            v.y = jumpHeight;
            rb.linearVelocity = v;

            animator.SetTrigger(JumpHash);
        }
    }

    public void SetMoveSpeedMultiplier(float multiplier) // player default speed * multiplier
    {
        moveSpeed = baseMoveSpeed * multiplier;
    }
}