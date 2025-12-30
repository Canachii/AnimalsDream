using UnityEngine;

public class FanTrap : TimerTrapController
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 20f;
    [SerializeField] private float upForce = 5f;

    [Header("Fan Rotation")]
    [SerializeField] private Transform _fanRotation;
    [SerializeField] private float _fanRotateSpeed;
    [SerializeField] private float _fanAcceleration = 100f;
    [SerializeField] private float _minFanSpeed = 0.0f;
    [SerializeField] private float _maxFanSpeed = 1500f;
    private bool _isFanOn = false;

    private int _fanLoopHandle = -1;

    protected override void OnActivate()
    {
        _isFanOn = true;
        if (_fanLoopHandle < 0)
            _fanLoopHandle = AudioManager.Instance?.StartLoop3D(SoundId.Trap_Fan, transform) ?? -1;
    }

    protected override void OnDeactivate()
    {
        _isFanOn = false;
        if (_fanLoopHandle >= 0)
        {
            AudioManager.Instance?.StopLoop(_fanLoopHandle);
            _fanLoopHandle = -1;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (_fanRotation == null)
            _fanRotation = transform.Find("Table_fan/Cylinder.002");
    }

    private void Update()
    {
        if (_fanRotation == null) return;
        _fanRotation.Rotate(Vector3.up * _fanRotateSpeed * Time.deltaTime);

        if (_isFanOn)
            Acceleration();
        else
            Deceleration();
    }

    protected override void OnTriggerStay(Collider other)
    {
        if (isActive && IsTarget(other))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null) return;

            Vector3 dir = transform.forward;
            dir.y = 0f;
            dir = dir.normalized;

            Vector3 forceDir = dir * knockbackForce + Vector3.up * upForce;
            rb.AddForce(forceDir);
        }
    }

    private void Acceleration()
    {
        _fanRotateSpeed += _fanAcceleration;

        if (_fanRotateSpeed > _maxFanSpeed)
        {
            _fanRotateSpeed = _maxFanSpeed;
        }
    }

    private void Deceleration()
    {
        _fanRotateSpeed -= _fanAcceleration;

        if (_fanRotateSpeed < _minFanSpeed)
        {
            _fanRotateSpeed = _minFanSpeed;
        }

        if (_fanLoopHandle >= 0)
        {
            AudioManager.Instance?.StopLoop(_fanLoopHandle);
            _fanLoopHandle = -1;
        }
    }
}
