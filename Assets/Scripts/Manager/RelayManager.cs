using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private UnityTransport _unityTransport;

    private Lobby _currentLobby;
    private float _heartbeatTimer;

    private async void Awake()
    {
        if (Instance == null)
            Instance = this;

        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Unity Services Initialized!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unity Services Init ERROR : {e.Message}");
        }
    }

    private void Update()
    {
        if (_currentLobby != null && NetworkManager.Singleton.IsHost)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = 15f;
                LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
        }
    }

    // -------------------------
    // 🔴 Host (JoinCode 생성)
    // -------------------------
    public async Task<string> StartHost()
    {
        try
        {
            if (_unityTransport == null)
            {
                _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }

            if (_unityTransport == null)
            {
                Debug.LogError("UnityTransport를 찾을 수 없음");
                return null;
            }

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            _unityTransport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartHost();

            Debug.Log($"✅ 호스트 시작 및 조인 코드: {joinCode}");
            return joinCode;
        }
        
        catch (System.Exception e)
        {
            Debug.LogError("호스트 생성 실패");
            return null;
        }
    }

    public async Task<string> StartHostWithLobby()
    {
        string joinCode = await StartHost(); // 기존 호스트 로직 실행

        if (!string.IsNullOrEmpty(joinCode))
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        {"JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode)}
                    }
                };

                _currentLobby = await LobbyService.Instance.CreateLobbyAsync("RandomRoom", 4, options);
                Debug.Log($"로비 생성 완료 ID : {_currentLobby.Id}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("로비 생성 실패");
            }
        }
        return joinCode;
    }

    // -------------------------
    // 🔵 Client (JoinCode로 접속)
    // -------------------------
    public async Task StartClient(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
    }

    // -------------------------
    // 🔵 Client (Quick Join 접속)
    // -------------------------
    public async Task QuickJoin()
    {
        try
        {
            // 참여 가능한 로비 찾기
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            _currentLobby = lobby;

            // 로비 데이터에서 JoinCode 꺼내기
            string joinCode = lobby.Data["JoinCode"].Value;

            // 해당 코드로 클라이언트 접속
            await StartClient(joinCode);
            Debug.Log($"퀵 조인 성공 코드 : {joinCode}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("참여 가능한 로비가 없음");
            throw e;
        }
    }
}
