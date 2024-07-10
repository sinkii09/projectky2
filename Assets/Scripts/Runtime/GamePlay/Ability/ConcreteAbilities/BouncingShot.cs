using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "BouncingShot", menuName = "Abilities/BouncingShot")]
public class BouncingShot : ArrowShot
{
    protected override ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in projectileInfoList)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<BouncingBullet>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }

    protected override void LaunchProjectile(ServerCharacter serverCharacter)
    {
        var projectileInfo = GetProjectileInfo();
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
        networkObject.transform.forward = serverCharacter.physicsWrapper.Transform.forward;
        networkObject.transform.position = serverCharacter.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObject.transform.position);
        networkObject.GetComponent<BouncingBullet>().Initialize(projectileInfo, serverCharacter);
        networkObject.Spawn(true);

        if (CheckAmount)
        {
            DecreaseAmount(serverCharacter);
        }
        serverCharacter.DequeueAbility();
    }
}
