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

    // -------------------------
    // 🔴 Host (JoinCode 생성)
    // -------------------------
    /*public async Task<string> StartHost()
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
    }*/

    public async Task<string> StartHostWithLobby()
    {
        try
        {
            if (_unityTransport == null)
            {
                _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            _unityTransport.SetRelayServerData(new Unity.Networking.Transport.Relay.RelayServerData(allocation, "dtls"));

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    {"JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode)}
                }
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyRoom", 4, options);

            NetworkManager.Singleton.StartHost();

            Debug.Log($"호스트 & 로비 생성 완료! 코드 : {joinCode}");
            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"호스트 생성 실패 : {e.Message}");
            return null;
        }
    }

    // -------------------------
    // 🔵 Client (JoinCode로 접속)
    // -------------------------
    public async Task StartClient(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            _unityTransport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"클라이언트 접속 실패 : {e.Message}");
        }
    }
}
