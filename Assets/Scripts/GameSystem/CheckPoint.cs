using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;
    [SerializeField] private int index;


    private void Awake()
    {
        if(raceManager == null) raceManager = FindFirstObjectByType<RaceManager>();

        GetComponent<Collider>().isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        other.GetComponent<PlayerRaceProgress>().UpdateChekpoint(index);
        //raceManager.ReachCheckpoint(index, player);
    }

}
