using System.Collections;
using UnityEngine;

public class JumpingTrap : TriggerTrapController
{
    [Header("Jump")]
    [SerializeField] private float upForce = 10f;
    [SerializeField] private float forwardForce = 7f;
    [SerializeField] private float delay = 0.1f;
    private Animator animator;

    protected override void OnTrapTriggered(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        animator = GetComponent<Animator>();

        animator.SetBool("active", true);
        animator.SetTrigger("Bounce");
        StartCoroutine(Jump(rb));
    }

    private IEnumerator Jump(Rigidbody rb)
    {
        Vector3 enter = rb.linearVelocity;
        Vector3 horizontalDir = new Vector3(enter.x, 0f, enter.z);

        if (horizontalDir.sqrMagnitude < 0.01f)
            horizontalDir = transform.forward;

        horizontalDir = horizontalDir.normalized;

        yield return new WaitForSeconds(delay);
        if (rb == null) yield break;

        rb.linearVelocity = Vector3.zero;

        Vector3 upVel = Vector3.up * upForce;
        Vector3 forwardVel = horizontalDir * forwardForce;

        Vector3 finalVel = upVel + forwardVel;

        rb.AddForce(finalVel, ForceMode.VelocityChange);
        animator.SetBool("active", false);
    }
}
