using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenLobbyUIKeyBinding : KeyBinding<float>
{
    public OpenLobbyUIKeyBinding(InputBinding inputBinding, string name,string key)
        :base(inputBinding, name)
    {
        BindInput("OpenLobbyUI", InputActionType.Button, "<Keyboard>/"+key);
    }
}