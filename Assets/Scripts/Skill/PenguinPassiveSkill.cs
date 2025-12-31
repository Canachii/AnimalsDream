using UnityEngine;
using Unity.Netcode;

public class PenguinPassiveSkill : Skill
{
    [Header("Passive Settings")]
    public float detectionRadius = 6f;
    public float[] bonusPerPlayer = { 0f, 0f, 0.1f, 0.2f };

    [Header("Particle Settings")]
    public ParticleSystem speedUpParticle;
    public float baseEmissionRate = 10f;
    public float maxEmissionMultiplier = 3f;

    private readonly NetworkVariable<int> nearbyPlayerCount = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private float currentSpeedBoost = 0f;
    private float originalEmissionRate;
    private ParticleSystem.EmissionModule emissionModule;

    private PlayerMovement movement;

    protected override void OnUse(PlayerController user) { }

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        skillName = "군집 본능";
        description = "근처 플레이어 2명 이상 → 속도 보너스\n2명: +10%, 3명+: +20%";
        cooldown = 0f;

        InitializeParticleSystem();
    }

    void InitializeParticleSystem()
    {
        if (speedUpParticle != null)
        {
            emissionModule = speedUpParticle.emission;
            originalEmissionRate = emissionModule.rateOverTime.constant;
            speedUpParticle.Stop();
        }
    }

    void Update()
    {
        if (IsServer)
        {
            UpdateNearbyPlayers();
        }

        if (IsOwner)
        {
            ApplySpeedUp();
        }

        UpdateParticleIntensity();
    }

    void UpdateNearbyPlayers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        int count = 0;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Player") && col.gameObject != gameObject)
            {
                count++;
            }
        }

        nearbyPlayerCount.Value = count;
    }

    void ApplySpeedUp()
    {
        float targetBoost = 0f;
        int count = nearbyPlayerCount.Value;

        if (count < bonusPerPlayer.Length)
        {
            targetBoost = bonusPerPlayer[count];
        }
        else
        {
            targetBoost = bonusPerPlayer[bonusPerPlayer.Length - 1];
        }

        if (!Mathf.Approximately(targetBoost, currentSpeedBoost))
        {
            currentSpeedBoost = targetBoost;
            if (movement != null)
            {
                movement.SetMoveSpeedMultiplier(1f + currentSpeedBoost);
            }
        }
    }

    void UpdateParticleIntensity()
    {
        if (speedUpParticle == null) return;

        int count = nearbyPlayerCount.Value;
        float boostAmount = 0f;

        if (count < bonusPerPlayer.Length) boostAmount = bonusPerPlayer[count];

        else boostAmount = bonusPerPlayer[bonusPerPlayer.Length - 1];

        if (boostAmount > 0f)
        {
            if (!speedUpParticle.isPlaying)
                speedUpParticle.Play();

            float intensityMultiplier = Mathf.Lerp(0.3f, maxEmissionMultiplier, boostAmount / 0.2f);

            emissionModule.rateOverTime = baseEmissionRate * intensityMultiplier;

            var main = speedUpParticle.main;

            main.startSizeMultiplier = Mathf.Lerp(0.7f, 1.5f, boostAmount / 0.2f);
        }
        else
        {
            speedUpParticle.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}