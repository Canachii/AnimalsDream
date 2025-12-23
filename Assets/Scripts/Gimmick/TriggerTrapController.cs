using UnityEngine;

public abstract class TriggerTrapController : TrapController
{
    protected bool isProcessing;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isProcessing) return;
        if (!IsTarget(other)) return;

        OnTrapTriggered(other);
    }

    protected abstract void OnTrapTriggered(Collider other);
}
