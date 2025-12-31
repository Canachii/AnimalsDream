using UnityEngine;
using Unity.Netcode;

public class HorsePassiveSkill : Skill
{
    [Header("Passive Settings")]
    public float stackInterval = 2.0f;
    public float bonusPerStack = 0.05f;
    public int maxStacks = 3;

    [Header("Particle Settings")]
    public ParticleSystem stackAuraParticle;
    public float baseEmissionRate = 5f;
    public float maxEmissionMultiplier = 1.5f;

    [Header("Debug Info")]
    private readonly NetworkVariable<int> currentStack = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );

    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float moveTimer = 0f;

    private PlayerMovement movement;
    private HorseActiveSkill activeSkill;
    private Vector3 lastPosition;
    private float smoothedSpeed = 0f;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;
    private float originalEmissionRate;

    protected override void OnUse(PlayerController user) { }

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        activeSkill = GetComponent<HorseActiveSkill>();
        skillName = " ";
        description = "이동 시 스택이 쌓이고 속도가 빨라집니다.";
    }

    public override void OnNetworkSpawn()
    {
        currentStack.OnValueChanged += (prev, current) =>
        {
            UpdateParticleByStack(current);

            if (IsOwner) ApplySpeedBonus(current);
        };

        lastPosition = transform.position;

        InitParticle();

        UpdateParticleByStack(currentStack.Value);
    }

    void InitParticle()
    {
        if (stackAuraParticle == null) return;

        emissionModule = stackAuraParticle.emission;

        mainModule = stackAuraParticle.main;

        originalEmissionRate = emissionModule.rateOverTime.constant;

        stackAuraParticle.Stop();
        stackAuraParticle.Clear();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (activeSkill != null && activeSkill.IsRushing)
        {
            if (currentStack.Value > 0)
            {
                ResetStack();
                Debug.Log("[Passive] active On -> stack 0");
            }
            lastPosition = transform.position;

            smoothedSpeed = 0f;
            return;
        }

        Vector3 currentPos = transform.position;
        Vector3 previousPos = lastPosition;
        currentPos.y = 0; previousPos.y = 0;

        float distance = Vector3.Distance(currentPos, previousPos);
        float instantSpeed = distance / Time.deltaTime;

        smoothedSpeed = Mathf.Lerp(smoothedSpeed, instantSpeed, Time.deltaTime * 10f);

        currentSpeed = smoothedSpeed;
        lastPosition = transform.position;

        bool isMoving = (smoothedSpeed > 0.1f) && (movement.IsGrounded || smoothedSpeed > 3.0f);

        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            if (moveTimer >= stackInterval)
            {
                moveTimer = 0f;
                AddStack();
            }
        }
        else
        {
            if (currentStack.Value > 0) ResetStack();
        }
    }

    void AddStack()
    {
        if (currentStack.Value < maxStacks)
        {
            currentStack.Value++;
            Debug.Log($"Passive stack +1 : ({currentStack.Value} Stack)");
        }
    }

    void ResetStack()
    {
        currentStack.Value = 0;
        moveTimer = 0f;
        movement.SetMoveSpeedMultiplier(1.0f);

        Debug.Log("Passive Reset");
    }

    void ApplySpeedBonus(int stack)
    {
        float targetMultiplier = 1.0f + (stack * bonusPerStack);

        movement.SetMoveSpeedMultiplier(targetMultiplier);
    }

    void UpdateParticleByStack(int stack)
    {
        if (stackAuraParticle == null) return;

        if (stack <= 0)
        {
            stackAuraParticle.Stop();
            return;
        }

        if (!stackAuraParticle.isPlaying)
        {
            stackAuraParticle.Clear();
            stackAuraParticle.Play(true);
        }

        float t = Mathf.InverseLerp(1, maxStacks, stack);

        float intensity = Mathf.Lerp(0.5f, maxEmissionMultiplier, t);

        emissionModule.rateOverTime = originalEmissionRate * intensity;

        mainModule.startSizeMultiplier = Mathf.Lerp(0.8f, 1.2f, t);
    }
}