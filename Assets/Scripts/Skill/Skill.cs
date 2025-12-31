using Unity.Netcode;
using UnityEngine;

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

    public void TryUse(PlayerController user)
    {
        if (!IsOwner)
        {
            TryUseServerRpc(user.NetworkObject.NetworkObjectId);
            return;
        }

        if (!CanUse()) return;

        OnUse(user);
        lastUseTime = Time.time;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TryUseServerRpc(ulong userNetworkObjectId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(userNetworkObjectId, out var netObj))
        {
            PlayerController user = netObj.GetComponent<PlayerController>();
            if (user != null && CanUse())
            {
                OnUse(user);
                lastUseTime = Time.time;
            }
        }
    }

    protected abstract void OnUse(PlayerController user);
}
