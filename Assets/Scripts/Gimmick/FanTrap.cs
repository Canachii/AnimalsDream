using UnityEngine;

public class FanTrap : TrapController
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 20f;
    [SerializeField] private float upForce = 5f;

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
}
