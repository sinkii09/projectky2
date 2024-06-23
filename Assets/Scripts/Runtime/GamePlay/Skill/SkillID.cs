using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public struct SkillID : INetworkSerializeByMemcpy, IEquatable<SkillID>
{
    public int ID;

    public bool Equals(SkillID other)
    {
        return ID == other.ID;
    }

    public override bool Equals(object obj)
    {
        return obj is SkillID other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ID;
    }

    public static bool operator ==(SkillID x, SkillID y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(SkillID x, SkillID y)
    {
        return !(x == y);
    }

    public override string ToString()
    {
        return $"ActionID({ID})";
    }
}
