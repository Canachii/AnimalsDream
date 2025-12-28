using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerController))]
public class DummyAutoMove : MonoBehaviour
{
    private PlayerMovement targetMovement;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Settings")]
    public float changeDirectionTime = 3f;

    [Header("Random Speed Settings")]
    public float minSpeed = 2.0f;
    public float maxSpeed = 8.0f;

    private float currentRandomSpeed;

    void Start()
    {
        targetMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();

        currentRandomSpeed = (minSpeed + maxSpeed) / 2;

        StartCoroutine(ChangeDirectionRoutine());
    }

    void FixedUpdate()
    {
        Vector3 moveStep = moveDirection * currentRandomSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + moveStep);

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
            float z = Random.value > 0.5f ? 1f : -1f;
            moveDirection = new Vector3(0, 0, z).normalized;

            currentRandomSpeed = Random.Range(minSpeed, maxSpeed);

            // Debug.Log($"[Dummy] Dir: {z}, Speed: {currentRandomSpeed:F1}");

            yield return new WaitForSeconds(changeDirectionTime);
        }
    }
}