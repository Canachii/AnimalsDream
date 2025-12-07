using UnityEngine;

public abstract class Skill : MonoBehaviour
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

