using UnityEngine;

public class FireTrap : TimerTrapController
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private AudioSource _audioSource;

    protected override void OnActivate()
    {
        Play();
    }

    protected override void OnDeactivate()
    {
        Stop();
    }

    protected override void Awake()
    {
        base.Awake();
        if (trapCollider != null)
            trapCollider.enabled = false;
    }

    protected override void Start()
    {
        base.Start();

        if (_particleSystem == null)
            _particleSystem = GetComponentInChildren<ParticleSystem>();

        if (_audioSource == null)
            _audioSource = GetComponentInChildren<AudioSource>();

        Stop();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isActive && other.CompareTag(target))
        {
            Debug.Log("»ç¸Á!");
        }

    }

    private void Stop()
    {
        _particleSystem.Stop();
        _audioSource.Stop();
    }

    private void Play()
    {
        _particleSystem.Play();
        _audioSource.Play();
    }
}
