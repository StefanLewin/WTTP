using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    //input fields
    private ThirdPersonActionAsset playerActionsAsset;
    private InputAction move;

    //movement fields 
    private Rigidbody _rigidbody;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;
    private bool onWall = false;
    private Vector3 WallNormal;
    private Vector3 WallHorizontal;
    private Vector3 WallVertical;
    
    [SerializeField] private Camera playerCamera;

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
        if (!onWall)
        {
            GroundMovement();
        }
        else
        {
            WallMovement();
        }
        
    }

    private void GroundMovement()
    {
        //Set Direction based on User Input
        forceDirection += move.ReadValue<Vector2>().x * movementForce * GetCameraRight(playerCamera);
        forceDirection += move.ReadValue<Vector2>().y * movementForce * GetCameraForward(playerCamera);

        //Stronger force, when in air
        forceDirection = IsGrounded() ? forceDirection : forceDirection * 3;

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

        LookAt();
    }

    private void WallMovement()
    {
        //Set Direction based on User
        //  Left-Right
        forceDirection += move.ReadValue<Vector2>().x * movementForce/2 * WallHorizontal;
        
        /*if(move.ReadValue<Vector2>().x > 0)
        {
            forceDirection += new Vector3(1, 0, 1);
        } 
        else if(move.ReadValue<Vector2>().x < 0)
        {
            forceDirection += new Vector3(-1, 0, -1);
        }*/

        //Debug.Log(Vector3.Cross(WallNormal, _rigidbody.transform.up));


        //  Up-Down
        forceDirection += move.ReadValue<Vector2>().y * movementForce/2 * WallVertical;


        //Debug.Log(forceDirection);
        //Apply Force to body 
        _rigidbody.AddForce(forceDirection, ForceMode.Impulse);

        //Reset Force
        forceDirection = Vector3.zero;


        //Make sure, that the maxSpeed is not exceeded
        Vector3 verticalVelocity = _rigidbody.velocity;
        verticalVelocity.z = 0;

        if (verticalVelocity.sqrMagnitude > maxSpeed * maxSpeed) { }
            //_rigidbody.velocity = verticalVelocity.normalized * maxSpeed + Vector3.forward * _rigidbody.velocity.z;

        //LookAt();
    }

    private void LookAt()
    {
        Vector3 direction = _rigidbody.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this._rigidbody.rotation = Quaternion.LookRotation(direction, Vector3.up);
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

    private Vector3 GetCameraUp(Camera p_playerCamera)
    {
        Vector3 up = p_playerCamera.transform.up;

        up.z = 0;
        return up.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
        else if (onWall)
        {
            InitGroundMovement();
        }
    }

    private bool IsGrounded()
    {
        Ray ray = new(this.transform.position , Vector3.down);

        return Physics.Raycast(ray, out RaycastHit hit, 1);
    }

    private bool StickToWall()
    {
        Ray ray = new(this.transform.position, _rigidbody.transform.forward);

        return Physics.Raycast(ray, out RaycastHit hit, 0.6f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (StickToWall())
        {
            Debug.Log("Wall Collision detected");
            InitWallMovement();
            Debug.Log("Contact normal 1 : " + collision.GetContact(0).normal);
            WallNormal = collision.GetContact(0).normal.normalized;
            WallHorizontal = Vector3.Cross(WallNormal, Vector3.up);
            WallVertical = Vector3.Cross(WallNormal, -_rigidbody.transform.right);

            Debug.Log("Wall Normal: " + WallNormal);
            Debug.Log("Wall Horizontal: " + WallHorizontal);
            Debug.Log("Wall Vertical: " + WallVertical);
        }             
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!StickToWall()) 
        {
            Debug.Log("Wall Collision exit");
            InitGroundMovement();
            WallNormal = Vector3.zero;
        }
    }

    private void InitWallMovement()
    {
        _rigidbody.useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
        onWall = true;
        Debug.Log("Up Vector: " + _rigidbody.transform.up);
        
    }

    private void InitGroundMovement()
    {
        onWall = false;
        _rigidbody.useGravity = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }
}
