using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerDataMessage : INetworkSerializable
{
    public ulong networkId;
    public string userId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref networkId);
        serializer.SerializeValue(ref userId);
    }
}