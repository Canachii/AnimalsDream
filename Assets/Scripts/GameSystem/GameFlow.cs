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
    [SerializeField] private float matchTimeLimit = 300f;

    // 서버만 쓸 수 있고, 모두가 읽을 수 있는 값들
    private readonly NetworkVariable<MatchState> netState =
        new(MatchState.Lobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> netCountdownSec =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private readonly NetworkVariable<double> netMatchStartTime =
    new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public double MatchStartTime => netMatchStartTime.Value;
    public MatchState State => netState.Value;
    public int CountdownSec => netCountdownSec.Value;

    public event Action<MatchState> OnStateChanged;
    public event Action<int> OnCountdownChanged; //UI 갱신용
    public event Action OnMatchStarted;
    public event Action OnMatchFinished;
    public event Action OnReachedGoalLine;


    public float RemainingTime
    {
        get
        {
            if (State != MatchState.Playing) return matchTimeLimit;
            if (NetworkManager == null) return matchTimeLimit;

            double now = NetworkManager.ServerTime.Time;
            double elapsed = now - MatchStartTime;
            return Mathf.Max(0f, matchTimeLimit - (float)elapsed);
        }
    }

    public override void OnNetworkSpawn()
    {
        // 모든 클라(호스트 포함)에서 동기화 값 변경 감지
        netState.OnValueChanged += HandleStateChanged;
        netCountdownSec.OnValueChanged += HandleCountdownChanged;

        // 늦게 들어온 클라도 현재 상태 1번 반영
        OnStateChanged?.Invoke(netState.Value);
        OnCountdownChanged?.Invoke(netCountdownSec.Value);
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
        SetStateServer(MatchState.Countdown);

        int sec = Mathf.CeilToInt(countdownSeconds);
        netCountdownSec.Value = sec;

        PlayCountdownSfxClientRpc();

        StartCoroutine(CoCountdownServer(sec));
    }

    private IEnumerator CoCountdownServer(int sec)
    {
        // 3을 1초 보여주고 2로 내려가도록 1초 대기 후 감소
        while (sec > 0)
        {
            yield return new WaitForSeconds(1f);
            sec--;
            netCountdownSec.Value = sec;
        }

        netMatchStartTime.Value = NetworkManager.ServerTime.Time;
        SetStateServer(MatchState.Playing);
    }

    [ClientRpc]
    private void PlayCountdownSfxClientRpc()
    {
        AudioManager.Instance?.Play(SoundId.Race_Countdown);
    }

    public void FinishMatch()
    {
        if (!IsServer) return;
        if (netState.Value != MatchState.Playing) return;

        SetStateServer(MatchState.Finished);
    }

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
        OnCountdownChanged?.Invoke(next);
    }

    private void HandleStateChanged(MatchState prev, MatchState next)
    {
        OnStateChanged?.Invoke(next);

        if (next == MatchState.Playing) OnMatchStarted?.Invoke();
        if (next == MatchState.Finished) OnMatchFinished?.Invoke();
    }
}
