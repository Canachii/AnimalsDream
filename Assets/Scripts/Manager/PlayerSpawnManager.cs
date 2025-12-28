using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    private const string GameSceneName = "GameScene";
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> characterPrefabs = new List<GameObject>();
    private int nextSpawnIndex = 0;
    private List<Transform> spawnPoints = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    public void InitializeSpawnManager()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void FindSpawnPoints()
    {
        GameObject[] found = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (found.Length == 0)
        {
            Debug.LogWarning("씬에 SpawnPoint 태그를 가진 오브젝트가 없음");
            spawnPoints.Clear();
            return;
        }

        spawnPoints = found.Select(go => go.transform).OrderBy(t => t.name).ToList();
        Debug.Log($"총 {spawnPoints.Count}개의 스폰포인트를 찾았습니다");
    }

    public void SpawnAllConnectedPlayers()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        FindSpawnPoints();

        // 연결된 모든 클라이언트에게 스폰 명령
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
            Debug.Log($"Late Join 클라이언트 {clientId} 감지. 스폰 요청");
            SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // 스폰 위치 결정
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
            Debug.LogWarning("스폰포인트가 없어서 (0,0,0)에 스폰합니다.");
        }

        // 플레이어 프리팹 인스턴스화
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
            Debug.LogError("플레이어 프리팹에 NetworkObject 컴포넌트가 없음");
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"클라이언트 {clientId} 플레이어 스폰 성공");
    }
}