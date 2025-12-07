using System;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Skills (slot order)")]
    [SerializeField] private Skill[] skills;

    private PlayerController owner;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();

        if(skills == null ||  skills.Length == 0)
        {
            skills = GetComponents<Skill>();   
        }
    }

    public void UseSkill(int index)
    {
        Skill skill = skills[index];
        skill.TryUse(owner);
    }



}
