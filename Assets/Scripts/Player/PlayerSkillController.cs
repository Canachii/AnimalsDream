using Unity.Netcode;
using UnityEngine;

public class PlayerSkillController : NetworkBehaviour
{
    [Header("Skills (slot order)")]
    [SerializeField] private Skill[] skills;

    private PlayerController owner;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();
        if (skills == null || skills.Length == 0)
        {
            skills = GetComponents<Skill>();
        }
    }

    public void UseSkill(int index)
    {
        UseSkillServerRpc(index);
    }

    [ServerRpc]
    private void UseSkillServerRpc(int index)
    {
        if (index >= 0 && index < skills.Length)
        {
            Skill skill = skills[index];
            skill.TryUse(owner);
        }
    }

    public Skill GetSkill(int index)
    {
        if (skills != null && index >= 0 && index < skills.Length)
        {
            return skills[index];
        }
        return null;
    }
}
