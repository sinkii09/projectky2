using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "CircleShot", menuName = "Abilities/CircleShot")]
public class CircleShot : Ability
{
    bool isStart;
    AbilityRequest data;
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        this.data = data;
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(serverCharacter.physicsWrapper.Transform.position, serverCharacter.physicsWrapper.Transform.rotation, special: true);
        serverCharacter.ClientCharacter.ClientPlayAbilitySFXRpc(serverCharacter.physicsWrapper.Transform.position, special: IsSpecialAbility);
        isStart = true;
    }
    public override void OnAbilityUpdate(ServerCharacter serverCharacter)
    {
        if(isStart && TimeRunning >= executeTime)
        {
            LaunchAbility(serverCharacter, data);
            serverCharacter.DequeueAbility();
        }
    }

    private void LaunchAbility(ServerCharacter serverCharacter, AbilityRequest data)
    {
        isStart = false;
        var projectileInfo = GetProjectileInfo();
        for (int i = 0; i < Damage; i++)
        {
            float angle = i * Mathf.PI * 2f / Damage;
            Vector3 spawnPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Radius + serverCharacter.physicsWrapper.Transform.position;
            Quaternion rotation = Quaternion.LookRotation(spawnPos - serverCharacter.physicsWrapper.Transform.position);
            NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, spawnPos + Vector3.up, rotation);
            networkObject.GetComponent<PhysicsProjectile>().Initialize(serverCharacter.OwnerClientId, projectileInfo, networkObject.transform);
            networkObject.Spawn(true);
        }
        if (CheckAmount)
        {
            DecreaseAmount(serverCharacter);
        }
    }
    protected virtual ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in projectileInfoList)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<PhysicsProjectile>())
                return projectileInfo;
        }
        throw new Exception($"Action {name} has no usable Projectiles!");
    }
    public override void OnPlayEffectClient(ClientCharacter clientCharacter, Vector3 position, Quaternion rotation, int num = 0)
    {
        foreach(var effect in effect)
        {
            var abilityFX = ParticlePool.Singleton.GetObject(effect, position, Quaternion.identity);
            abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect);
        }
    }
}
