using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerStateMachine : MonoBehaviour
{
#region Variablen
    [Header("InputSystem")]
        
        private ThirdPersonActionAsset _input;
        private Vector2 _currentMovementInput;
        private bool _isMovementPressed;
        private bool _isJumpPressed;
        private bool _isRunPressed;
        private bool _isCrouchPressed;

    [Header("Movement")]
        [SerializeField] [Range(0.0f, 10.0f)] float _walkspeed;
        [SerializeField] [Range(0.0f, 10.0f)] float _runspeed;
        [SerializeField] [Range(0.0f, 10.0f)] float _crouchspeed;
        [SerializeField] float _accelerationtime;
        [SerializeField] AnimationCurve _accelerationcurve;
        private float _accelerationCounter = 0;
        [SerializeField] [Range(0.0f, 10.0f)] float _jumpForce;
        [SerializeField] [Range(0.0f, 10.0f)] float _longjumpForce;
        [SerializeField] float _airMultiplier;
        [SerializeField] float _jumpCooldown;
        [SerializeField] float _airDrag;
        [SerializeField] float _groundDrag;
        [SerializeField] float _slideDrag;
        [SerializeField] float _rotationSpeed;
        [SerializeField] public float _crouchingHeightFactor;

    [Header("Ground Check")]
        [SerializeField] LayerMask _whatIsGrounded;
        private bool _grounded;

    [Header("State Variables")]
        PlayerBaseState _currentState;
        PlayerStateFactory _states;
    
    [Header("Other")]
        public  Rigidbody _rb;
        private Vector3 _velocity;
        private Vector3 _moveDirection;
        [SerializeField] private Transform _orientation;
        [SerializeField] private Transform _playerObj;
        [SerializeField] private Animator _playerSprite;
        [SerializeField] public CapsuleCollider _collider;
        private float _angle;

#endregion


#region getter & setter
    public PlayerBaseState Currentstate { get {return _currentState;} set {_currentState = value;}}
    public Vector2 CurrentMovementInput { get {return _currentMovementInput;}}
    public bool IsMovementPressed   { get {return _isMovementPressed;}}
    public bool IsJumpPressed       { get {return _isJumpPressed;}}
    public bool IsRunPressed        { get {return _isRunPressed;}}
    public bool IsCrouchPressed     { get {return _isCrouchPressed;}}
    public float WalkSpeed      { get {return _walkspeed;}}
    public float RunSpeed       { get {return _runspeed;}}
    public float CrouchSpeed    { get {return _crouchspeed;}}
    public float AccelerationTime           { get {return _accelerationtime;}}
    public AnimationCurve AccelerationCurve { get {return _accelerationcurve;}}
    public float AccelerationCounter        { get {return _accelerationCounter;} set {_accelerationCounter = value;}}
    public float JumpForce      { get {return _jumpForce;}}
    public float LongJumpForce  { get {return _longjumpForce;}}
    public float AirMultiplier  { get {return _airMultiplier;}}
    public float JumpCooldown   { get {return _jumpCooldown;}}
    public float AirDrag    { get {return _airDrag;}}
    public float GroundDrag { get {return _groundDrag;}}
    public float SlideDrag  { get {return _slideDrag;}}
    public bool Grounded    { get {return _grounded;}}
    public Vector3 MoveDirection { get {return _moveDirection;} set {_moveDirection = value;}}
    public Vector3 Velocity      { get {return _velocity;}}
    public Transform Orientation { get {return _orientation;}}
    public Animator PlayerSprite { get {return _playerSprite;}}
    public float Angle           { get {return _angle;}}
#endregion


#region Input-Methoden

    /*
    private void OnMovementInput(InputAction.CallbackContext context) {
        _currentMovementInput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }
    private void OnJump(InputAction.CallbackContext context) {
        _isJumpPressed = context.ReadValueAsButton();
    }
    */


    /*
    private void OnRun(InputAction.CallbackContext context) {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnCrouch(InputAction.CallbackContext context) {
        _isCrouchPressed = context.ReadValueAsButton();
    }
    */
#endregion

#region Unity-Methoden
    private void Awake() {
        _input = new ThirdPersonActionAsset();

        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();
    }

    private void OnEnable() {
        //Initiate InputActions

        /*
        _input.Player.Move.performed += OnMovementInput;
        _input.Player.Move.canceled += OnMovementInput;

        _input.Player.Jump.started += OnJump;
        _input.Player.Jump.canceled += OnJump;
        */

        /* WIP
        _input.Player.Run.started += OnRun;
        _input.Player.Run.canceled += OnRun;
        _input.Player.Crouch.started += OnCrouch;
        _input.Player.Crouch.canceled += OnCrouch;
        */

        //_input.Player.Enable();
    }

    private void OnDisable()
    {
        //_input.Player.Disable();
    }

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    void Update() {
        _grounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.1f, _whatIsGrounded);
        _moveDirection = _orientation.forward * _currentMovementInput.y + _orientation.right * _currentMovementInput.x;
        _angle = (Camera.main.transform.rotation.eulerAngles.y - _playerObj.rotation.eulerAngles.y + 360) % 360;
        
        try
        {
            _playerSprite.transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        }
        catch (System.Exception)
        {
            
        }
        
        if(_isMovementPressed) {
            _playerObj.forward = Vector3.Slerp(_playerObj.forward,_moveDirection.normalized, Time.deltaTime * _rotationSpeed);
        }

        _velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        _currentState.UpdateState();

        Debug.Log(_currentState);
    }

    private void FixedUpdate() {
        _currentState.FixedUpdateState();
    }



    #endregion
}
