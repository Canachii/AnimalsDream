using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private UnityTransport _unityTransport;

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
}
