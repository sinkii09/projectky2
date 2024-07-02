using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class GameIndicator : MonoBehaviour
{
    Transform follow;

    Vector3 defaultScale;
    Vector3 offset = new Vector3(0, 0.1f, 0);
    //public enum IndicatorType
    //{
    //    Range,
    //    Radius,
    //}

    //public IndicatorType type;
    public void Initialize(Transform parent)
    {
        gameObject.SetActive(false);
        follow = parent;
        defaultScale = transform.localScale;
    }
    public void ShowIndicator(float radius)
    {
        gameObject.SetActive(true);
        transform.localScale = transform.localScale * radius * 2;
        //switch (type)
        //{
        //    case IndicatorType.Range:
        //        transform.localScale = transform.localScale * radius;
        //        break;
        //    case IndicatorType.Radius:
        //        transform.localScale = transform.localScale * radius * 2;
        //        break;
        //}
        
    }

    public void HideIndicator()
    {
        gameObject.SetActive(false);
        transform.localScale = defaultScale;
    }

    private void Update()
    {
        if(follow != null)
        {
            transform.position = follow.position + offset;
        }
    }
}
