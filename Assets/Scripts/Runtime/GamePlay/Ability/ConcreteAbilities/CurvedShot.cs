using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "CurvedShot", menuName = "Abilities/CurvedShot")]

public class CurvedShot : Ability
{
    public float maxHeight = 10;

    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);

        LaunchProjectile(serverCharacter,data);
    }

    public override bool CanActivate(ServerCharacter serverCharacter)
    {
        return true;
    }
    protected virtual ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in projectileInfoList)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<CurvedProjectile>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }
    protected virtual void LaunchProjectile(ServerCharacter serverCharacter, AbilityRequest data)
    {
        CoroutineRunner.Instance.StartCoroutine(ExecuteTimeDelay());
        var projectileInfo = GetProjectileInfo();
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
        networkObject.transform.forward = serverCharacter.physicsWrapper.transform.forward;
        networkObject.transform.position = serverCharacter.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObject.transform.position);

        Vector3 startPosition = networkObject.transform.position;
        Vector3 targetPosition = data.Position;
        Vector3 controlPoint = (startPosition + targetPosition) / 2 + Vector3.up * maxHeight;
        networkObject.GetComponent<CurvedProjectile>().Initialize(startPosition, controlPoint, targetPosition,projectileInfo,serverCharacter);
        networkObject.Spawn(true);
        if (CheckAmount)
        {
            DecreaseAmount(serverCharacter);
        }
        serverCharacter.DequeueAbility();
    }
}
