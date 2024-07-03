using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee", menuName = "Abilities/MeleeAttack")]
public class MeleeAttack : Ability
{
    static RaycastHit[] s_Hits = new RaycastHit[4];
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);

        TriggerAttack(serverCharacter);
        
    }

    public override bool CanActivate(ServerCharacter serverCharacter)
    {
        return true;
    }
    void TriggerAttack(ServerCharacter serverCharacter)
    {
        CoroutineRunner.Instance.StartCoroutine(ExecuteTimeDelay());
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
        Debug.Log(s_Hits.Length);
        IDamageable foundFoe = null;

        for (int i = 0; i < numResults; i++)
        {
            if (results[i].collider.GetComponent<ServerCharacter>().OwnerClientId == serverCharacter.OwnerClientId) continue;
            var damageable = results[i].collider.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable())
            {
                foundFoe = damageable;
            }
        }
        return foundFoe;
    }
}
