using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject Bullet;

    NetworkVariable<int> num = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"isOwner? {IsOwner}, ClientID: {OwnerClientId}");
        num.OnValueChanged += (int preValue, int newValue) =>
        {
            //Debug.Log($"OwnerClientID: {OwnerClientId}, num {num.Value}");
        };
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyUp(KeyCode.A))
        {
            SpawnObjectRpc(transform.position, Quaternion.identity);
        }
    }

    [Rpc(SendTo.Server)]
    void SpawnObjectRpc(Vector3 pos, Quaternion rotation)
    {
        Debug.Log("ÃÑ¾Ë ½ºÆù");
        Instantiate(Bullet, pos, rotation)
            .GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
    }
}