using System.Collections;
using UnityEngine;

public abstract class TrapController : MonoBehaviour
{
    protected Collider trapCollider;
    protected Renderer trapRenderer;
    //protected Color originColor;
    protected bool isActive = false;


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

        GetComponent<Renderer>().material.color = Color.red;

        Debug.Log($"[{gameObject.name}] 활성화");
    }

    private void DeactivateTrap()
    {
        trapCollider.enabled = false;

        GetComponent<Renderer>().material.color = Color.gray;

        Debug.Log($"[{gameObject.name}] 비활성화");
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();

}
