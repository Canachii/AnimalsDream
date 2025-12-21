using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    //순위 계산
    //골인 판정 처리
    // 경기 종료 조건 판단
    //플레이어 등록/해제
    //이밴트 발행
    //OnRankingUpdated(rankingList)
    //OnPlayerFinished(playerId, time)

    private List<PlayerRaceProgress> players = new List<PlayerRaceProgress>();
    [SerializeField] private Transform[] checkPoint;
    int playerCount;

    private void Awake()
    {
        playerCount = players.Count; 
    }

    private void Update()
    {
        UpdateRanks();
    }
    private void UpdateRanks()
    {
        //1. index별로 순위 정하기
        //2. 같은 index일때 다음 
        for(int i =0; i < players.Count; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                if (players[i].lastCheckpointIndex > players[j].lastCheckpointIndex)
                {
                    PlayerRaceProgress temp = players[i];
                    players[i] = players[j];
                    players[j] = temp;
                }
                if(players[i].lastCheckpointIndex == players[j].lastCheckpointIndex)
                {
                    int nextCheckpointIndex = players[i].lastCheckpointIndex + 1;
                    float a = Vector3.Distance(checkPoint[nextCheckpointIndex].position, players[i].transform.position);
                    float b = Vector3.Distance(checkPoint[nextCheckpointIndex].position, players[j].transform.position);
                    if(a < b)
                    {
                        PlayerRaceProgress temp = players[i];
                        players[i] = players[j];
                        players[j] = temp;
                    }
                }
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].SetRank(i + 1);
        }
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
