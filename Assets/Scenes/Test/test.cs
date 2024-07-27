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
    [SerializeField] string SFXname;
    private void Start()
    {
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            AudioManager.Instance.PlaySFXAtPosition(SFXname, transform.position + new Vector3(1,1,1));
        }
    }
}

