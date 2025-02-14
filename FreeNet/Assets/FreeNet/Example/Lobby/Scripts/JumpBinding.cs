using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpBinding
{
    InputBinding _inputBinding;
    InputActionMap _inputActionMap;
    float _jump;
    public event Action<float> _onJumpInputChanged;
    public JumpBinding(InputBinding inputBinding, string name)
    {
        _inputBinding = inputBinding;
        if (!inputBinding.FindActionMap(name, out _inputActionMap))
        {
            _inputActionMap = new InputActionMap(name);
            inputBinding.AddActionMap(_inputActionMap);
        }
        BindInput();
    }

    void BindInput()
    {
        _inputActionMap.AddAction("Jump", InputActionType.Button).AddBinding("<Keyboard>/space");
        BindEvent();
    }
    void BindEvent()
    {
        _inputActionMap["Jump"].performed += OnJumpPressed;
        _inputActionMap["Jump"].canceled += OnJumpReleased;
    }

    void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        _jump = ctx.ReadValue<float>();
        _onJumpInputChanged?.Invoke(_jump);
    }

    void OnJumpReleased(InputAction.CallbackContext ctx)
    {
        _jump = ctx.ReadValue<float>();
        _onJumpInputChanged?.Invoke(_jump);
    }

}