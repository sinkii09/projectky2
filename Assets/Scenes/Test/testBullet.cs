using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class testBullet : MonoBehaviour
{
    public bool isStart;
    Rigidbody rb;
    [SerializeField] float t;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isStart = !isStart;
            rb.velocity = transform.forward *t;
        }
    }
    private void FixedUpdate()
    {
        //if (isStart)
        //{
        //    rb.MovePosition(transform.position + Vector3.forward * Time.fixedDeltaTime * t);
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Environment"))
        {
            Debug.Log("coll");
            isStart = false;
        }
    }
}
