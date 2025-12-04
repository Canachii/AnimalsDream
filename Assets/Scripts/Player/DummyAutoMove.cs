using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerController))]
public class DummyAutoMove : MonoBehaviour
{
    private PlayerController targetController;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Settings")]
    public float changeDirectionTime = 3f; // 방향 바꾸는 주기

    void Start()
    {
        targetController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        // 주기적으로 방향을 바꾸는 코루틴 시작
        StartCoroutine(ChangeDirectionRoutine());
    }

    void FixedUpdate()
    {
        // PlayerController가 가지고 있는 현재 moveSpeed를 가져와서 이동함
        // 즉, 스킬을 맞아서 moveSpeed가 줄어들면 얘도 느려짐
        float currentSpeed = targetController.moveSpeed;

        // 이동 처리
        Vector3 moveStep = moveDirection * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveStep);

        // 회전 처리 (보는 방향으로 몸 돌리기)
        if (moveDirection != Vector3.zero)
        {
            Quaternion newRot = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, newRot, 10f * Time.fixedDeltaTime));
        }
    }

    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            // 랜덤한 방향(X, Z) 결정
            float x = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            moveDirection = new Vector3(x, 0, z).normalized;

            // 지정된 시간만큼 대기
            yield return new WaitForSeconds(changeDirectionTime);
        }
    }
}