using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    //  ϰ  Ŭ̾Ʈ ȭǴ Ʈ
    private NetworkList<PlayerData> _players = new NetworkList<PlayerData>();
    public NetworkList<PlayerData> Players => _players;

    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // ο ÷̾   ̺Ʈ 
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        // ̹ Ʈ ش ID ִ ȮϿ ߺ 
        foreach (var player in _players)
        {
            if (player.ClientId == clientId) return;
        }

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
            PlayerName = $"Player{clientId + 1}" //    ĳ ̸ ؾ
        });
    }
}
