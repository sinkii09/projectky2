using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct CharacterInGameState : INetworkSerializable, IEquatable<CharacterInGameState>
{
    public ulong ClientId;
    public NetworkString ClientName;
    public int CharacterId;
    public bool IsAlive;
    public int Kill;
    public int Dead;
    public int Health;
    public float DeadTime;

    public CharacterInGameState(ulong clientId, NetworkString clientName, int characterId, bool isAlive, int health, int kill = 0, int dead = 0, float deadTime = 0)
    {
        ClientId = clientId;
        ClientName = clientName;
        CharacterId = characterId;
        IsAlive = isAlive;
        Health = health;
        Kill = kill;
        Dead = dead;
        DeadTime = deadTime;
    }

    public bool Equals(CharacterInGameState other)
    {
        return ClientId == other.ClientId &&
            ClientName == other.ClientName &&
            CharacterId == other.CharacterId &&
            IsAlive == other.IsAlive &&
            Health == other.Health &&
            Kill == other.Kill && 
            Dead == other.Dead &&
            DeadTime == other.DeadTime;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref ClientName);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsAlive);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref Kill);
        serializer.SerializeValue(ref Dead);
        serializer.SerializeValue(ref DeadTime);
    }
}
