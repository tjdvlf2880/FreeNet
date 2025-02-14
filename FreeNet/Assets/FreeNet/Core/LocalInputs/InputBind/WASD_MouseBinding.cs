using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class WASD_MouseBinding
{
    InputBinding _inputBinding;
    InputActionMap _inputActionMap;
    Vector3 _moveInput;
    Vector2 _mouseInput;

    bool _up;
    bool _down;
    bool _left;
    bool _right;

    public event Action<Vector3> _onMoveInputChanged;
    public event Action<Vector2> _onMouseInputChanged;

    public WASD_MouseBinding(InputBinding inputBinding, string name)
    {
        _inputBinding = inputBinding;
        if(!inputBinding.FindActionMap(name,out _inputActionMap))
        {
            _inputActionMap = new InputActionMap(name);
            inputBinding.AddActionMap(_inputActionMap);
        }
        BindInput();
    }
    void BindInput()
    {
        _inputActionMap.AddAction("Up", InputActionType.Button).AddBinding("<Keyboard>/w");
        _inputActionMap.AddAction("Down", InputActionType.Button).AddBinding("<Keyboard>/s");
        _inputActionMap.AddAction("Left", InputActionType.Button).AddBinding("<Keyboard>/a");
        _inputActionMap.AddAction("Right", InputActionType.Button).AddBinding("<Keyboard>/d");
        var mouseBinding = _inputActionMap.AddAction("MouseMove", InputActionType.Value).AddBinding("<Mouse>/delta");
        mouseBinding.WithProcessor("ScaleVector2(x=0.5,y=0.5)");
        mouseBinding.WithProcessor("invert");
        BindEvent();
    }
    private void BindEvent()
    {
        _inputActionMap["Up"].performed += OnUpPressed;
        _inputActionMap["Up"].canceled += OnUpReleased;
        _inputActionMap["Left"].performed += OnLeftPressed;
        _inputActionMap["Left"].canceled += OnLeftReleased;
        _inputActionMap["Down"].performed += OnDownPressed;
        _inputActionMap["Down"].canceled += OnDownReleased;
        _inputActionMap["Right"].performed += OnRightPressed;
        _inputActionMap["Right"].canceled += OnRightReleased;
        _inputActionMap["MouseMove"].performed += OnMouseMovePressed;
    }
    private void OnRightReleased(InputAction.CallbackContext ctx)
    {
        _right = false;
        _moveInput.x = _left ? -1 : 0;
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnRightPressed(InputAction.CallbackContext ctx)
    {
        _right = true;
        _moveInput.x = ctx.ReadValue<float>();
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnDownReleased(InputAction.CallbackContext ctx)
    {
        _down = false;
        _moveInput.z = _up ? 1 : 0;
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnDownPressed(InputAction.CallbackContext ctx)
    {
        _down = true;
        _moveInput.z = -ctx.ReadValue<float>();
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnLeftPressed(InputAction.CallbackContext ctx)
    {
        _left = true;
        _moveInput.x = -ctx.ReadValue<float>();
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnLeftReleased(InputAction.CallbackContext ctx)
    {
        _left = false;
        _moveInput.x = _right ? 1 : 0;
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnUpReleased(InputAction.CallbackContext ctx)
    {
        _up = false;
        _moveInput.z = _down ? -1 : 0;
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnUpPressed(InputAction.CallbackContext ctx)
    {
        _up = true;
        _moveInput.z = ctx.ReadValue<float>();
        _onMoveInputChanged?.Invoke(_moveInput);
    }
    private void OnMouseMovePressed(InputAction.CallbackContext ctx)
    {
        _mouseInput = ctx.ReadValue<Vector2>();
        _onMouseInputChanged?.Invoke(_mouseInput);
    }
}
