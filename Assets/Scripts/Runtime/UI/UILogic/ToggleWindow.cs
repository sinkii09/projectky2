using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ToggleWindow : MonoBehaviour
{
    public virtual void Active(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
