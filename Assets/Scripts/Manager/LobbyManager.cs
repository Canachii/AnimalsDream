using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private Text playerListText;

    // 서버에서 관리하고 모든 클라이언트에게 동기화되는 리스트
    private NetworkList<PlayerData> _players = new NetworkList<PlayerData>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // 새로운 플레이어 접속 시 이벤트 등록
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        if (!IsServer)
        {
            startBtn.gameObject.SetActive(false);
        }

        // 리스트가 변할 때마다 UI 업데이트 함수 실행
        _players.OnListChanged += (changeEvent) => UpdatePlayerListUI();

        if (startBtn != null)
        {
            startBtn.gameObject.SetActive(IsServer);
        }

        // 초기 UI 업데이트
        UpdatePlayerListUI();
    }

    private void HandleClientConnected(ulong clientId)
    {
        AddPlayerToList(clientId);
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].ClientId == clientId)
            {
                _players.RemoveAt(i);
                break;
            }
        }
    }

    private void AddPlayerToList(ulong clientId)
    {
        _players.Add(new PlayerData
        {
            ClientId = clientId,
            PlayerName = $"캐릭터 {clientId + 1}" // 추후 랜덤으로 배정될 캐릭터 이름으로 수정해야함
        });
    }

    private void UpdatePlayerListUI()
    {
        if (playerListText == null) return;

        foreach (var player in _players)
        {
            bool isHost = player.ClientId == NetworkManager.ServerClientId;
            string listText = isHost ? "(호스트)" : "";
            playerListText.text += $"{player.PlayerName}{listText}\n";
        }
    }
}
