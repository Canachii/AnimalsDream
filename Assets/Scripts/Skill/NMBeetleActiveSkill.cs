using UnityEngine;
using Unity.Netcode;

public class BeetleActiveSkill : Skill
{
    [Header("Bite Settings")]
    public float biteDuration = 2.0f;
    public Vector3 biteBoxSize = new Vector3(2f, 1.5f, 2f);
    public float biteOffset = 1.0f;
    public LayerMask targetLayer;

    private BeetlePassiveSkill connectedPassive;

    private void Awake()
    {
        // 같은 오브젝트에 있는 패시브 스킬 미리 찾기
        connectedPassive = GetComponent<BeetlePassiveSkill>();
    }

    protected override void OnUse(PlayerController user)
    {
        // 공격 판정 시작
        PerformBite(user);
    }

    private void PerformBite(PlayerController user)
    {
        //공격 범위(Box) 정의
        Vector3 center = transform.position + transform.forward * biteOffset;
        Collider[] hitColliders = Physics.OverlapBox(center, biteBoxSize / 2, transform.rotation, targetLayer);

        int hitCount = 0;

        foreach (var col in hitColliders)
        {
            if (col.gameObject == user.gameObject) continue;

            PlayerController target = col.GetComponent<PlayerController>();

            //PlayerController가 있는 대상(=다른 플레이어)만 처리
            if (target != null)
            {
                target.ApplyCrowdControlServerRpc(biteDuration);

                hitCount++;
                Debug.Log($"[{skillName}] {target.name} 물기 성공!");
            }
        }

        //한 명이라도 물었다면 패시브(이동속도 증가) 발동
        if (hitCount > 0 && connectedPassive != null)
        {
            connectedPassive.OnBiteSuccess(hitCount);
        }
    }

    // 에디터에서 공격 범위를 눈으로 확인하는 기능
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.forward * biteOffset;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, biteBoxSize);
    }
}