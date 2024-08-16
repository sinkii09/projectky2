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

    [SerializeField] Vector3 direction;
    private void Start()
    {
        transform.rotation = Quaternion.LookRotation(direction);
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            transform.rotation = Quaternion.LookRotation(Vector3.zero);

        }
    }
}

