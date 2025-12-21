using UnityEngine;

public class PlayerRaceProgress : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;
    public int lastCheckpointIndex = -1;
    public int Rank { get; private set; }

    private void OnEnable()
    {
        raceManager.RegisterPlayer(this);
    }
    private void OnDisable()
    {
        raceManager.UnregisterPlayer(this);
    }

    public void UpdateChekpoint(int index)
    {
        lastCheckpointIndex = index;
    }

    public void SetRank(int rank) => Rank =rank;


    private void Update()
    {
        Debug.Log(name + " Rank: " + Rank);
    }
}
