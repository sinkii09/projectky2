using UnityEngine;

[CreateAssetMenu(fileName = "SlamDunk", menuName = "Abilities/SlamDunk")]
public class SlamDunk : Ability
{
    bool isStart;
    AbilityRequest data;

    private float t;
    private float totalDistance;
    private Vector3 startPoint;
    private Vector3 controlPoint;
    private Vector3 endPoint;
    public LayerMask targetLayer;
    private Collider[] hitColliders = new Collider[10];
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        this.data = data;
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        PerformAbility(serverCharacter, data);
    }

    public override bool CanActivate(ServerCharacter serverCharacter)
    {
        return true;
    }

    public override void OnAbilityUpdate(ServerCharacter serverCharacter)
    {
        if(isStart)
        {
            if (t < 1)
            {
                t += Time.deltaTime * 20 / totalDistance;
                serverCharacter.transform.position = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
            }
            else
            {
                OnHit(serverCharacter);
                serverCharacter.DequeueAbility();
            }
        }
    }
    private Vector3 CalculateBezierPoint(float t, Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 position = uu * startPoint; // u^2 * P0
        position += 2 * u * t * controlPoint; // 2 * u * t * P1
        position += tt * endPoint; // t^2 * P2
        return position;
    }
    private float CalculateTotalDistance(int segments = 20)
    {
        float distance = 0f;
        Vector3 previousPoint = startPoint;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 currentPoint = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
            distance += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return distance;
    }
    private void OnHit(ServerCharacter serverCharacter)
    {
        serverCharacter.Movement.CancelMove();
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(data.Position, serverCharacter.physicsWrapper.Transform.rotation,special:IsSpecialAbility);
        isStart = false;
        int hitCount = Physics.OverlapSphereNonAlloc(serverCharacter.physicsWrapper.Transform.position, data.Ability.Radius, hitColliders, targetLayer);
        for (int i = 0; i < hitCount; i++)
        {
            var other = hitColliders[i].GetComponent<ServerCharacter>();
            if (other != null && other.OwnerClientId == serverCharacter.OwnerClientId) continue;
            var damageable = hitColliders[i].GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable())
            {
                damageable.ReceiveHP(-Damage, serverCharacter);
            }
        }
    }

    private void PerformAbility(ServerCharacter serverCharacter, AbilityRequest data)
    {
        t = 0;
        targetLayer = 1 << LayerMask.NameToLayer("PCs");
        startPoint = serverCharacter.physicsWrapper.Transform.position;
        endPoint = data.Position;
        controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 10;
        totalDistance = CalculateTotalDistance();
        
        isStart = true;
        serverCharacter.Movement.Jump();
    }
    public override void OnPlayClient(ClientCharacter clientCharacter, Vector3 position, Quaternion rotation, int num = 0)
    {
        foreach (var effect in effect)
        {
            var abilityFX = ParticlePool.Singleton.GetObject(effect, position, Quaternion.identity);
            abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect);
        }
    }
}
