using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<bool> FireEvent;

    public Vector2 MousePosition { get; private set; }

    private PlayerInputActions _controller;
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }
    public bool IsLeftMouseButtonDownThisFrame()
    {
        return _controller.Player.LeftMouseClick.WasPressedThisFrame();
    }
    private void OnEnable()
    {
        if (_controller == null)
        {
            _controller = new PlayerInputActions();
            _controller.Player.SetCallbacks(this);
        }

        _controller.Player.Enable();
    }

    private void OnDisable()
    {
        _controller.Player.Disable();
    }

    public void OnLeftMouseClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            FireEvent?.Invoke(true);
        }
    }
}
