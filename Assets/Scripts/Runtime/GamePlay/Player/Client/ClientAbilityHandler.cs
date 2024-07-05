using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientAbilityHandler
{
    ClientCharacter clientCharacter;
    public ClientAbilityHandler(ClientCharacter clientCharacter)
    {
        this.clientCharacter = clientCharacter;
    }
    public void PlayAbility(Ability ability,Vector3 position,Quaternion rotation,int num = 0)
    {
        ability.OnPlayClient(clientCharacter, position,rotation,num);
    }
}
