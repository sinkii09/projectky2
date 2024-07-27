using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "AOElaunch", menuName = "Abilities/AOELaunch")]
public class AOELaunch : Ability
{
    bool isStart;
    AbilityRequest data;

    public string launchSFX;
    public override void Activate(ServerCharacter serverCharacter, AbilityRequest data)
    {
        this.data = data;
        serverCharacter.physicsWrapper.Transform.forward = data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(abilityAnimationTrigger);

        PerformAOE(serverCharacter,data);

    }
    public override void OnAbilityUpdate(ServerCharacter serverCharacter)
    {
        if(isStart && TimeRunning >= durationTime)
        {
            isStart = false;
            OnHit(serverCharacter);
            serverCharacter.DequeueAbility();
        }

    }
    private void OnHit(ServerCharacter serverCharacter)
    {
        serverCharacter.ClientCharacter.ClientPlayAbilitySFXRpc(serverCharacter.physicsWrapper.Transform.position, special: IsSpecialAbility);
        var colliders = Physics.OverlapSphere(data.Position, Radius, LayerMask.GetMask("PCs"));
        for (var i = 0; i < colliders.Length; i++)
        {
            var other = colliders[i].GetComponent<ServerCharacter>();
            if (other == null || other.OwnerClientId == serverCharacter.OwnerClientId) { return; }
            var enemy = colliders[i].GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.ReceiveHP(-Damage, serverCharacter);
            }
        }
    }
    private void PerformAOE(ServerCharacter serverCharacter, AbilityRequest data)
    {  
        CoroutineRunner.Instance.StartCoroutine(ExecuteTimeDelay());
        isStart = true;
        serverCharacter.ClientCharacter.ClientPlayEffectRpc(data.Position, serverCharacter.physicsWrapper.Transform.rotation, special: IsSpecialAbility);
    }

    public override void OnPlayEffectClient(ClientCharacter clientCharacter,Vector3 position, Quaternion rotation, int num = 0)
    {
        var abilityFX = ParticlePool.Singleton.GetObject(effect[0], position, Quaternion.identity);
        abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effect[0]);
        bool hasVFX = abilityFX.TryGetComponent(out VisualEffect VFX);
        if(hasVFX)
        {
            VFX.Play();
        }
        AudioManager.Instance.PlaySFXAtPosition(launchSFX, position);
    }
}
