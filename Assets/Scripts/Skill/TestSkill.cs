using UnityEngine;

public class TestSkill : Skill
{
    protected override void OnUse(PlayerController user)
    {
        Debug.Log("Use TestSkill");
    }
}
