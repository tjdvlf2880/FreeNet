using UnityEngine;
using UnityEngine.InputSystem;

public class InputBinding : MonoBehaviour
{
    PlayerInput _playerInput;
    InputActionAsset _inputAsset;
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        _playerInput.actions = _inputAsset;
        AddKeybouadMouseScheme();
        _playerInput.SwitchCurrentControlScheme("Keyboard&Mouse");
        _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
    }

    void AddKeybouadMouseScheme()
    {
        var keyboardMouseScheme = new InputControlScheme("Keyboard&Mouse")
        .WithRequiredDevice("<Keyboard>")
        .WithRequiredDevice("<Mouse>");
        _inputAsset.AddControlScheme(keyboardMouseScheme);
    }

    public void EnableMap(string name)
    {
        if (FindActionMap(name, out var map))
        {
            map.Enable();
        }
    }
    public void AddActionMap(InputActionMap actionMap)
    {
        _playerInput.actions.AddActionMap(actionMap);
    }
    public void RemoveActionMap(InputActionMap actionMap)
    {
        _playerInput.actions.RemoveActionMap(actionMap);
    }
    public bool FindActionMap(string name,out InputActionMap map)
    {
        map = _playerInput.actions.FindActionMap(name);
        return map != null;
    }

    private void OnDestroy()
    {
        Destroy(_inputAsset);
    }
}