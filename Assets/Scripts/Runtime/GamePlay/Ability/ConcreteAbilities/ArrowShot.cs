using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "ArrowShot", menuName = "Abilities/ArrowShot")]
public class ArrowShot : Ability
{
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);

        LaunchProjectile(serverCharacter);
    }
    public override bool CanActivate(ServerCharacter serverCharacter)
    {
        return true;
    }

    protected virtual void LaunchProjectile(ServerCharacter serverCharacter)
    {
        CoroutineRunner.Instance.StartCoroutine(ExecuteTimeDelay());
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
        Debug.Log("launch");
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
}
