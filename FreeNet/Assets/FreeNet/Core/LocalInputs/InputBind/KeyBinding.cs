using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyBinding<T> where T : struct
{
    InputBinding _inputBinding;
    InputActionMap _inputActionMap;
    T _value;
    public event Action<T> _onKeyInputChanged;
    public KeyBinding(InputBinding inputBinding, string name)
    {
        _inputBinding = inputBinding;
        if (!inputBinding.FindActionMap(name, out _inputActionMap))
        {
            _inputActionMap = new InputActionMap(name);
            inputBinding.AddActionMap(_inputActionMap);
        }
    }

    public void BindInput(string actionName, InputActionType type , string binding)
    {
        _inputActionMap.AddAction(actionName, type).AddBinding(binding);
        BindEvent(actionName);
    }
    void BindEvent(string actionName)
    {
        _inputActionMap[actionName].performed += OnActionPressed;
        _inputActionMap[actionName].canceled += OnAcionReleased;
    }

    void OnActionPressed(InputAction.CallbackContext ctx) 
    {
        _value = ctx.ReadValue<T>();
        _onKeyInputChanged?.Invoke(_value);
    }

    void OnAcionReleased(InputAction.CallbackContext ctx)
    {
        _value = ctx.ReadValue<T>();
        _onKeyInputChanged?.Invoke(_value);
    }
}
