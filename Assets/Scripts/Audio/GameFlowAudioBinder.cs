using UnityEngine;

public class GameFlowAudioBinder : MonoBehaviour
{
    [SerializeField] private GameFlow gameFlow;

    private void Awake()
    {
        if (!gameFlow)
            gameFlow = FindFirstObjectByType<GameFlow>();
        AudioManager.Instance?.StopBgm(fade: true);
    }

    private void OnEnable()
    {
        if (!gameFlow) return;

        gameFlow.OnMatchStarted += OnStart;
        gameFlow.OnMatchFinished += OnEnd;
    }

    private void OnDisable()
    {
        if (!gameFlow) return;

        gameFlow.OnMatchStarted -= OnStart;
        gameFlow.OnMatchFinished -= OnEnd;
    }


    private void OnStart()
    {
        AudioManager.Instance?.PlayBgm(SoundId.Race_BGM, fade: true, restart: false);
    }


    private void OnEnd()
    {
        AudioManager.Instance?.Play(SoundId.Race_GameEnd);
        AudioManager.Instance?.StopBgm(fade: true);
    }
}
