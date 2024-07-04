using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dash", menuName = "Abilities/Dash")]
public class Dash : Ability
{
    private bool isDashing = false;
    private float dashTime;
    Rigidbody rb;
    AbilityRequest data;
    private Collider[] hitColliders = new Collider[10];
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(serverCharacter.physicsWrapper.transform.position);
        rb = serverCharacter.GetComponent<Rigidbody>();
        this.data = data;
    }

    public override bool CanActivate(ServerCharacter serverCharacter)
    {
        return true;
    }

    public override void OnAbilityUpdate(ServerCharacter serverCharacter)
    {
        if (!isDashing && TimeRunning >= executeTime)
        {
            StartDash(serverCharacter);
            Debug.Log("start dash");
        }
        if(isDashing)
        {
            if(Time.time < dashTime)
            {
                Vector3 newPosition = rb.position + data.Direction * (Radius / durationTime) * Time.deltaTime;
                rb.MovePosition(newPosition);
            }
            else
            {
                isDashing = false;
                serverCharacter.DequeueAbility();
                serverCharacter.Movement.CanMoving(true);
            }
            CheckCollision(serverCharacter);
        }
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
                    serverCharacter.ClientCharacter.ClientPlayEffectRpc(collisionPoint,1);
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
        dashTime = Time.time + durationTime;
        serverCharacter.Movement.Dash();
        isDashing = true;
    }
    public override void OnPlayClient(ClientCharacter clientCharacter, Vector3 position, int num = 0)
    {
        var abilityFX = ParticlePool.Singleton.GetObject(effect[num], position, Quaternion.identity);
        abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect[num]);
        if (num == 0)
        {
            abilityFX.transform.SetParent(clientCharacter.transform);
        }
    }
}
