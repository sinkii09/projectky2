using System;
using Unity.Netcode;

public struct WeaponID : INetworkSerializeByMemcpy, IEquatable<WeaponID>
{
    public int ID;

    public bool Equals(WeaponID other)
    {
        return ID == other.ID;
    }

    public override bool Equals(object obj)
    {
        return obj is WeaponID other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ID;
    }

    public static bool operator ==(WeaponID x, WeaponID y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(WeaponID x, WeaponID y)
    {
        return !(x == y);
    }

    public override string ToString()
    {
        return $"WeaponID({ID})";
    }
}
