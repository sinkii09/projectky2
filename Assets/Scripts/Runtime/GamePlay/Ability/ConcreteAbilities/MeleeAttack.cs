﻿using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee", menuName = "Abilities/MeleeAttack")]
public class MeleeAttack : Ability
{
    static RaycastHit[] s_Hits = new RaycastHit[4];
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.LookAt(data.Direction);
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(serverCharacter.physicsWrapper.Transform.position, serverCharacter.physicsWrapper.Transform.rotation, 0,IsSpecialAbility);
        TriggerAttack(serverCharacter);
        
    }
    void TriggerAttack(ServerCharacter serverCharacter)
    {
        var foe = GetIdealMeleeFoe(serverCharacter, serverCharacter.physicsWrapper.DamageCollider, MaxRange);
        if (foe != null)
        {
            foe.ReceiveHP(-Damage, serverCharacter);
        }
        serverCharacter.DequeueAbility();
    }
    public static IDamageable GetIdealMeleeFoe(ServerCharacter serverCharacter, Collider ourCollider, float meleeRange)
    {

        var layermask = 1 << LayerMask.NameToLayer("PCs");
        var mybound = ourCollider.bounds;
        int numResults = Physics.BoxCastNonAlloc(ourCollider.transform.position, mybound.extents, ourCollider.transform.forward, s_Hits, Quaternion.identity, meleeRange, layermask);
        RaycastHit[] results = s_Hits;
        IDamageable foundFoe = null;

        for (int i = 0; i < numResults; i++)
        {
            if (results[i].collider.GetComponent<ServerCharacter>().OwnerClientId == serverCharacter.OwnerClientId) continue;
            var damageable = results[i].collider.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable())
            {
                serverCharacter.ClientCharacter.ClientPlayEffectRpc(results[i].point, serverCharacter.physicsWrapper.Transform.rotation, 1,false);
                foundFoe = damageable;
            }
        }
        return foundFoe;
    }
    public override void OnPlayEffectClient(ClientCharacter clientCharacter, Vector3 position, Quaternion rotation, int num = 0)
    {
        var abilityFX = ParticlePool.Singleton.GetObject(effect[num], position,rotation);
        abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect[num]);
        if(num == 0)
        {
            OnPlaySFXClient(position);
        }
    }
}
