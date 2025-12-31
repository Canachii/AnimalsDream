using UnityEngine;

public class PenguinPassiveSkill : Skill
{
    [Header("Passive Settings")]
    public float detectionRadius = 6f;
    public float[] bonusPerPlayer = { 0f, 0f, 0.1f, 0.2f };

    [Header("Particle Settings")]
    public ParticleSystem speedUpParticle;
    public float baseEmissionRate = 10f;
    public float maxEmissionMultiplier = 3f;

    private int nearbyPlayerCount = 0;
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
        if (!IsServer) return;

        UpdateNearbyPlayers();
        ApplySpeedUp();
        UpdateParticleIntensity();
    }

    void UpdateNearbyPlayers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, -5);
        nearbyPlayerCount = 0;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Player") && col.gameObject != gameObject)
            {
                nearbyPlayerCount++;
            }
        }
    }

    void ApplySpeedUp()
    {
        float targetBoost = 0f;
        if (nearbyPlayerCount < bonusPerPlayer.Length)
        {
            targetBoost = bonusPerPlayer[nearbyPlayerCount];
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

        if (currentSpeedBoost > 0f)
        {
            if (!speedUpParticle.isPlaying)
                speedUpParticle.Play();

            float intensityMultiplier = Mathf.Lerp(0.3f, maxEmissionMultiplier, currentSpeedBoost / 0.2f);

            emissionModule.rateOverTime = baseEmissionRate * intensityMultiplier;

            var main = speedUpParticle.main;
            main.startSizeMultiplier = Mathf.Lerp(0.7f, 1.5f, currentSpeedBoost / 0.2f);
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
