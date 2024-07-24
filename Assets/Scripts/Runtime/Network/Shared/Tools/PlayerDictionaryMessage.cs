using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerDictionaryMessage : INetworkSerializable
{
    public Dictionary<ulong, string> PlayerDictionary;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int count = PlayerDictionary != null ? PlayerDictionary.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            PlayerDictionary = new Dictionary<ulong, string>(count);
            for (int i = 0; i < count; i++)
            {
                ulong key = 0;
                string value = null;
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref value);
                PlayerDictionary[key] = value;
            }
        }
        else
        {
            foreach (var kvp in PlayerDictionary)
            {
                ulong key = kvp.Key;
                string value = kvp.Value;
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref value);
            }
        }
    }
}
