using UnityEngine;

public class InputBindExample : MonoBehaviour
{
    [SerializeField]
    string _bindingMapName;
    InputBinding _inputBinding;
    WASD_MouseBinding _wasdMouseBinding;
    void Start()
    {
        _inputBinding = GetComponent<InputBinding>();
        BindInput();
    }
    void BindInput()
    {
        _wasdMouseBinding = new WASD_MouseBinding(_inputBinding, _bindingMapName);
        _inputBinding.EnableMap(_bindingMapName);
        _wasdMouseBinding._onMoveInputChanged += OnMoveInputChange;
        _wasdMouseBinding._onMouseInputChanged += OnMouseInputChanged;
    }
    void OnMoveInputChange(Vector3 val)
    {

    }
    void OnFixedMouseInputChanged()
    {
       
    }
    void OnMouseInputChanged(Vector2 val)
    {
        
    }
}
