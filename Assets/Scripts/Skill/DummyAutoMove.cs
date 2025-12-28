using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class DummyAutoMove : MonoBehaviour
{
    private PlayerMovement targetMovement;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Movement Settings")]
    [Tooltip("방향을 바꾸는 시간 간격 (최소 ~ 최대)")]
    public Vector2 changeDirInterval = new Vector2(0.5f, 2.0f);

    [Header("Random Speed Settings")]
    public float minSpeed = 2.0f;
    public float maxSpeed = 8.0f;

    [Header("Area Settings (플레인 범위)")]
    [Tooltip("이동 가능한 영역의 크기 (X: 좌우 폭, Y: 앞뒤 길이)")]
    public Vector2 areaSize = new Vector2(10f, 20f);

    [Tooltip("이동 영역의 중심점 오프셋")]
    public Vector3 areaOffset = Vector3.zero;

    private float currentRandomSpeed;
    private Vector3 startPosition;

    void Start()
    {
        targetMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
        currentRandomSpeed = (minSpeed + maxSpeed) / 2;

        StartCoroutine(ChangeDirectionRoutine());
    }

    void FixedUpdate()
    {
        Vector3 moveStep = moveDirection * currentRandomSpeed * Time.fixedDeltaTime;
        Vector3 nextPosition = rb.position + moveStep;

        float minX = startPosition.x + areaOffset.x - (areaSize.x / 2);
        float maxX = startPosition.x + areaOffset.x + (areaSize.x / 2);
        float minZ = startPosition.z + areaOffset.y - (areaSize.y / 2);
        float maxZ = startPosition.z + areaOffset.y + (areaSize.y / 2);

        if (nextPosition.z < minZ || nextPosition.z > maxZ)
        {
            moveDirection.z *= -1;
            nextPosition.z = Mathf.Clamp(nextPosition.z, minZ, maxZ);
        }

        nextPosition.x = Mathf.Clamp(nextPosition.x, minX, maxX);

        rb.MovePosition(nextPosition);

        if (moveDirection != Vector3.zero)
        {
            Quaternion newRot = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, newRot, 15f * Time.fixedDeltaTime));
        }
    }

    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            float z = Random.value > 0.5f ? 1f : -1f;
            moveDirection = new Vector3(0, 0, z).normalized;

            currentRandomSpeed = Random.Range(minSpeed, maxSpeed);

            float waitTime = Random.Range(changeDirInterval.x, changeDirInterval.y);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (Application.isPlaying ? startPosition : transform.position) + areaOffset;
        Vector3 size = new Vector3(areaSize.x, 1f, areaSize.y);
        Gizmos.DrawWireCube(center, size);
    }
}