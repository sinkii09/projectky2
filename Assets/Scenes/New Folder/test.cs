using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class test : MonoBehaviour
{
    [SerializeField] KillNotifyHolder killNotifyHolder;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            killNotifyHolder.UpdateCardList("abdsf", "adfaewr");
        }
    }
}

