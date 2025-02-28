using System;
using UnityEngine.InputSystem;

public class ToggleInputBinding
{
    string _actionMap;
    InputManager _inputManager;
    InputActionMap _inputActionMap;
    float _toggleInput;
    public event Action<bool> _onToggleInputChanged;

    public ToggleInputBinding(InputManager inputManager, InputActionMap actionMap, Key toggleKey)
    {
        _inputManager = inputManager;
        _inputActionMap = actionMap;
        var action = _inputManager.AddAction(_inputActionMap, "Toggle", InputActionType.Button);
        _inputManager.AddKeyboardBinding(action, toggleKey);
        _inputManager.AddCallback(action, InputManager.CallbackType.Performed, OnToggle);
    }
    private void OnToggle(InputAction.CallbackContext ctx)
    {
        _toggleInput = ctx.ReadValue<float>();
        _onToggleInputChanged?.Invoke(_toggleInput == 1);
    }
}
