using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class test : MonoBehaviour
{
    [SerializeField] bool isStart;
    AbilityRequest data;

    [SerializeField]private float t;
    private float totalDistance;
    [SerializeField] private Vector3 startPoint;
    private Vector3 controlPoint;
    [SerializeField] private Vector3 endPoint;
    Collider[] m_CollisionCache = new Collider[3];
    [SerializeField] GameObject testObject;
    Vector3 num = new Vector3();
    float dashTime;
    Rigidbody rb;
    Vector3 direction;
    private void Start()
    {
        rb = testObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //if (isStart)
        //{
        //    if (t < 1)
        //    {
        //        t += Time.deltaTime * 20 / totalDistance;
        //        testObject.transform.position = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
        //        Debug.Log(CalculateBezierPoint(t, startPoint, controlPoint, endPoint));
        //    }
        //}

    }
    void Run()
    {
        if (dashTime>0)
        {
            var newPos = testObject.transform.position + direction * t * Time.fixedDeltaTime;
            //testObject.transform.position = newPos;
            rb.MovePosition(newPos);
            dashTime -= Time.fixedDeltaTime;
        }
        else
        {
            isStart=false;
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(testObject.transform.position, endPoint);
    }
    void Detect()
    {
        var numCollisions = Physics.OverlapSphereNonAlloc(testObject.transform.position, 2, m_CollisionCache, LayerMask.GetMask("Environment"));
        if(numCollisions > 0 )
        {
            testObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Debug.Log("stop");
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

