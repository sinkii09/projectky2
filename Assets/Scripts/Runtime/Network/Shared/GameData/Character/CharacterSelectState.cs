using System;
using Unity.Netcode;

public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool IsLockedIn;
    public NetworkString ClientName;

    public CharacterSelectState(ulong clientId, NetworkString clientName ,int characterId = -1, bool isLockedIn = false)
    {
        ClientId = clientId;
        CharacterId = characterId;
        IsLockedIn = isLockedIn;
        ClientName = clientName;
    }
    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId &&
            ClientName == other.ClientName &&
        CharacterId == other.CharacterId &&
        IsLockedIn == other.IsLockedIn;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref ClientName);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsLockedIn);
    }
}
