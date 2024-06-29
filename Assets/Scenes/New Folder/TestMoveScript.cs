using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class TestMoveScript : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 controlPoint;
    public Vector3 endPoint;

    private float t;
    private float totalDistance;
    public float speed;

    [SerializeField] GameObject testObject;

    bool islaunch;
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Launch();
        }
        if(islaunch)
        {
            if (t < 1)
            {
                t += Time.deltaTime * speed / totalDistance;
                testObject.transform.position = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
            }
        }
    }
    void Launch()
    {
        startPoint = transform.position;
        endPoint = new Vector3(5,0,5);
        controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 10;
        totalDistance = CalculateTotalDistance();
        t = 0;
        islaunch = true;
    }
    private Vector3 CalculateBezierPoint(float t, Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 position = uu * startPoint; // u^2 * P0
        position += 2 * u * t * controlPoint; // 2 * u * t * P1
        position += tt * endPoint; // t^2 * P2
        return position;
    }
    private float CalculateTotalDistance(int segments = 20)
    {
        float distance = 0f;
        Vector3 previousPoint = startPoint;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 currentPoint = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
            distance += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return distance;
    }
}
