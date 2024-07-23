using UnityEngine;
using UnityEngine.VFX;

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
    LayerMask targetLayer;
    LayerMask obstacleLayer;
    private Collider[] hitColliders = new Collider[10];

    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        this.data = data;
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);
        

        PerformAbility(serverCharacter, data.Position);
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
    private Vector3 CalculateHighestBezierPoint(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
    {
        float highestT = 0.5f;
        return CalculateBezierPoint(highestT, startPoint, controlPoint, endPoint);
    }
    private void OnHit(ServerCharacter serverCharacter)
    {
        serverCharacter.Movement.CancelMove();
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(endPoint, serverCharacter.physicsWrapper.Transform.rotation,special:IsSpecialAbility);
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

    private void PerformAbility(ServerCharacter serverCharacter, Vector3 position)
    {
        t = 0;
        targetLayer = 1 << LayerMask.NameToLayer("PCs");
        obstacleLayer = 1<< LayerMask.NameToLayer("Environment");
        startPoint = serverCharacter.physicsWrapper.Transform.position;
        endPoint = position;
        
        Vector3 highestPoint = CalculateHighestBezierPoint(startPoint, controlPoint, endPoint);
        Vector3 checkPoint = new Vector3(startPoint.x,highestPoint.y+ 3,startPoint.z);
        RaycastHit hit;
        if (Physics.Raycast(checkPoint, (endPoint - startPoint).normalized, out hit, Vector3.Distance(startPoint, endPoint), obstacleLayer))
        {
            Vector3 XZhitPoint = new Vector3(hit.point.x,endPoint.y,hit.point.z);
            endPoint = XZhitPoint - (XZhitPoint - startPoint).normalized * 2f;
        }
        controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 10;
        totalDistance = CalculateTotalDistance();
        
        isStart = true;
        serverCharacter.Movement.Jump();
        serverCharacter.ClientCharacter.ShowAbilityIndicatorRpc(serverCharacter.OwnerClientId, endPoint, Radius);
    }
    public override void OnPlayEffectClient(ClientCharacter clientCharacter, Vector3 position, Quaternion rotation, int num = 0)
    {
        foreach (var effect in effect)
        {
            var abilityFX = ParticlePool.Singleton.GetObject(effect, position, Quaternion.identity);
            abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect);
        }
        OnPlaySFXClient(position);
    }
}
