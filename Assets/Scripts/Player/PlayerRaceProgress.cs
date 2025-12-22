using UnityEngine;
using UnityEngine.UIElements;

public class PlayerRaceProgress : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;
    public int lastCheckpointIndex = -1;
    public Transform lastCheckointTransform; 
    public float finishTime;
    public bool finished;

    public int Rank { get; private set; }

    private void OnEnable()
    {
        raceManager.RegisterPlayer(this);
        finished = false;
    }
    private void OnDisable()
    {
        raceManager.UnregisterPlayer(this);
        finished = false;
    }

    public void UpdateChekpoint(int index, Transform transform)
    {
        lastCheckpointIndex = index;
        lastCheckointTransform = transform;
    }

    public void SetRank(int rank) => Rank = rank;


    private void Update()
    {
        //Debug.Log(name + " Rank: " + Rank);
    }
}
