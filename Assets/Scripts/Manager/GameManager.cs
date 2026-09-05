using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class GameManager : MonoBehaviour
{
    private PlayerSpawnManager spawnManager;
    private const string GameSceneName = "GameScene";

    private readonly HashSet<ulong> _loadedClients = new HashSet<ulong>();
    private bool _gameSceneFlowStarted;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        spawnManager = Object.FindAnyObjectByType<PlayerSpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("GameManager : PlayerSpawnManager를 찾을 수 없습니다.");
            enabled = false;
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            if (NetworkManager.Singleton.SceneManager != null)
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;
        }
    }

    private void HandleServerStarted()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        Debug.Log("호스트 서버 시작 감지. GameScene 로드 시작.");

        // 씬 로드 완료 이벤트 구독
        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoadComplete;

        _loadedClients.Clear();
        _gameSceneFlowStarted = false;
    }

    private void HandleSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // 서버(호스트)에서만 집계
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        if (sceneName != GameSceneName) return;

        _loadedClients.Add(clientId);

        int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        int loadedCount = _loadedClients.Count;

        Debug.Log($"[GameManager] GameScene load complete: client={clientId} loaded={loadedCount}/{connectedCount}");


        if (_gameSceneFlowStarted) return;
        if (loadedCount != connectedCount) return;

        Debug.Log("[GameManager] All clients loaded GameScene. Spawn + HUD + Countdown start.");

        _gameSceneFlowStarted = true;

        // 1) 플레이어 스폰(서버에서)
        spawnManager.InitializeSpawnManager();
        spawnManager.SpawnAllConnectedPlayers();

        // 2) HUD 세팅(각 클라에서 필요)
        SetupHudClientRpc();

        // 3) 카운트다운 시작(서버에서 GameFlow 찾은 뒤)
        StartCoroutine(CoStartCountdownAfterGameFlowReady());

        NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;
    }

    private IEnumerator CoStartCountdownAfterGameFlowReady()
    {
        // 서버에서 GameFlow 준비될 때까지 대기
        GameFlow gf = null;
        while (gf == null)
        {
            gf = FindFirstObjectByType<GameFlow>();
            yield return null;
        }

        while (gf.NetworkObject == null || !gf.NetworkObject.IsSpawned)
            yield return null;

        gf.StartCountdown();
        Debug.Log("[GameManager] Countdown started by server.");
    }

    [ClientRpc]
    private void SetupHudClientRpc()
    {
        SetupHUD_Local();
    }

    private void SetupHUD_Local()
    {
        var docs = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        foreach (var doc in docs)
        {
            if (doc.visualTreeAsset != null && doc.visualTreeAsset.name.Contains("GameHUDView"))
            {
                if (doc.GetComponent<GameHUDController>() == null)
                {
                    doc.gameObject.AddComponent<GameHUDController>();
                }
                break;
            }
        }
    }
}
