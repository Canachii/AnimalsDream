using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum MatchState
{
    Lobby,
    Countdown,
    Playing,
    Finished
}

public class GameFlow : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float countdownSeconds = 3f;

    // 서버만 쓸 수 있고, 모두가 읽을 수 있는 값들
    private readonly NetworkVariable<MatchState> netState =
        new(MatchState.Lobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> netCountdownSec =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private readonly NetworkVariable<double> netMatchStartTime =
    new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public double MatchStartTime => netMatchStartTime.Value;

    public MatchState State => netState.Value;

    public event Action<MatchState> OnStateChanged;
    public event Action<int> OnCountdownTick; // 3,2,1...
    public event Action OnMatchStarted;
    public event Action OnMatchFinished;
    public event Action OnReachedGoalLine;

    public override void OnNetworkSpawn()
    {
        // 모든 클라(호스트 포함)에서 동기화 값 변경 감지
        netState.OnValueChanged += HandleStateChanged;
        netCountdownSec.OnValueChanged += HandleCountdownChanged;

        // 씬 로드 후 서버(호스트)에서만 카운트다운 시작
        if (IsServer && netState.Value == MatchState.Lobby)
        {
            StartCountdown();
        }

        // 늦게 들어온 클라도 현재 상태 1번 반영
        OnStateChanged?.Invoke(netState.Value);
    }

    public override void OnNetworkDespawn()
    {
        netState.OnValueChanged -= HandleStateChanged;
        netCountdownSec.OnValueChanged -= HandleCountdownChanged;
    }

    public void StartCountdown()
    {
        if (!IsServer) return;
        if (netState.Value != MatchState.Lobby) return;

        StopAllCoroutines();
        StartCoroutine(CoCountdownServer());
    }

    private IEnumerator CoCountdownServer()
    {
        SetStateServer(MatchState.Countdown);

        int sec = Mathf.CeilToInt(countdownSeconds);
        netCountdownSec.Value = sec;

        while (sec > 0)
        {
            Debug.Log(sec);
            yield return new WaitForSeconds(1f);
            sec--;
            netCountdownSec.Value = sec;
        }
        netMatchStartTime.Value = NetworkManager.ServerTime.Time;

        SetStateServer(MatchState.Playing);
    }

    public void FinishMatch()
    {
        if (!IsServer) return;
        if (netState.Value != MatchState.Playing) return;

        SetStateServer(MatchState.Finished);
    }

    // GoalLine은 상황에 따라: 서버에서 판단해서 모두에게 알림
    public void ReachedGoalLine()
    {
        if (IsServer) ReachedGoalLineClientRpc();
        else ReachedGoalLineServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReachedGoalLineServerRpc()
    {
        ReachedGoalLineClientRpc();
    }

    [ClientRpc]
    private void ReachedGoalLineClientRpc()
    {
        OnReachedGoalLine?.Invoke();
    }

    private void SetStateServer(MatchState next)
    {
        if (!IsServer) return;
        netState.Value = next;
    }

    private void HandleCountdownChanged(int prev, int next)
    {
        // next: 3,2,1,0 으로 떨어짐
        if (prev <= 0 && next > 0)
            OnCountdownTick?.Invoke(next);
    }

    private void HandleStateChanged(MatchState prev, MatchState next)
    {
        OnStateChanged?.Invoke(next);

        if (next == MatchState.Playing) OnMatchStarted?.Invoke();
        if (next == MatchState.Finished) OnMatchFinished?.Invoke();
    }
}
