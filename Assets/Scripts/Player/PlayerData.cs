using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using System;

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
    }

    public bool Equals(PlayerData other) => ClientId == other.ClientId;
}