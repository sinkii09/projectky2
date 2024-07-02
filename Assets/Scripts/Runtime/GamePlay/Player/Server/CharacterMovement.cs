using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum MovementState
{
    Idle = 0,
    Moving = 1,
    Charging = 2,
    Knockback = 3,
    Jump = 4,
}
[Serializable]
public enum MovementStatus
{
    Idle,
    Normal,
    Uncontrolled,
    Slowed,
    Hasted,
    Jump,
}
public class CharacterMovement : NetworkBehaviour
{
    [SerializeField]
    Rigidbody m_Rigidbody;
    [SerializeField]
    ServerCharacter m_ServerCharacter;

    private MovementState m_MovementState;

    MovementStatus m_PreviousState;

    private Vector3 moveDirection;
    private Vector3 knockBackDirection;
    private Vector3 jumpDirection;
    private float m_SpecialModeDurationRemaining;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpSpeed = 10f;
    private float m_ForcedSpeed;
    private void Awake()
    {
        enabled = false;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            enabled = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            enabled = false;
        }
    }
    public void CancelMove()
    {
        m_MovementState = MovementState.Idle;
    }
    private void FixedUpdate()
    {

        PerformMovement();

        var currentState = GetMovementStatus(m_MovementState);
        if (m_PreviousState != currentState)
        {
            m_ServerCharacter.MovementStatus.Value = currentState;
            m_PreviousState = currentState;
        }
    }
    void PerformMovement()
    {

        float desiredMovementAmount;
        Vector3 movementVector;
        if (m_ServerCharacter.isGameOver.Value)
        {
            CancelMove();
            return;
        }
        if (m_MovementState == MovementState.Idle)
        {
            return;
        }

        if (m_MovementState == MovementState.Jump)
        {
            m_SpecialModeDurationRemaining -= Time.fixedDeltaTime;
            if (m_SpecialModeDurationRemaining <= 0)
            {
                m_MovementState = MovementState.Idle;
                return;
            }
            desiredMovementAmount = jumpSpeed * Time.fixedDeltaTime;
            movementVector = jumpDirection;
        }
        else if (m_MovementState == MovementState.Knockback)
        {
            m_SpecialModeDurationRemaining -= Time.fixedDeltaTime;
            if (m_SpecialModeDurationRemaining <= 0)
            {
                m_MovementState = MovementState.Idle;
                return;
            }

            desiredMovementAmount = m_ForcedSpeed * Time.fixedDeltaTime;
            movementVector = knockBackDirection * desiredMovementAmount;
        }
        else
        {
            desiredMovementAmount = moveSpeed * Time.fixedDeltaTime;
            movementVector = moveDirection;
            if (moveDirection == Vector3.zero)
            {
                m_MovementState = MovementState.Idle;
                return;
            }
        }
        transform.parent.position += movementVector * desiredMovementAmount;
        transform.parent.rotation = Quaternion.LookRotation(moveDirection);
        m_Rigidbody.position = transform.position;
        m_Rigidbody.rotation = transform.rotation;
    }
    public bool IsMoving()
    {
        return m_MovementState != MovementState.Idle;
    }
    public bool IsPerformingForcedMovement()
    {
        return m_MovementState == MovementState.Knockback || m_MovementState == MovementState.Charging;
    }
    public void SetMoveDirection(Vector3 moveDirection)
    {
        m_MovementState = MovementState.Moving;
        this.moveDirection = new Vector3(moveDirection.x, 0, moveDirection.y).normalized;
    }
    internal void SetJump()
    {
        m_MovementState = MovementState.Jump;
        jumpDirection = (transform.forward + transform.up);
        m_SpecialModeDurationRemaining = 1.5f;
    }
    public void StartKnockback(Vector3 knocker, float speed, float duration)
    {
        m_MovementState = MovementState.Knockback;
        knockBackDirection = transform.position - knocker;
        m_ForcedSpeed = speed;
        m_SpecialModeDurationRemaining = duration;
    }
    private MovementStatus GetMovementStatus(MovementState movementState)
    {
        switch (movementState)
        {
            case MovementState.Idle:
                return MovementStatus.Idle;
            case MovementState.Knockback:
                return MovementStatus.Uncontrolled;
            case MovementState.Jump:
                return MovementStatus.Jump;
            default:
                return MovementStatus.Normal;
        }
    }
}
