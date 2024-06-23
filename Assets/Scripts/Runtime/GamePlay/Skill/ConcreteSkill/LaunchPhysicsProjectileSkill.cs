using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Skills/Launch Projectile Skill")]
public class LaunchPhysicsProjectileSkill : Skill
{
    protected bool m_Launched = false;
    public override bool OnStart(ServerCharacter serverCharacter)
    {
        serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.ClientCharacter.ClientPlaySkillRpc(Data);
        return true;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        if (!m_Launched)
        {
            serverCharacter.physicsWrapper.Transform.forward = Vector3.Lerp(serverCharacter.physicsWrapper.Transform.forward, Data.Direction, Time.deltaTime * 10);
            LaunchProjectile(serverCharacter);
        }
        return true;
    }
    public override void Reset()
    {
        m_Launched = false;
        base.Reset();
    }
    protected virtual void LaunchProjectile(ServerCharacter serverCharacter)
    {
        if (!m_Launched)
        {

            m_Launched = true;
            var projectileInfo = GetProjectileInfo();
            NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
            networkObject.transform.forward = serverCharacter.physicsWrapper.transform.forward;
            networkObject.transform.position = serverCharacter.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObject.transform.position);
            networkObject.GetComponent<PhysicsProjectile>().Initialize(serverCharacter.NetworkObjectId, projectileInfo, networkObject.transform);
            networkObject.Spawn(true);

            if (Config.CheckingAmount)
            {
                DecreaseAmount(serverCharacter);
            }
        }
    }
    protected virtual ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in Config.Projectiles)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<PhysicsProjectile>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }
    public override void End(ServerCharacter serverCharacter)
    {
        LaunchProjectile(serverCharacter);
    }
    public override void Cancel(ServerCharacter serverCharacter)
    {
        if (!string.IsNullOrEmpty(Config.Anim2))
        {
            serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim2);
        }
    }

    public override bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        return true;
    }
}
