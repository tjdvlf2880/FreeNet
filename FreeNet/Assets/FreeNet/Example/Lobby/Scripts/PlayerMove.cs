using UnityEngine;
public class PlayerMove : MonoBehaviour
{
    CharacterController _characterController;
    [SerializeField]
    float _speed;
    [SerializeField]
    float _mouseMultiplyer;
    [SerializeField]
    Vector2 _pitchRange;
    [SerializeField]
    string _bindingMapName;

    float _pitch;
    float _yaw;

    WASD_MouseBinding _defaultBinding;


    Vector3 _moveflag;
    Vector2 _mouseflag;
    protected void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
    protected void Start()
    {
        BindInput();
    }
    private void Update()
    {

    }
    private void FixedUpdate()
    {
        _pitch = _pitch - _mouseflag.y;
        _pitch = Mathf.Clamp(_pitch, _pitchRange.x, _pitchRange.y);
        _pitch = _pitch % 360;
        _yaw = _yaw + _mouseflag.x;
        _yaw = _yaw % 360;

        Quaternion pitchRotation = Quaternion.AngleAxis(_pitch, Vector3.right);
        Quaternion yawRotation = Quaternion.AngleAxis(_yaw, Vector3.up);

        Vector3 vector = yawRotation * pitchRotation * _moveflag * _speed * Time.fixedDeltaTime;
        _characterController.Move(vector);
        transform.rotation = yawRotation * pitchRotation;
    }

    void OnMouseInputChanged(Vector2 val)
    {
        _mouseflag = val* _mouseMultiplyer;
    }
    void OnMoveInputChange(Vector3 val)
    {
        _moveflag = val;
    }
    void BindInput()
    {
        _defaultBinding._onMoveInputChanged += OnMoveInputChange;
        _defaultBinding._onMouseInputChanged += OnMouseInputChanged;
    }
    public void OnDestroy()
    {
        _defaultBinding._onMoveInputChanged -= OnMoveInputChange;
        _defaultBinding._onMouseInputChanged -= OnMouseInputChanged;
    }

}
