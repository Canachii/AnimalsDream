using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private RespawnSystem raceManager;
    [SerializeField] private int index;

    private void Awake()
    {
        if(raceManager == null) raceManager = FindFirstObjectByType<RespawnSystem>();

        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        other.GetComponent<PlayerRaceProgress>().UpdateChekpoint(index);
    }
}
