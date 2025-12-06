using UnityEngine;

public class FanTrap : TrapController
{
    private string target = "Player";

    protected override void OnActivate()
    {

    }

    protected override void OnDeactivate()
    {

    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerStay(Collider other)
    {
        if (isActive && other.CompareTag(target))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null) return;

            Debug.Log($"{gameObject.name}함정 발동");

            Vector3 dir = transform.forward;
            dir.y = 0f;
            dir = dir.normalized;

            Vector3 forceDir = dir * knockbackForce + Vector3.up * upForce;
            rb.AddForce(forceDir);

            Debug.Log("넉백 적용");
        }
    }
}
