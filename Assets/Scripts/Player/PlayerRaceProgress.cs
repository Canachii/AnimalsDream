using UnityEngine;

public class PlayerRaceProgress : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;
    public int lastCheckpointIndex = -1;
    public Transform lastCheckpointTransform; 
    public float finishTime;
    public bool finished;

    public int Rank { get; private set; }
    public int DeathCount { get; private set; }

    private void Awake()
    {
        if (!raceManager)
        { 
            raceManager = FindFirstObjectByType<RaceManager>();
            Debug.Assert(raceManager, "[PlayerRaceProgress] RaceManager reference missing.");
        }
        DeathCount = 0;
    }


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

    public void UpdateCheckpoint(int index, Transform transform)
    {
        lastCheckpointIndex = index;
        lastCheckpointTransform = transform;
    }

    public void SetRank(int rank) => Rank = rank;
    public void SetDeathCount(int deathCount) => DeathCount += deathCount;


    private void Update()
    {
        //Debug.Log(name + " Rank: " + Rank);
    }
}
