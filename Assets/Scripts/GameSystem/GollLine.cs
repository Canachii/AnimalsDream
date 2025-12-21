using UnityEngine;

public class GollLine : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;

    private void Awake()
    {
        if (raceManager == null) raceManager = FindFirstObjectByType<RaceManager>();

        GetComponent<Collider>().isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        //GetComponentInParent
        var profress = other.GetComponent<PlayerRaceProgress>();
        if(profress == null)return;
        raceManager.Finish(profress);
    }
}
