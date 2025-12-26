using System.Collections;
using UnityEngine;

public abstract class TimerTrapController : TrapController
{
    [SerializeField] protected float trapActiveTime = 3f;
    [SerializeField] protected float trapDuration = 1f;

    protected bool isActive;

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Start()
    {
        StartCoroutine(TrapCycle());
    }

    private IEnumerator TrapCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(trapActiveTime);
            ActivateTrap();

            yield return new WaitForSeconds(trapDuration);
            DeactivateTrap();
        }
    }

    private void ActivateTrap()
    {
        isActive = true;
        OnActivate();
    }

    private void DeactivateTrap()
    {
        isActive = false;
        OnDeactivate();
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();

    protected virtual void OnTriggerEnter(Collider other) { }

}
