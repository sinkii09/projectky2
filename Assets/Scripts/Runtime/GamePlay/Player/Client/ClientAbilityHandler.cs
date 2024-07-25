using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ClientAbilityHandler
{
    ClientCharacter clientCharacter;
    public ClientAbilityHandler(ClientCharacter clientCharacter)
    {
        this.clientCharacter = clientCharacter;
    }
    public void PlayAbility(Ability ability,Vector3 position,Quaternion rotation,int num = 0)
    {
        ability.OnPlayEffectClient(clientCharacter, position,rotation,num);
    }
    public void PlaySFXAtPosition(Ability ability, Vector3 position)
    {
        ability.OnPlaySFXClient(position);
    }
    internal void ShowAbilityIndicator(Ability ability,Vector3 position,float radius)
    {
        ability.OnShowIndicatorClient(position,radius);
    }
}
