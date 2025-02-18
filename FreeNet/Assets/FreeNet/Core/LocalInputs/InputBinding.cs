using UnityEngine;
using UnityEngine.InputSystem;

public class InputBinding : MonoBehaviour
{
    InputManager _inputManager;
    WASD_MouseBinding _WASD_MouseBinding;


    void Start()
    {
        _inputManager = GetComponent<InputManager>();

        var keyboardMouseScheme = _inputManager.AddControlScheme("Default");
        _inputManager.AddControlScheme(keyboardMouseScheme, InputSystemNaming.Device.Keyboard);
        _inputManager.AddControlScheme(keyboardMouseScheme, InputSystemNaming.Device.Mouse);
        var actionMap = _inputManager.AddActionMap("Player");
        _WASD_MouseBinding = new WASD_MouseBinding(_inputManager, actionMap);
        _inputManager.Enable(actionMap,true);
        _inputManager.Assign();
    }
}
