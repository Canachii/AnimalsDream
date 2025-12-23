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
        if(trapCollider != null)
            trapCollider.enabled = false;
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
        trapCollider.enabled = true;
        OnActivate();

        Debug.Log($"[{gameObject.name}] 활성화");
    }

    private void DeactivateTrap()
    {
        isActive = false;
        trapCollider.enabled = false;
        OnDeactivate();

        Debug.Log($"[{gameObject.name}] 비활성화");
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();

    protected virtual void OnTriggerEnter(Collider other) { }

}
