using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int index;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        other.GetComponent<PlayerRaceProgress>().UpdateChekpoint(index, transform);
    }
}
