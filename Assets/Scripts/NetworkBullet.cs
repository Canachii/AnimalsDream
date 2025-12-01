//NetworkBullet.cs
using Unity.Netcode;
using UnityEngine;

public class NetworkBullet : NetworkBehaviour
{
    public override async void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        await Awaitable.WaitForSecondsAsync(3);

        DespawnObjectRpc(NetworkObject);
    }


    void Update()
    {
        if (!IsOwner) return; // 소유자만 이동 처리
        transform.Translate(Vector3.up * Time.deltaTime * 10);
    }

    //서버에게 오브젝트 삭제 요청을 한다.
    [Rpc(SendTo.Server)]
    void DespawnObjectRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            targetObject.Despawn();
        }
    }
}