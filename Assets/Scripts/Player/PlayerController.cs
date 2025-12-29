using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerSkillController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : NetworkBehaviour
{
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerInputHandler input;

    // 행동 불가(CC) 상태 플래그
    private bool isCrowdControlled = false;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        skillController = GetComponent<PlayerSkillController>();
        input = GetComponent<PlayerInputHandler>();

        if (movement == null || skillController == null || input == null)
        {
            Debug.LogError($"[{name}] PlayerController is missing required components.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // 내 캐릭터가 아니면 조작 불가

        // [중요] 행동 불가 상태라면 -> 이동 및 스킬 차단
        if (isCrowdControlled)
        {
            movement.SetMoveInput(Vector2.zero); // 강제 정지
            return;
        }

        // --- 평소 상태 ---
        movement.SetMoveInput(input.MoveInput);

        if (input.JumpPressed)
            movement.Jump();

        if (input.SkillPressed)
            skillController.UseSkill(0);
    }

    public void ApplyCrowdControl(float duration)
    {
        // 1. 네트워크에 스폰된 상태인지 확인 (멀티플레이어)
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            // 공격자가 서버에게 요청 (RequireOwnership = false 필수)
            ApplyCrowdControlServerRpc(duration);
        }
        else
        {
            // 2. 네트워크가 없는 상태 (더미, 싱글 테스트)
            // 그냥 바로 적용
            StartCoroutine(CrowdControlRoutine(duration));
        }
    }

    // [Server] 서버가 받아서 모든 클라이언트(특히 피해자)에게 전파
    [Rpc(SendTo.Everyone)]
    private void ApplyCrowdControlServerRpc(float duration)
    {
        ApplyCrowdControlClientRpc(duration);
    }

    // [Client] 피해자 본인 컴퓨터에서 실행됨
    [ClientRpc]
    private void ApplyCrowdControlClientRpc(float duration)
    {
        // 코루틴 실행 -> isCrowdControlled = true -> Update에서 입력 막힘
        StartCoroutine(CrowdControlRoutine(duration));
    }

    // 실제 로직 (상태 변경 및 타이머)
    private IEnumerator CrowdControlRoutine(float duration)
    {
        // 기존 상태 초기화 (연속으로 맞았을 때 꼬임 방지)
        StopCoroutine("CrowdControlRoutine");

        isCrowdControlled = true;
        // Debug.Log($"[{name}] 으악! 움직일 수 없어! ({duration}초)");

        yield return new WaitForSeconds(duration);

        isCrowdControlled = false;
        // Debug.Log($"[{name}] 이제 움직일 수 있어.");
    }
}