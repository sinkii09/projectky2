using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveScript : MonoBehaviour
{
    [SerializeField]
    Rigidbody m_Rigidbody;
    [SerializeField] InputReader m_InputReader;

    private Vector3 moveDirection;
    [SerializeField] private float moveSpeed = 10f;
    private void Start()
    {
        m_InputReader.MoveEvent += InputReader_MoveEvent;
    }
    private void OnDestroy()
    {
        m_InputReader.MoveEvent -= InputReader_MoveEvent;
    }
    private void FixedUpdate()
    {
        float desiredMovementAmount;
        Vector3 movementVector;
        desiredMovementAmount = moveSpeed * Time.fixedDeltaTime;
        movementVector = moveDirection;
        transform.position += movementVector * desiredMovementAmount;
        transform.rotation = Quaternion.LookRotation(moveDirection);
        m_Rigidbody.position = transform.position;
        m_Rigidbody.rotation = transform.rotation;
    }
    private void InputReader_MoveEvent(Vector2 moveDir)
    {
        moveDirection = new Vector3(moveDir.x, 0,moveDir.y).normalized;
    }
}
