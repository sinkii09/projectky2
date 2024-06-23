using System;
using Unity.Netcode;

public struct CharacterPostGameState : INetworkSerializable, IEquatable<CharacterPostGameState>
{
    public ulong ClientId;
    public NetworkString ClientName;
    public int CharacterId;
    public int Kill;
    public int Dead;
    public int Score;
    public int Place;
    public int RankingPoint;

    public CharacterPostGameState(ulong clientId, NetworkString clientName, int characterId, int kill, int dead, int score, int place, int rankingPoint)
    {
        ClientId = clientId;
        ClientName = clientName;
        CharacterId = characterId;
        Kill = kill;
        Dead = dead;
        Score = score;
        Place = place;
        RankingPoint = rankingPoint;
    }

    public bool Equals(CharacterPostGameState other)
    {
        return ClientId == other.ClientId &&
            ClientName == other.ClientName &&
            CharacterId == other.CharacterId &&
            Kill == other.Kill && Dead == other.Dead &&
            Score == other.Score && Place == other.Place &&
            RankingPoint == other.RankingPoint;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref ClientName);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref Kill);
        serializer.SerializeValue(ref Dead);
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref Place);
        serializer.SerializeValue(ref RankingPoint);
    }
}
