using UnityEngine;

public class FireTrap : TrapController
{
    [SerializeField] private ParticleSystem _particleSystem;

    protected override void OnActivate()
    {
        _particleSystem.Play();
    }

    protected override void OnDeactivate()
    {
        _particleSystem.Stop();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isActive && other.CompareTag(target))
        {
            Debug.Log("»ç¸Á!");
        }

        if(_particleSystem == null)
            _particleSystem = GetComponentInChildren<ParticleSystem>();

        if( _particleSystem != null)
            _particleSystem.Stop();
    }
}
