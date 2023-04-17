using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    private enum MovementStates
    {
        Idle,
        Walking,
        InAir,
        Swinging,
        OnWall
    }

    [Header("Debugging")]
    [SerializeField] private MovementStates currentMovementState;

    [Header("Movement Parameters")]    
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float maxSpeed = 5f;

    [Header("Referenced Objects")]
    [SerializeField] private Camera playerCamera;

    //Components
    private Rigidbody _rigidbody;

    //input fields
    private ThirdPersonActionAsset playerActionsAsset;
    private InputAction move;

    //Movement Vectors
    private Vector3 forceDirection = Vector3.zero;
    private Vector3 WallNormal;
    private Vector3 WallHorizontal;
    private Vector3 WallVertical;

    //Temporary fields
    private bool wallLock = false;
    private bool cornerDetected = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        //Reference Rigidbody Component
        _rigidbody= GetComponent<Rigidbody>();
        
        //Instantiate Action Asset
        playerActionsAsset = new ThirdPersonActionAsset();
        currentMovementState = MovementStates.Idle;
    }

    private void OnEnable()
    {
        //Subsribe DoJump() method to the Jump Action in the Action Asset
        playerActionsAsset.Player.Jump.started += DoJump;
        
        //Reference the Move Action
        move = playerActionsAsset.Player.Move;

        //Enable the Action Asset
        playerActionsAsset.Player.Enable();
    }

    private void OnDisable()
    {
        playerActionsAsset.Player.Jump.started -= DoJump;
        playerActionsAsset.Player.Disable();
    }

    private void FixedUpdate()
    {
        if (currentMovementState == MovementStates.OnWall)
            WallMovement();
        else
            GroundMovement();        
    }

    private void GroundMovement()
    {
        //Set Direction based on User Input
        forceDirection += move.ReadValue<Vector2>().x * movementForce * GetCameraRight(playerCamera);
        forceDirection += move.ReadValue<Vector2>().y * movementForce * GetCameraForward(playerCamera);

        if (IsGrounded())
        {
            currentMovementState = forceDirection.magnitude > 0 ? MovementStates.Walking : MovementStates.Idle;
        }
        else
        {
            //Stronger force, when in air
            forceDirection *= 3;
            currentMovementState = MovementStates.InAir;
        }

        //Apply Force to body 
        _rigidbody.AddForce(forceDirection, ForceMode.Impulse);

        //Reset Force
        forceDirection = Vector3.zero;

        //Increase effect of gravity over time
        if (_rigidbody.velocity.y < 0f) 
            _rigidbody.velocity -= 8 * Physics.gravity.y * Time.fixedDeltaTime * Vector3.down;

        //Make sure, that the maxSpeed is not exceeded
        Vector3 horizontalVelocity = _rigidbody.velocity;
        horizontalVelocity.y = 0;

        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            _rigidbody.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * _rigidbody.velocity.y;

        //Set players rotation to movement direction
        AdjustPlayerRotationGround();
    }

    private void WallMovement()
    {
        if (cornerDetected)
            return;

        if (StickToWall() || wallLock)
        {
            CheckForCorner();

            currentMovementState = MovementStates.OnWall;
            //Set Direction based on User
            //  Left-Right
            forceDirection += move.ReadValue<Vector2>().x * movementForce / 2 * WallHorizontal;
            //  Up-Down
            forceDirection += move.ReadValue<Vector2>().y * movementForce / 2 * WallVertical;

            //Apply Force to body 
            //_rigidbody.AddForce(forceDirection - WallNormal * 2, ForceMode.Impulse);
            _rigidbody.AddForce(forceDirection, ForceMode.Impulse);

            //Reset Force
            forceDirection = Vector3.zero;

            //Adjust the players rotation based on direction from user input
            AdjustPlayerRotationWall();
        }
        else
        {
                currentMovementState = MovementStates.InAir;
                InitGroundMovement();
        }
    }

    private void AdjustPlayerRotationGround()
    {
        Vector3 direction = _rigidbody.velocity;
        
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            _rigidbody.angularVelocity = Vector3.zero;
    }

    private void AdjustPlayerRotationWall()
    {
        Vector3 direction = _rigidbody.velocity;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            _rigidbody.transform.rotation = Quaternion.LookRotation(-WallNormal, direction);
        else
            _rigidbody.angularVelocity = Vector3.zero;            
    }

    private Vector3 GetCameraForward(Camera p_playerCamera)
    {
        Vector3 forward = p_playerCamera.transform.forward;

        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera p_playerCamera)
    {
        Vector3 right = p_playerCamera.transform.right;

        right.y = 0;
        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
        else if (currentMovementState == MovementStates.OnWall)
        {
            InitGroundMovement();
        }

        currentMovementState = MovementStates.InAir;
    }

    private bool IsGrounded()
    {
        Ray ray = new(this.transform.position , Vector3.down);

        return Physics.Raycast(ray, out _, 1);
    }

    private bool StickToWall()
    {        
        Ray rayForward = new(_rigidbody.transform.position, _rigidbody.transform.forward);
        
        Ray rayForwardFoot = new((_rigidbody.transform.position - _rigidbody.transform.up), _rigidbody.transform.forward);

        if(currentMovementState == MovementStates.Walking || currentMovementState == MovementStates.InAir)
            return (Physics.Raycast(rayForward, out _, 0.6f) && Physics.Raycast(rayForwardFoot, out _, 0.6f));
        else if(currentMovementState == MovementStates.OnWall)
            return (Physics.Raycast(rayForward, out _, 0.6f) || Physics.Raycast(rayForwardFoot, out _, 0.6f));
        else return false;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (StickToWall())
        {
            InitWallMovement();

            WallNormal = collision.GetContact(0).normal.normalized;
            WallHorizontal = Vector3.Cross(WallNormal, Vector3.up);
            WallVertical = Vector3.Cross(WallNormal, -_rigidbody.transform.right);

            
            currentMovementState = MovementStates.OnWall;
        }             
    }

    private void InitWallMovement()
    {
        _rigidbody.useGravity = false;
        //_rigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
    }

    private void InitGroundMovement()
    {
        _rigidbody.useGravity = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    private void CheckForCorner()
    {
        //Ray rayForward = new(_rigidbody.transform.position + _rigidbody.transform.up * 0.5f + _rigidbody.transform.forward * 0.3f , _rigidbody.transform.forward);
        //Ray rayDownward = new(_rigidbody.transform.position + _rigidbody.transform.up * 0.7f + _rigidbody.transform.forward * 0.3f, _rigidbody.transform.forward - _rigidbody.transform.up);

        Ray rayForward = new(_rigidbody.transform.position + _rigidbody.transform.forward * 0.3f, _rigidbody.transform.forward);
        Ray rayDownward = new(_rigidbody.transform.position + _rigidbody.transform.up * 0.3f + _rigidbody.transform.forward * 0.3f, _rigidbody.transform.forward - _rigidbody.transform.up);


        if (!Physics.Raycast(rayForward, out _, 0.6f) && Physics.Raycast(rayDownward, out RaycastHit wallHit, 1.0f)){

            Debug.DrawRay(_rigidbody.transform.position + _rigidbody.transform.forward * 0.3f, _rigidbody.transform.forward * 10, Color.yellow, 1);
            Debug.DrawRay(_rigidbody.transform.position + _rigidbody.transform.up * 0.3f + _rigidbody.transform.forward * 0.3f, (_rigidbody.transform.forward - _rigidbody.transform.up) * 10, Color.red, 1);

            cornerDetected = true;
            wallLock = true;

            if(wallHit.normal != Vector3.up)
            {
                Debug.Log("Corner detected");
                Debug.Log(wallHit.collider.name + " | " + wallHit.normal);

                StartCoroutine(GoAroundCorner(_rigidbody.rotation, wallHit.normal));
            }

            //wallLock = false;
        }
    }

    IEnumerator GoAroundCorner(Quaternion pRotation, Vector3 newWall)
    {
        Quaternion initRotation = pRotation;
        _rigidbody.velocity = Vector3.zero;

        while (Quaternion.Angle(initRotation, _rigidbody.rotation) < 90)
        {
            _rigidbody.AddForce(_rigidbody.transform.up * 0.03f, ForceMode.Impulse);

            Quaternion startRotation = _rigidbody.transform.rotation;
            _rigidbody.rotation = startRotation * Quaternion.AngleAxis(5, Vector3.right);

            yield return null;
        }

        /*
        if(_rigidbody.rotation.eulerAngles.z < 0 && _rigidbody.rotation.eulerAngles.z != -90)
        {
            Quaternion startRotation = _rigidbody.transform.rotation;
            float newRotation = _rigidbody.rotation.eulerAngles.z - 90;
            Debug.Log("Minus: " + newRotation);
            _rigidbody.rotation = startRotation * Quaternion.AngleAxis(newRotation, Vector3.up);
        } 
        else if(_rigidbody.rotation.eulerAngles.z > 0 && _rigidbody.rotation.eulerAngles.z != 90)
        {
            Debug.Log(_rigidbody.rotation.eulerAngles.z);
            Quaternion startRotation = _rigidbody.transform.rotation;
            float newRotation = _rigidbody.rotation.eulerAngles.z + 90;
            Debug.Log("Plus: " + newRotation);
            _rigidbody.rotation = startRotation * Quaternion.AngleAxis(newRotation, Vector3.up);
        }*/

        WallNormal = newWall.normalized;
        WallHorizontal = Vector3.Cross(newWall, Vector3.up);
        WallVertical = Vector3.Cross(newWall, -_rigidbody.transform.up);

        cornerDetected = false;
    }
}
