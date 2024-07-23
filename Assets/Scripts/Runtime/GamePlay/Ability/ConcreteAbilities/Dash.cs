using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

[CreateAssetMenu(fileName = "Dash", menuName = "Abilities/Dash")]
public class Dash : Ability
{
    private Vector3 startPos;
    private float dashTime;
    private float distance;
    private float speed = 60;
    private bool isDashing = false;
    
    Rigidbody rb;
    AbilityRequest data;
    private Collider[] hitColliders = new Collider[10];

    ServerCharacter character;
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(serverCharacter.physicsWrapper.transform.position, serverCharacter.physicsWrapper.Transform.rotation,special:IsSpecialAbility);
        rb = serverCharacter.GetComponent<Rigidbody>();
        this.data = data;
        character = serverCharacter;
    }
    public override void OnAbilityUpdate(ServerCharacter serverCharacter)
    {
        if (!isDashing && TimeRunning >= executeTime)
        {
            StartDash(serverCharacter);
        }
        if(isDashing)
        {
            CheckCollision(serverCharacter);
        }
    }

    public override void OnReset()
    {
        isDashing=false;
        character.SetInvincible(false);
    }

    void CheckCollision(ServerCharacter serverCharacter)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(serverCharacter.physicsWrapper.Transform.position, data.Ability.Radius, hitColliders, LayerMask.GetMask("PCs"));
        for (int i = 0; i < hitCount; i++)
        {
            var other = hitColliders[i].GetComponent<ServerCharacter>();
            if (other != null && other.OwnerClientId == serverCharacter.OwnerClientId) continue;
            var damageable = hitColliders[i].GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable())
            {
                damageable.ReceiveHP(-Damage, serverCharacter);
                Vector3 collisionPoint;
                if (TryGetCollisionPoint(serverCharacter.physicsWrapper.Transform.position, hitColliders[i], out collisionPoint))
                {
                    serverCharacter.ClientCharacter.ClientPlayEffectRpc(collisionPoint, serverCharacter.physicsWrapper.Transform.rotation, 1,IsSpecialAbility);
                }
                
            }
        }
    }
    bool TryGetCollisionPoint(Vector3 sphereCenter, Collider collider, out Vector3 collisionPoint)
    {
        collisionPoint = Vector3.zero;
        RaycastHit hit;

        Vector3 direction = (collider.transform.position - sphereCenter).normalized;
        if (Physics.Raycast(sphereCenter, direction, out hit, Radius, LayerMask.GetMask("PCs")))
        {
            collisionPoint = hit.point;
            return true;
        }

        return false;
    }
    void StartDash(ServerCharacter serverCharacter)
    {
        startPos = serverCharacter.physicsWrapper.Transform.position;
        distance = Vector3.Distance(startPos, data.Position);
        RaycastHit hit;
        if (Physics.Raycast(startPos, data.Direction, out hit, distance, LayerMask.GetMask("Environment")))
        {
            distance = hit.distance - 1;
        }
        dashTime = distance / speed;
        serverCharacter.Movement.StartDash(data.Direction,speed,dashTime);
        serverCharacter.SetInvincible(true);
        isDashing = true;
    }
    public override void OnPlayEffectClient(ClientCharacter clientCharacter, Vector3 position,Quaternion rotation, int num = 0)
    {
        var abilityFX = ParticlePool.Singleton.GetObject(effect[num], position, rotation);
        abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect[num]);
        if (num == 0)
        {
            abilityFX.transform.SetParent(clientCharacter.transform);
            OnPlaySFXClient(position);
        }
    }
}
