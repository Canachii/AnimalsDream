using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RaceManager : NetworkBehaviour
{
    private readonly List<PlayerRaceProgress> players = new();
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

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            finishPlayerCount = 0;
            endScheduled = false;
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (checkPoint == null || checkPoint.Length == 0) return;
        UpdateRanks();
    }

    private void UpdateRanks()
    {
        players.RemoveAll(p => p == null);

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
                    (players[i], players[j]) = (players[j], players[i]);
                }
            }
        }

        for (int i = 0; i < players.Count; i++)
            players[i].SetRank(i + 1);
    }

    public void Finish(PlayerRaceProgress p)
    {
        if (p == null) return;
        if (NetworkManager.Singleton == null) return;

        if (IsServer)
        {
            FinishInternal(p);
            return;
        }

        var netObj = p.GetComponent<NetworkObject>();
        if (netObj == null) return;
        
        FinishServerRpc(netObj);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FinishServerRpc(NetworkObjectReference playerRef)
    {
        if (!IsServer) return;

        if (!playerRef.TryGet(out var netObj)) return;
        var progress = netObj.GetComponent<PlayerRaceProgress>();
        if (progress == null) return;
        
        FinishInternal(progress);
    }

    private void FinishInternal(PlayerRaceProgress p)
    {
        if (!IsServer) return;
        if (gameFlow.State != MatchState.Playing) return;
        if (p.finished) return;

        int idx = players.IndexOf(p);
        if (idx < 0)
        {
            players.Add(p);
            idx = players.Count - 1;
        }

        if (idx != finishPlayerCount)
        {
            var temp = players[finishPlayerCount];
            players[finishPlayerCount] = p;
            players[idx] = temp;
        }

        p.finished = true;
        p.finishTime = (float)(NetworkManager.Singleton.ServerTime.Time - gameFlow.MatchStartTime);

        finishPlayerCount++;

        // 완주 직후 랭크 반영
        for (int i = 0; i < players.Count; i++)
            players[i].SetRank(i + 1);

        StartCoroutine(ReachedGoalLine());

        if (!endScheduled)
        {
            endScheduled = true;
            StartCoroutine(EndMatchAfterDelay());
        };
    }
    
    private IEnumerator ReachedGoalLine()
    {
        yield return new WaitForSeconds(1f);
        if (IsServer) gameFlow.ReachedGoalLine();
    }

    private IEnumerator EndMatchAfterDelay()
    {
        yield return new WaitForSeconds(endDelaySeconds);
        if (IsServer) gameFlow.FinishMatch();
    }

    public void RegisterPlayer(PlayerRaceProgress p)
    {
        if (p == null) return;
        if (players.Contains(p)) return;
        players.Add(p);
    }

    public void UnregisterPlayer(PlayerRaceProgress p)
    {
        if (p == null) return;

        int idx = players.IndexOf(p);
        if (idx < 0) return;

        players.RemoveAt(idx);

        if (IsServer && idx < finishPlayerCount)
            finishPlayerCount = Mathf.Max(0, finishPlayerCount - 1);
    }

    public int PlayerCount => players.Count;
}
