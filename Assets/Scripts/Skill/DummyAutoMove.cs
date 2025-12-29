using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class DummyAutoMove : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Movement Settings")]
    public Vector2 changeDirInterval = new Vector2(0.5f, 2.0f);

    [Header("Random Speed Settings")]
    public float minSpeed = 2.0f;
    public float maxSpeed = 8.0f;

    [Header("Area Settings")]
    public Vector2 areaSize = new Vector2(10f, 20f);
    public Vector3 areaOffset = Vector3.zero;

    private float currentRandomSpeed;
    private Vector3 startPosition;

    private bool isStunned = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        currentRandomSpeed = (minSpeed + maxSpeed) / 2;
        StartCoroutine(ChangeDirectionRoutine());
    }

    void FixedUpdate()
    {
        if (isStunned) return;

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

    public void ApplyStun(float duration)
    {
        // 호출 확인
        Debug.Log($"[DummyAutoMove] ApplyStun 호출됨 ({duration}초)");

        StopCoroutine("StunRoutine");
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        Debug.Log($"[DummyAutoMove] 멈춤 시작");

        yield return new WaitForSeconds(duration);

        isStunned = false;
        Debug.Log($"[DummyAutoMove] 다시 이동 시작");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (Application.isPlaying ? startPosition : transform.position) + areaOffset;
        Vector3 size = new Vector3(areaSize.x, 1f, areaSize.y);
        Gizmos.DrawWireCube(center, size);
    }
}