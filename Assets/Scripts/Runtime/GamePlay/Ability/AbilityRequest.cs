using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AbilityRequest
{
    public ulong clientId {  get; private set; }
    public Ability Ability { get; set; }
    public Vector3 Position { get; private set; }
    public Vector3 Direction { get; private set; }
    public AbilityRequest(Ability ability, Vector3 position, Vector3 direction)
    {

        Ability = ability;
        Position = position;
        Direction = direction;
    }
}
