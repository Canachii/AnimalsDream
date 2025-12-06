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
    public float changeDirectionTime = 3f;

    void Start()
    {
        targetController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();

        StartCoroutine(ChangeDirectionRoutine());
    }

    void FixedUpdate()
    {
        float currentSpeed = targetController.moveSpeed;

        Vector3 moveStep = moveDirection * currentSpeed * Time.fixedDeltaTime;
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
            float x = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            moveDirection = new Vector3(x, 0, z).normalized;

            yield return new WaitForSeconds(changeDirectionTime);
        }
    }
}