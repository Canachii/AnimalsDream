using System.Collections;
using UnityEngine;

public abstract class TrapController : MonoBehaviour
{
    protected Collider trapCollider;
    protected Renderer trapRenderer;
    protected string target = "Player";

    protected virtual void Awake()
    {
        trapCollider = GetComponent<Collider>();
        trapRenderer = GetComponent<Renderer>();
    }

    protected bool IsTarget(Collider other)
    {
        if (other.CompareTag(target))
            return true;
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(target))
            return true;

        return false;
    }

    protected virtual void OnTriggerStay(Collider other) { }

}
