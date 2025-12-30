using System.Collections;
using UnityEngine;

public class ElectricArea : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private BoxCollider areaBox;

    [Header("Lightning")]
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private int lightningCount = 3;
    [SerializeField] private float strikeActiveTime = 0.3f;
    [SerializeField] private float Delay = 2f;

    private GameObject[] lightnings;
    private bool playerInside;
    private Coroutine loop;

    private void Awake()
    {
        lightnings = new GameObject[lightningCount];
        for (int i = 0; i < lightningCount; i++)
        {
            lightnings[i] = Instantiate(lightningPrefab, transform);
            lightnings[i].SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerInside) return;
        AudioManager.Instance?.PlayAtPoint(SoundId.Trap_Warning,  transform.position);

        playerInside = true;
        loop = StartCoroutine(LightningLoop());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        if (loop != null) StopCoroutine(loop);
        loop = null;

        foreach (var l in lightnings)
            l.SetActive(false);
    }

    private IEnumerator LightningLoop()
    {
        yield return new WaitForSeconds(Delay);
        while (playerInside)
        {
            yield return StartCoroutine(LightningSpawn());
            yield return new WaitForSeconds(Delay);
        }
    }

    private IEnumerator LightningSpawn()
    {
        for (int i = 0; i < lightnings.Length; i++)
        {
            Vector3 pos = RandomPoint();
            lightnings[i].transform.position = pos;
            lightnings[i].SetActive(true);
            AudioManager.Instance?.PlayAtPoint(SoundId.Trap_Thunder, lightnings[i].transform.position);
        }

        yield return new WaitForSeconds(strikeActiveTime);

        for (int i = 0; i < lightnings.Length; i++)
            lightnings[i].SetActive(false);
    }

    private Vector3 RandomPoint()
    {
        Bounds b = areaBox.bounds;

        float x = Random.Range(b.min.x, b.max.x);
        float z = Random.Range(b.min.z, b.max.z);

        return new Vector3(x, 11f, z);
    }

}
