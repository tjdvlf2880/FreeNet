using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using static InputSystemNaming;
public class WASD_MouseBinding 
{
    InputManager _inputManager;
    InputActionMap _inputActionMap;
    Vector3 _moveInput;
    Vector2 _mouseInput;

    public event Action<Vector3> _onMoveInputChanged;
    public event Action<Vector2> _onMouseInputChanged;

    public WASD_MouseBinding(InputManager inputManager , InputActionMap actionMap)
    {
        _inputManager = inputManager;
        _inputActionMap = actionMap;
        var action = _inputManager.AddAction(_inputActionMap,"WASD",InputActionType.PassThrough);
        var compositeSyntax = _inputManager.AddCompositeBinding(action, CompositeType.Vector2D);
        _inputManager.AddCompositeBinding(compositeSyntax, InputSystemNaming.Vector2DSyntax.Up,Device.Keyboard, Key.W);
        _inputManager.AddCompositeBinding(compositeSyntax, InputSystemNaming.Vector2DSyntax.Down, Device.Keyboard, Key.S);
        _inputManager.AddCompositeBinding(compositeSyntax, InputSystemNaming.Vector2DSyntax.Left, Device.Keyboard, Key.A);
        _inputManager.AddCompositeBinding(compositeSyntax, InputSystemNaming.Vector2DSyntax.Right, Device.Keyboard, Key.D);
        _inputManager.AddCallback(action, InputManager.CallbackType.Performed, OnWASD);

        action = _inputManager.AddAction(_inputActionMap, "MouseMove", InputActionType.Value);
        var syntax = _inputManager.AddMouseBinding(action, MouseType.delta);
        syntax.WithProcessor(InputSystemNaming.Processor.ScaleVector2.ToInputSystemName(new Vector2(0.5f,0.5f)));
        syntax.WithProcessor(InputSystemNaming.Processor.Invert.ToInputSystemName());
        _inputManager.AddCallback(action, InputManager.CallbackType.Performed, OnMouse);

    }
    private void OnWASD(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnMouse(InputAction.CallbackContext ctx)
    {
        _mouseInput = ctx.ReadValue<Vector2>();
        _onMouseInputChanged?.Invoke(_mouseInput);
    }
}
