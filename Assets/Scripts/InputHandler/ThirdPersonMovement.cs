using Cinemachine.Utility;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class manages the user input for the movement of the player.
/// </summary>
public class ThirdPersonMovement : MonoBehaviour
{
    /// <summary>
    /// Definiton of all possible movement-states
    /// </summary>
    private enum MovementStates
    {
        Idle,
        Walking,
        InAir,
        Swinging,
        OnWall
    }

    [Header("Debugging")]
    /// <summary>
    /// Shows the current movement state in the inspector panel.
    /// </summary>
    [SerializeField] private MovementStates currentMovementState;

    [Header("Movement Parameters")]
    /// <summary>
    /// The movement force defines how fast the player is moving.
    /// </summary>
    [SerializeField] private float movementForce = 1f;
    /// <summary>
    /// The jump force defines how high the player is able to jump. 
    /// </summary>
    [SerializeField] private float jumpForce = 2f;
    /// <summary>
    /// Max Speed caps the movement speed, so the player does not accelerate to infinity.
    /// </summary>
    [SerializeField] private float maxSpeed = 5f;

    [Header("Referenced Objects")]
    /// <summary>
    /// Reference to the player camera.
    /// </summary>
    [SerializeField] private Camera playerCamera;

    private Rigidbody _rigidbody;
    private ThirdPersonActionAsset playerActionsAsset;
    private InputAction move;
    private Vector3 forceDirection = Vector3.zero;
    private Vector3 WallNormal;
    private Vector3 WallHorizontal;
    private Vector3 WallVertical;


    // TEMPORARY TEST VARIABLE
    // Prevents the player from leaving Wall Movement, unless the jump action is performed.
    private bool wallLock = false;

    // TEMPORARY TEST VARIABLE
    // Disables all movement input, until the player has gone around a corner.
    private bool cornerDetected = false;

    private Vector3 WallHitPosition;

    [SerializeField] private Transform rotationAnchor;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Assigning reference to rigidbody component.
    /// Instantiating Action Asset.
    /// </summary>
    private void Awake()
    {
        //Reference Rigidbody Component
        _rigidbody = GetComponent<Rigidbody>();
        
        //Instantiate Action Asset
        playerActionsAsset = new ThirdPersonActionAsset();
        currentMovementState = MovementStates.Idle;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        //Subsribe DoJump() method to the Jump Action in the Action Asset
        playerActionsAsset.Player.Jump.started += DoJump;
        
        //Reference the Move Action
        move = playerActionsAsset.Player.Move;

        //Enable the Action Asset
        playerActionsAsset.Player.Enable();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled.
    /// </summary>
    private void OnDisable()
    {
        //Unsubsribe DoJump() method to the Jump Action in the Action Asset
        playerActionsAsset.Player.Jump.started -= DoJump;

        //Disable the Action Asset
        playerActionsAsset.Player.Disable();
    }

    /// <summary>
    /// Frame-Rate independent update method.
    /// </summary>
    private void FixedUpdate()
    {
        //Debug.DrawRay(_rigidbody.transform.position + _rigidbody.transform.up * 0.5f, _rigidbody.transform.forward * 10, Color.yellow, 1);                      //Forward
        //Debug.DrawRay(_rigidbody.transform.position + _rigidbody.transform.up * 1.25f, (_rigidbody.transform.forward - _rigidbody.transform.up) * 1, Color.red, 1);    //Downward

        //Debug.DrawRay(WallHitPosition, WallNormal * 10, Color.green, 5);

        //Switch between Wall- and Ground-Movement, based on current MovementState.

        if (currentMovementState == MovementStates.OnWall)
            WallMovement();
        else
            GroundMovement();        
    }

    /// <summary>
    /// This method defines the player movement, when it is moving along the ground.
    /// </summary>
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

    /// <summary>
    /// This method defines the player movement, when it is moving along a wall.
    /// </summary>
    private void WallMovement()
    {
        //TEST: Disable movement while moving around a corner.
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
            //_rigidbody.AddForce(- WallNormal * 2, ForceMode.Impulse);
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

    /// <summary>
    /// This method sets the players rotation to the movement direction, while moving along the ground.
    /// </summary>
    private void AdjustPlayerRotationGround()
    {
        Vector3 direction = _rigidbody.velocity;
        
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            _rigidbody.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// This method sets the players rotation to the movement direction, while moving along a wall.
    /// </summary>
    private void AdjustPlayerRotationWall()
    {
        Vector3 direction = _rigidbody.velocity;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            _rigidbody.transform.rotation = Quaternion.LookRotation(-WallNormal, direction);
        else
            _rigidbody.angularVelocity = Vector3.zero;            
    }

    /// <summary>
    /// Helper method for Grond Movement.
    /// Gets the forward Vector of the camera, so the player will move in the direction where the camera is pointing to.
    /// </summary>
    /// <param name="p_playerCamera"></param>
    /// <returns></returns>
    private Vector3 GetCameraForward(Camera p_playerCamera)
    {
        Vector3 forward = p_playerCamera.transform.forward;

        forward.y = 0;
        return forward.normalized;
    }

    /// <summary>
    /// Helper method for Grond Movement.
    /// Gets the Right Vector of the camera, so the player will move in the direction where the camera is pointing to.
    /// </summary>
    /// <param name="p_playerCamera"></param>
    /// <returns></returns>
    private Vector3 GetCameraRight(Camera p_playerCamera)
    {
        Vector3 right = p_playerCamera.transform.right;

        right.y = 0;
        return right.normalized;
    }

    /// <summary>
    /// The method being executed, when the Jump action is called.
    /// THis method will be subscribed to the appropiate InputAction in the OnEnable()-method.
    /// </summary>
    /// <param name="obj"></param>
    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
            forceDirection += Vector3.up * jumpForce;
        else if (currentMovementState == MovementStates.OnWall)
            InitGroundMovement();

        currentMovementState = MovementStates.InAir;
    }

    /// <summary>
    /// Checks, if the player is touching the ground.
    /// </summary>
    /// <returns></returns>
    private bool IsGrounded()
    {
        Ray ray = new(this.transform.position , Vector3.down);

        return Physics.Raycast(ray, out _, 1);
    }

    /// <summary>
    /// This method check, if the player should stick to a wall.
    /// </summary>
    /// <returns>True, if yes. False, if not.</returns>
    private bool StickToWall()
    {        
        Ray rayForward = new(_rigidbody.transform.position, _rigidbody.transform.forward);
        Ray rayForwardFoot = new((_rigidbody.transform.position - _rigidbody.transform.up), _rigidbody.transform.forward);
        Ray rayForwardHead = new((_rigidbody.transform.position + _rigidbody.transform.up), _rigidbody.transform.forward);

        Debug.DrawRay(_rigidbody.transform.position, _rigidbody.transform.forward * 10, Color.yellow, 5);
        Debug.DrawRay((_rigidbody.transform.position - _rigidbody.transform.up), _rigidbody.transform.forward * 10, Color.yellow, 5);
        Debug.DrawRay((_rigidbody.transform.position + _rigidbody.transform.up), _rigidbody.transform.forward * 10, Color.yellow, 5);

        //If not already on a wall, both rays have to return true. (In case the player only hits a curb for example)
        //If on a wall, only one ray needs to return true. (In case if player peeks around the corner.)
        if (currentMovementState == MovementStates.Walking || currentMovementState == MovementStates.InAir)
            return (Physics.Raycast(rayForward, out _, 0.6f) && Physics.Raycast(rayForwardFoot, out _, 0.6f));
        else if (currentMovementState == MovementStates.OnWall)
            return (Physics.Raycast(rayForward, out _, 0.6f) || Physics.Raycast(rayForwardFoot, out _, 0.6f) || Physics.Raycast(rayForwardHead, out _, 0.6f));
        else
        {
            Debug.Log("STICK TO WALL HAT NICHT GEKLAPPT");
            return false;
        }
    }

    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (StickToWall())
        {
            InitWallMovement();

            WallNormal = collision.GetContact(0).normal.normalized;
            WallHorizontal = Vector3.Cross(WallNormal, Vector3.up);
            WallVertical = Vector3.Cross(WallNormal, WallHorizontal).Abs().normalized;

            WallHitPosition = collision.GetContact(0).thisCollider.transform.position;
            Debug.DrawRay(WallHitPosition, WallNormal * 10, Color.green, 20);
            Debug.DrawRay(WallHitPosition, WallHorizontal * 10, Color.magenta, 20);
            Debug.DrawRay(WallHitPosition, WallVertical * 10, Color.white, 20);

            /*
            WallNormal = collision.GetContact(0).normal.normalized;
            WallHorizontal = Vector3.Cross(WallNormal, Vector3.up);
            WallHorizontal = Vector3.Cross(WallNormal, WallHorizontal).normalized;
            Debug.Log("NORMAL: " + WallNormal + " | HORIZONTAL: " + WallHorizontal + " | VERTICAL: " + WallVertical);
            */
            currentMovementState = MovementStates.OnWall;
        }             
    }

    /// <summary>
    /// Sets gravity and constraints needed for wall movement.
    /// </summary>
    private void InitWallMovement()
    {
        _rigidbody.useGravity = false;
        //_rigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Sets gravity and constraints needed for ground movement.
    /// </summary>
    private void InitGroundMovement()
    {
        _rigidbody.useGravity = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    /// <summary>
    /// WORK IN PROGRESS
    /// Sends out two rays, to check if player wants to go around a corner in wall movement.
    /// Initiates a Coroutine to go around a corner, if this is the case
    /// </summary>
    private void CheckForCorner()
    {

        Ray rayForward = new(_rigidbody.transform.position + _rigidbody.transform.up * 0.5f, _rigidbody.transform.forward);
        Ray rayDownward = new(_rigidbody.transform.position + _rigidbody.transform.up * 1.25f, _rigidbody.transform.forward - _rigidbody.transform.up);


        if (!Physics.Raycast(rayForward, out _, 0.6f) && Physics.Raycast(rayDownward, out RaycastHit wallHit, 1.5f)){

            if(wallHit.normal != Vector3.up && !wallLock)
            {
                WallHitPosition = wallHit.collider.transform.position;  
                
                /*
                Debug.Log("Corner detected");
                Debug.Log(wallHit.collider.name + " | " + wallHit.normal);
                Debug.DrawRay(_rigidbody.transform.position + _rigidbody.transform.up * 0.5f, _rigidbody.transform.forward * 10, Color.yellow, 5);                      //Forward
                Debug.DrawRay(_rigidbody.transform.position + _rigidbody.transform.up * 1.25f, (_rigidbody.transform.forward - _rigidbody.transform.up) * 10, Color.red, 5);    //Downward
                */

                _rigidbody.velocity = Vector3.zero;

                WallNormal = wallHit.normal.normalized;                        
                WallHorizontal = Vector3.Cross(wallHit.normal, Vector3.up);                        
                WallVertical = Vector3.Cross(wallHit.normal, WallHorizontal).Abs().normalized;
                
                /*
                Debug.DrawRay(WallHitPosition, WallNormal * 10, Color.green, 20);
                Debug.DrawRay(WallHitPosition, WallHorizontal * 10, Color.magenta, 20);
                Debug.DrawRay(WallHitPosition, WallVertical * 10, Color.white, 20);
                Debug.Log("NORMAL: " + WallNormal + " | HORIZOINTAL: " + WallHorizontal + " | VERTICAL: " + WallVertical);
                */
                StartCoroutine(CornerCoroutine());
            }
        }
    }

    IEnumerator CornerCoroutine()
    {
        Debug.Log("Corner Coroutine started!");
        cornerDetected= true;
        wallLock = true;

        int rotationAngle = 10;
        Vector3 moveDirection = _rigidbody.transform.right;

        if(_rigidbody.transform.right.y < 0)
        {
            rotationAngle = -10;
            moveDirection = -_rigidbody.transform.right;
        }

        _rigidbody.transform.Translate(moveDirection / 2);


        for (int i = 0; i < 9; i++)
        {
            _rigidbody.transform.RotateAround(rotationAnchor.position, Vector3.up, rotationAngle);
            yield return new WaitForFixedUpdate();
        }
        wallLock = false;
        cornerDetected = false;
        Debug.Log("Corner Coroutine finished!");
    }
}