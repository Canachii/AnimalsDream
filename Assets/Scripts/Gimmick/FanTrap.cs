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

    protected override void OnActivate()
    {
        _isFanOn = true;
    }

    protected override void OnDeactivate()
    {
        _isFanOn = false;
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
    }
}
