using UnityEngine;

public class PenguinPassiveSkill : Skill
{
    [Header("Passive Settings")]
    public float detectionRadius = 6f;
    public float[] bonusPerPlayer = { 0f, 0f, 0.1f, 0.2f };

    private int nearbyPlayerCount = 0;
    private float currentSpeedBoost = 0f;

    private PlayerMovement movement;

    protected override void OnUse(PlayerController user) { }

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        skillName = "군집 본능";
        description = "근처 플레이어 2명 이상 → 속도 보너스\n2명: +10%, 3명+: +20%";
        cooldown = 0f;
    }

    void Update()
    {
        UpdateNearbyPlayers();
        ApplyFlockBonus();
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

    void ApplyFlockBonus()
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
