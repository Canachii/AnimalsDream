using System;
using System.Collections;
using UnityEngine;

public enum MatchState
{
    Lobby,
    Countdown,
    Playing,
    Finished
}

public class GameFlow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float countdownSeconds = 3f;

    public MatchState State { get; private set; } = MatchState.Lobby;

    public event Action<MatchState> OnStateChanged;
    public event Action<int> OnCountdownTick; // 3,2,1...
    public event Action OnMatchStarted;
    public event Action OnMatchFinished;

    public float MatchStartTime { get; private set; }

    public void Start()
    {
        StartCountdown();
    }

    public void StartCountdown()
    {
        if(State != MatchState.Lobby) return;   
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        SetState(MatchState.Countdown);

        int sec = Mathf.CeilToInt(countdownSeconds);

        while(sec > 0)
        {
            Debug.Log(sec+"√ ");
            OnCountdownTick?.Invoke(sec);
            yield return new WaitForSeconds(1f);
            sec--;
        }

        MatchStartTime = Time.time;
        SetState(MatchState.Playing);
        OnMatchStarted?.Invoke();
    }

    public void FinishMatch()
    {
        if(State != MatchState.Playing) return; 
        SetState(MatchState.Finished);
        OnMatchFinished?.Invoke();
    }
    public void SetState(MatchState next)
    {
        State = next;
        OnStateChanged?.Invoke(State);
    }

}