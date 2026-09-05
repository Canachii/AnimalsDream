using UnityEngine;

public class FanTrap : TimerTrapController
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 20f;
    [SerializeField] private float upForce = 5f;

    [Header("Fan Rotation")]
    [SerializeField] private Transform _fanRotation;
    [SerializeField] private float _fanRotateSpeed;
    [SerializeField] private float _minFanSpeed = 0.0f;
    [SerializeField] private float _maxFanSpeed = 1500f;
    [SerializeField] private float _speedLerp = 2f;

    private float _targetFanSpeed = 0f;

    private int _fanLoopHandle = -1;
    [SerializeField] private GameObject _particle;

    protected override void OnActivate()
    {
        _targetFanSpeed = _maxFanSpeed;

        if (_fanLoopHandle < 0)
            _fanLoopHandle = AudioManager.Instance?.StartLoop3D(SoundId.Trap_Fan, transform) ?? -1;

        if (_particle != null)
            _particle.gameObject.SetActive(true);
    }

    protected override void OnDeactivate()
    {
        _targetFanSpeed = _minFanSpeed;

        if (_fanLoopHandle >= 0)
        {
            AudioManager.Instance?.StopLoop(_fanLoopHandle);
            _fanLoopHandle = -1;
        }

        if (_particle != null)
            _particle.gameObject.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();
        if (_fanRotation == null)
            _fanRotation = transform.Find("Table_fan/Cylinder.002");

        _particle.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_fanRotation == null) return;

        _fanRotation.Rotate(Vector3.up * _fanRotateSpeed * Time.deltaTime);
        _fanRotateSpeed = Mathf.Lerp(_fanRotateSpeed, _targetFanSpeed, Time.deltaTime * _speedLerp);
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
}
