using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "BoomerangShot", menuName = "Abilities/BoomerangShot")]
public class BoomerangShot : Ability
{
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.LookAt(data.Direction);
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(serverCharacter.physicsWrapper.Transform.position, Quaternion.identity, special: IsSpecialAbility);
        serverCharacter.ClientCharacter.ClientPlayAbilitySFXRpc(serverCharacter.physicsWrapper.Transform.position, special: IsSpecialAbility);
        LaunchProjectile(serverCharacter,data);
    }
    protected virtual void LaunchProjectile(ServerCharacter serverCharacter, AbilityRequest data)
    {
        CoroutineRunner.Instance.StartCoroutine(ExecuteTimeDelay());
        var projectileInfo = GetProjectileInfo();
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
        networkObject.transform.forward = serverCharacter.physicsWrapper.transform.forward;
        networkObject.transform.position = serverCharacter.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObject.transform.position);
        networkObject.GetComponent<BoomerangProjectile>().Initialize(networkObject.transform.position,data.Position,projectileInfo,serverCharacter);
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
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<BoomerangProjectile>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }
    public override void OnPlayEffectClient(ClientCharacter clientCharacter, Vector3 position, Quaternion rotation, int num = 0)
    {
        var abilityFX = ParticlePool.Singleton.GetObject(effect[0], position, Quaternion.identity);
        abilityFX.transform.position = clientCharacter.transform.localToWorldMatrix.MultiplyPoint(new Vector3(.25f, 1, 1));
        abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect[0]);
        bool hasVFX = abilityFX.TryGetComponent(out VisualEffect VFX);
        if (hasVFX)
        {
            VFX.Play();
        }
    }
}
