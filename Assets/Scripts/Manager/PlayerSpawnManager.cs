using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> characterPrefabs = new List<GameObject>();
    private List<Transform> spawnPoints = new List<Transform>();
    private int nextSpawnIndex = 0;

    private const string GameSceneName = "GameScene";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeSpawnManager()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    private void FindSpawnPoints()
    {
        GameObject[] found = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (found.Length == 0)
        {
            Debug.LogWarning("���� SpawnPoint �±׸� ���� ������Ʈ�� ����");
            spawnPoints.Clear();
            return;
        }

        spawnPoints = found.Select(go => go.transform).OrderBy(t => t.name).ToList();
        Debug.Log($"�� {spawnPoints.Count}���� ��������Ʈ�� ã�ҽ��ϴ�");
    }

    public void SpawnAllConnectedPlayers()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        FindSpawnPoints();

        // ����� ��� Ŭ���̾�Ʈ���� ���� ����
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId) == null)
            {
                SpawnPlayerForClient(clientId);
            }
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer &&
            SceneManager.GetActiveScene().name == GameSceneName)
        {
            Debug.Log($"Late Join Ŭ���̾�Ʈ {clientId} ����. ���� ��û");
            SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // ���� ��ġ ����
        Transform spawnPos;
        if (spawnPoints.Count > 0)
        {
            spawnPos = spawnPoints[nextSpawnIndex];
            nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Count;
        }
        else
        {
            spawnPos = new GameObject().transform;
            spawnPos.position = Vector3.zero;
            Debug.LogWarning("��������Ʈ�� ��� (0,0,0)�� �����մϴ�.");
        }

        // �÷��̾� ������ �ν��Ͻ�ȭ
        GameObject selectedPrefab = playerPrefab;

        if (characterPrefabs != null && characterPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, characterPrefabs.Count);
            selectedPrefab = characterPrefabs[randomIndex];
        }

        if (selectedPrefab == null)
        {
            Debug.LogError("SpawnPlayerForClient: No player prefab assigned!");
            return;
        }

        GameObject playerObj = Instantiate(selectedPrefab, spawnPos.position, spawnPos.rotation);

        NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("�÷��̾� �����տ� NetworkObject ������Ʈ�� ����");
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"Ŭ����Ʈ {clientId} �÷��̾� ���� ���� ({selectedPrefab.name})");
    }
}
