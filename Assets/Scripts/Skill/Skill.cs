using UnityEngine;
using Unity.Netcode;

public abstract class Skill : NetworkBehaviour
{
    [Header("Info")]
    public string skillName;
    public Sprite icon;
    public float cooldown = 1f;
    [TextArea] public string description;

    private float lastUseTime = -999f;

    public bool CanUse()
    {
        return Time.time >= lastUseTime + cooldown;
    }
    public float RemainingCooldown
    {
        get
        {
            float remaining = (lastUseTime + cooldown) - Time.time;
            return Mathf.Max(0f, remaining);
        }
    }

    public float CooldownRatio
    {
        get
        {
            if (cooldown <= 0f) return 0f;
            return Mathf.Clamp01(RemainingCooldown / cooldown);
        }
    }
    public bool TryUse(PlayerController user)
    {
        if (!CanUse())
        {
            Debug.Log("Cool down");
            return false;
        }

        OnUse(user);
        lastUseTime = Time.time;
        return true;
    }

    protected abstract void OnUse(PlayerController user);
}

