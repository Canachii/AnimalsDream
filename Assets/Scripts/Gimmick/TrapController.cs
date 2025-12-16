using System.Collections;
using UnityEngine;

public abstract class TrapController : MonoBehaviour
{
    protected Collider trapCollider;
    protected Renderer trapRenderer;
    //protected Color originColor;
    protected bool isActive = false;

    protected string target = "Player";

    protected virtual void Start()
    {
        trapCollider = GetComponent<Collider>();
        trapRenderer = GetComponent<Renderer>();

        if( trapCollider != null )
        {
            trapCollider.enabled = false;
        }

        StartCoroutine(TrapCycle());
    }

    private IEnumerator TrapCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            ActivateTrap();

            yield return new WaitForSeconds(3f);

            DeactivateTrap();
        }
    }

    private void ActivateTrap()
    {
        trapCollider.enabled = true;

        trapRenderer.material.color = Color.red;

        OnActivate();
        Debug.Log($"[{gameObject.name}] 활성화");
    }

    private void DeactivateTrap()
    {
        trapCollider.enabled = false;

        trapRenderer.material.color = Color.gray;

        OnDeactivate();
        Debug.Log($"[{gameObject.name}] 비활성화");
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();

    protected virtual void OnTriggerStay(Collider other) { }
    protected virtual void OnTriggerEnter(Collider other) { }

}
