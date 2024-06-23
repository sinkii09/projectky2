using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Skills/Melee Skill")]
public partial class MeleeSkill : Skill
{
    private bool m_ExecutionFired;
    private ulong m_ProvisionalTarget;

    static RaycastHit[] s_Hits = new RaycastHit[4];
    public override bool OnStart(ServerCharacter serverCharacter)
    {

        serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
        
        serverCharacter.ClientCharacter.ClientPlaySkillRpc(Data);
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        m_ExecutionFired = false;
        m_ProvisionalTarget = 0;
        //m_ImpactPlayed = false;
        //m_SpawnedGraphics = null;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        if (!m_ExecutionFired && (Time.time - TimeStarted) >= Config.ExecTimeSeconds)
        {
            m_ExecutionFired = true;
            var foe = GetIdealMeleeFoe(serverCharacter,serverCharacter.physicsWrapper.DamageCollider, Config.Range,Config.Anim);
            if (foe != null)
            {
                foe.ReceiveHP(-Config.Damage, serverCharacter);
            }
        }
        return true;
    }

    public static IDamageable GetIdealMeleeFoe(ServerCharacter serverCharacter,Collider ourCollider,float meleeRange,string anim)
    {
        
        var layermask = LayerMask.NameToLayer("PCs");
        var mybound = ourCollider.bounds;
        int numResults = Physics.BoxCastNonAlloc(ourCollider.transform.position, mybound.extents, ourCollider.transform.forward, s_Hits, Quaternion.identity, meleeRange,layermask);
        RaycastHit[] results = s_Hits;

        IDamageable foundFoe = null;

        for (int i = 0; i < numResults; i++)
        {
            var damageable = results[i].collider.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable())
            {
                foundFoe = damageable;
            }
        }
        return foundFoe;
    }
}
