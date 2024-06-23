using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public sealed class PlayerAvatar : GuidScriptableObject, INetworkSerializeByMemcpy
{
    public GameObject Graphics;

    public GameObject GraphicsCharacterSelect;

    public Sprite Portrait;
}
