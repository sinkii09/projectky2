using System.Collections;
using UnityEngine;

public class ToggleWindow : MonoBehaviour
{
    public virtual void Active(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
