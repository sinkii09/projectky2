using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Skills/Toss Skill")]
public class TossSkill : Skill
{
    bool m_Launched;
    public override bool OnStart(ServerCharacter serverCharacter)
    {

        serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.ClientCharacter.ClientPlaySkillRpc(Data);

        return true;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        if (TimeRunning >= Config.ExecTimeSeconds && !m_Launched)
        {
            Throw(serverCharacter);
        }

        return true;
    }
    public override void Reset()
    {
        base.Reset();
        m_Launched = false;
    }
    ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in Config.Projectiles)
        {
            if (projectileInfo.Prefab)
            {
                return projectileInfo;
            }
        }
        throw new System.Exception($"Action {this.name} has no usable Projectiles!");
    }
    void Throw(ServerCharacter serverCharacter)
    {
        if (!m_Launched)
        {
            m_Launched = true;

            var projectileInfo = GetProjectileInfo();

            var no = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
            no.GetComponent<TossProjectile>().Initialize(serverCharacter);
            var networkObjectTransform = no.transform;

            networkObjectTransform.forward = serverCharacter.physicsWrapper.Transform.forward;

            networkObjectTransform.position = serverCharacter.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObjectTransform.position) +
                                              networkObjectTransform.forward + Vector3.up;

            no.Spawn(true);

            var tossedItemRigidbody = no.GetComponent<Rigidbody>();

            tossedItemRigidbody.AddForce((networkObjectTransform.forward * 100f) + (networkObjectTransform.up * 100f), ForceMode.Impulse);
            tossedItemRigidbody.AddTorque((networkObjectTransform.forward * Random.Range(-15f, 15f)) + (networkObjectTransform.up * Random.Range(-15f, 15f)), ForceMode.Impulse);

            if (Config.CheckingAmount)
            {
                DecreaseAmount(serverCharacter);
            }
        }
    }
}
