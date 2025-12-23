using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    private List<PlayerRaceProgress> players = new List<PlayerRaceProgress>();
    [SerializeField] private GameFlow gameFlow;
    [SerializeField] private Transform[] checkPoint;

    private int finishPlayerCount;
    private bool endScheduled;

    [Header("Finish Settings")]
    [SerializeField] private float endDelaySeconds = 3f;

    private void Awake()
    {
        if (!gameFlow) 
        { 
            gameFlow = FindFirstObjectByType<GameFlow>();
            Debug.Assert(gameFlow, "[RaceManager] GameFlow reference missing.");
        }

        finishPlayerCount = 0;
        endScheduled = false;
    }

    private void Update()
    {
        UpdateRanks();
    }

    private void UpdateRanks()
    {
        for (int i = finishPlayerCount; i < players.Count - 1; i++)
        {
            for (int j = i + 1; j < players.Count; j++)
            {
                bool shouldSwap = false;

                if (players[i].lastCheckpointIndex < players[j].lastCheckpointIndex)
                {
                    shouldSwap = true;
                }
                else if (players[i].lastCheckpointIndex == players[j].lastCheckpointIndex)
                {
                    int next = players[i].lastCheckpointIndex + 1;
                    if (next >= checkPoint.Length) continue;

                    float a = (checkPoint[next].position - players[i].transform.position).sqrMagnitude;
                    float b = (checkPoint[next].position - players[j].transform.position).sqrMagnitude;

                    if (a > b) shouldSwap = true;
                }

                if (shouldSwap)
                {
                    var temp = players[i];
                    players[i] = players[j];
                    players[j] = temp;
                }
            }
        }

        for (int i = finishPlayerCount; i < players.Count; i++)
            players[i].SetRank(i + 1);
    }

    public void Finish(PlayerRaceProgress p)
    {

        if (gameFlow.State != MatchState.Playing) return;
        if (p.finished) return;

        p.finished = true;
        p.finishTime = Time.time - gameFlow.MatchStartTime;

        StartCoroutine(ReachedGoalLine());


        if (!endScheduled)
        {
            endScheduled = true;
            StartCoroutine(EndMatchAfterDelay());
        }
        finishPlayerCount++;
    }
    
    private IEnumerator ReachedGoalLine()
    {
        yield return new WaitForSeconds(1f);
        gameFlow.ReachedGoalLine();
    }

    private IEnumerator EndMatchAfterDelay()
    {
        yield return new WaitForSeconds(endDelaySeconds);
        gameFlow.FinishMatch();
    }

    public void RegisterPlayer(PlayerRaceProgress p)
    {
        players.Add(p);
    }

    public void UnregisterPlayer(PlayerRaceProgress p)
    {
        players.Remove(p);
    }

}
