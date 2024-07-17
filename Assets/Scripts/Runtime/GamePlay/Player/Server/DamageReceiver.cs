using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageReceiver : NetworkBehaviour, IDamageable
{
    public event Action< int, ServerCharacter> DamageReceived;

    public event Action<Collision> CollisionEntered;

    [SerializeField]
    ServerCharacter serverCharacter;

    
    public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
    {
        return IDamageable.SpecialDamageFlags.None;
    }

    public bool IsDamageable()
    {
        return serverCharacter.LifeState.Value == LifeStateEnum.Alive || !serverCharacter.IsInvincilbe.Value ;
    }

    public void ReceiveHP(int HP, ServerCharacter inflicter = null )
    {
        if (IsDamageable())
        {
            DamageReceived?.Invoke(HP,inflicter);
        }
    }
    void OnCollisionEnter(Collision other)
    {
        CollisionEntered?.Invoke(other);
    }
}

public interface IDamageable
{
    void ReceiveHP(int HP, ServerCharacter inflicter = null);

    ulong NetworkObjectId { get; }

    Transform transform { get; }

    [Flags]
    public enum SpecialDamageFlags
    {
        None = 0,
        UnusedFlag = 1 << 0, // does nothing
        StunOnTrample = 1 << 1,
        NotDamagedByPlayers = 1 << 2,
    }
    SpecialDamageFlags GetSpecialDamageFlags();
    bool IsDamageable();
}

