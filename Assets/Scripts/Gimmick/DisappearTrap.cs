using System.Collections;
using UnityEngine;

public class DisapperaTrap : TrapController
{

    protected override void OnActivate()
    {

    }

    protected override void OnDeactivate()
    {

    }

    protected override void Start()
    {
        base.Start();
        originColor = trapRenderer.material.color;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        
    }

    private IEnumerator Disappear()
    {
        if (trapRenderer != null)
            trapRenderer.material.color = Color.red;

        yield return new WaitForSeconds(1f);
    }
}
