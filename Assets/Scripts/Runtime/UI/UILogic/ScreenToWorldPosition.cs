using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenToWorldPosition : MonoBehaviour
{
    [SerializeField] RectTransform screenTransform;

    [SerializeField] bool SetAtStart;
    private void Start()
    {
        if (SetAtStart)
        {
            
            SetWorldPosition(screenTransform);
        }
    }
    public void SetWorldPosition(RectTransform screenRect)
    {
        float distanceFromCamera = 10.0f;
        Vector3 screenPosition = new Vector3(screenRect.position.x, screenRect.position.y, distanceFromCamera);
        transform.position = Camera.main.ScreenToWorldPoint(screenPosition);
    }
}
