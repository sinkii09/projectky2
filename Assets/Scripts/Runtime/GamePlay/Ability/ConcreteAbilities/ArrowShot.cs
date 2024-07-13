﻿using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "ArrowShot", menuName = "Abilities/ArrowShot")]
public class ArrowShot : Ability
{
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(serverCharacter.physicsWrapper.Transform.position, Quaternion.identity, special: IsSpecialAbility);
        LaunchProjectile(serverCharacter);
    }
    public override bool CanActivate(ServerCharacter serverCharacter)
    {
        return true;
    }

    protected virtual void LaunchProjectile(ServerCharacter serverCharacter)
    {
        var projectileInfo = GetProjectileInfo();
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
        networkObject.transform.forward = serverCharacter.physicsWrapper.transform.forward;
        networkObject.transform.position = serverCharacter.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObject.transform.position);
        networkObject.GetComponent<PhysicsProjectile>().Initialize(serverCharacter.OwnerClientId, projectileInfo, networkObject.transform);
        networkObject.Spawn(true);

        if (CheckAmount)
        {
            DecreaseAmount(serverCharacter);
        }
        serverCharacter.DequeueAbility();
    }
    protected virtual ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in projectileInfoList)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<PhysicsProjectile>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }
    public override void OnPlayClient(ClientCharacter clientCharacter, Vector3 position, Quaternion rotation, int num = 0)
    {
        Debug.Log("trigger effect");
        var abilityFX = ParticlePool.Singleton.GetObject(effect[0], position, Quaternion.identity);
        abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect[0]);
        bool hasVFX = abilityFX.TryGetComponent(out VisualEffect VFX);
        if (hasVFX)
        {
            VFX.Play();
        }
    }
}
