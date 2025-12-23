using UnityEngine;

public class GoalLine : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;

    private void Awake()
    {
        if (!raceManager) 
        {
            raceManager = FindFirstObjectByType<RaceManager>();
            Debug.Assert(raceManager, "[GoalLine] RaceManager reference missing.");
        } 

        GetComponent<Collider>().isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        
        var progress = other.GetComponent<PlayerRaceProgress>();
        if(progress == null)return;

        raceManager.Finish(progress);
    }
}
