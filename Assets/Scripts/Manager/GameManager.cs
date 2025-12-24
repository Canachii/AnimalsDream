using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerSpawnManager spawnManager;
    private const string GameSceneName = "GameScene";

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
        }
    }

    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("호스트 서버 시작 감지. GameScene 로드 시작.");

            // 씬 로드 완료 이벤트 구독
            NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoadComplete;

            // GameScene으로 전환
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
    }

    private void HandleSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == GameSceneName && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("서버 GameScene 로드 완료. 플레이어 스폰 시작");

            spawnManager.InitializeSpawnManager();
            spawnManager.SpawnAllConnectedPlayers();

            NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;
        }
    }
}
