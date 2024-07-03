using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class test : MonoBehaviour
{
    bool isStart;
    AbilityRequest data;

    private float t;
    private float totalDistance;
    private Vector3 startPoint;
    private Vector3 controlPoint;
    private Vector3 endPoint;

    [SerializeField] GameObject testObject;
    private void Start()
    {

    }
    private void Update()
    {
        if (isStart)
        {
            if (t < 1)
            {
                t += Time.deltaTime * 20 / totalDistance;
                testObject.transform.position = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
                Debug.Log(CalculateBezierPoint(t, startPoint, controlPoint, endPoint));
            }
        }
        if(Input.GetMouseButtonDown(0))
        {
            PerformAbility();
        }
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
    private void PerformAbility()
    {
        t = 0;
        startPoint = testObject.transform.position;
        endPoint = new Vector3(5, 0, 5);
        controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 10;
        totalDistance = CalculateTotalDistance();
        isStart = true;
        Debug.Log($"start point = {startPoint} -- endPoint = {endPoint} --- controlPoint = {controlPoint}");
    }
}

