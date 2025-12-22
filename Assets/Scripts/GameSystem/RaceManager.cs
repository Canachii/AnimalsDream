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
                // i가 j보다 뒤면 스왑해야 함 (0번이 1등이니까)
                bool shouldSwap = false;

                if (players[i].lastCheckpointIndex < players[j].lastCheckpointIndex)
                {
                    shouldSwap = true;
                }
                else if (players[i].lastCheckpointIndex == players[j].lastCheckpointIndex)
                {
                    int next = players[i].lastCheckpointIndex + 1;
                    if (next >= checkPoint.Length) continue; // 혹은 next = checkPoint.Length - 1 / next %= checkPoint.Length

                    float a = (checkPoint[next].position - players[i].transform.position).sqrMagnitude;
                    float b = (checkPoint[next].position - players[j].transform.position).sqrMagnitude;

                    // i가 더 멀면 뒤 -> 스왑
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

        StartCoroutine(ReachedGollLine());


        if (!endScheduled)
        {
            endScheduled = true;
            StartCoroutine(EndMatchAfterDelay());
        }
        finishPlayerCount++;
    }
    private IEnumerator ReachedGollLine()
    {
        yield return new WaitForSeconds(1f);
        gameFlow.ReachedGollLine();
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
