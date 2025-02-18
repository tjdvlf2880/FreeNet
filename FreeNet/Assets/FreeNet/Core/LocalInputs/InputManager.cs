using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystemNaming;
using static UnityEngine.InputSystem.InputActionSetupExtensions;

public class InputManager : MonoBehaviour
{
    PlayerInput _playerInput;
    InputActionAsset _inputActionAsset;
    public enum CallbackType
    {
        Started,
        Cancled,
        Performed
    }
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        _playerInput.neverAutoSwitchControlSchemes = true;
        _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
    }
    public void Assign()
    {
        _playerInput.actions = _inputActionAsset;
    }
    public void Enable(InputActionMap actionMap,bool enabled)
    {
        if(enabled)
        {
            actionMap.Enable();
        }
        else
        {
            actionMap.Disable();
        }
    }
    public void Switch(InputActionMap actionmap)
    {
        _playerInput.SwitchCurrentActionMap(actionmap.name);
    }
    public void Switch(string scheme, InputDevice[] devices)
    {
        _playerInput.SwitchCurrentControlScheme(scheme, devices);
    }

    public InputActionMap AddActionMap(string actionMap)
    {
        return _inputActionAsset.AddActionMap(actionMap);
    }
    public InputAction AddAction(InputActionMap actionMap, string action , InputActionType type)
    {
        return actionMap.AddAction(action, InputActionType.PassThrough);
    }
    public BindingSyntax AddKeyboardBinding(InputAction action, Key key)
    {
        string path = InputSystemNaming.Device.Keyboard.ToInputSystemName() + '/' + key;
        return action.AddBinding(path);
    }
    public BindingSyntax AddMouseBinding(InputAction action,InputSystemNaming.MouseType type)
    {
        string path = InputSystemNaming.Device.Mouse.ToInputSystemName() + '/' + type.ToInputSystemName();
        return action.AddBinding(path);
    }
    public CompositeSyntax AddCompositeBinding(InputAction action, InputSystemNaming.CompositeType type)
    {
        return action.AddCompositeBinding(type.ToInputSystemName());
    }
    public CompositeSyntax AddCompositeBinding(CompositeSyntax compositeSyntax, InputSystemNaming.Vector2DSyntax syntax, InputSystemNaming.Device device, Key key)
    {
        string path = device.ToInputSystemName() + '/' + key;
        return compositeSyntax.With(syntax.ToInputSystemName(), path);
    }
    public ControlSchemeSyntax AddControlScheme(string schemeName)
    {
        return _inputActionAsset.AddControlScheme(schemeName);
    }
    public ControlSchemeSyntax AddControlScheme(ControlSchemeSyntax syntax, InputSystemNaming.Device device)
    {
        return syntax.WithRequiredDevice(device.ToInputSystemName());
    }
    public void AddCallback(InputAction action, CallbackType type , Action<InputAction.CallbackContext> callback)
    {
        switch(type)
        {
            case CallbackType.Started:
                action.started += callback;
                break;
            case CallbackType.Performed:
                action.performed += callback;
                break;
            case CallbackType.Cancled:
                action.canceled += callback;
                break;
        };
    }
    public void Dispose()
    {
        UnityEngine.Object.Destroy(_inputActionAsset);
    }
    public void OnDestroy()
    {
        Dispose();
    }

}
