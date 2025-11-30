using UnityEngine;

public class TestSkill : Skill
{
    protected override void OnUse(PlayerController user)
    {
        Debug.Log("테스트 스킬 사용");
    }
}
