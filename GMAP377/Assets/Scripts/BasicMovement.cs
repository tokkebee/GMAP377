using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : MonoBehaviour
{
    //Public
    public Camera playerCam;
    public bool menuInteracting; //Not used at the moment, only helpful for UI menus
    //Serialized Private
    [SerializeField] private CharacterController charCon;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpForce;
    //Private
    private float _vertSpeed;
    private Vector2 _lookDir;
    private Vector3 _movement;
    private Vector2 _lookInput;
    private Vector2 _moveInput;

    private void Start()//Snag CharacterController component on object
    {
        charCon = GetComponent<CharacterController>();
    }
    
    private void FixedUpdate()//Calls movement methods on fixed update
    {
        VertForceCalc();
        Movement();
    }

    private void Update() => CameraManagement(); //Manages camera on update
    
    private void Movement()//Input handling and basic movement calculations
    {
        if (!menuInteracting)
        {
            //Reads move Vector2 from Input actions (WASD by default)
            _moveInput = InputSystem.actions["Move"].ReadValue<Vector2>();
            
            float speed = Mathf.Lerp(walkSpeed, runSpeed, InputSystem.actions["Sprint"]
                .ReadValue<float>());//Interpolates between walking and running according to shift key
            
            //Sets movement floats according to inputs*speed
            float moveForwards = _moveInput.y * speed * Time.deltaTime;
            float moveSide = _moveInput.x * speed * Time.deltaTime;
            
            //Packs movement into global Vector3, adds vertical calculation (from VertForceCalc())
            _movement = (transform.forward*moveForwards)+(transform.right*moveSide)
                +_vertSpeed*transform.up;
            
            //Moves player using CharacterController component
            charCon.Move(_movement);
        }
    }

    private float VertForceCalc()
    {
        //If the player is standing on the ground, their jump force 
        if (charCon.isGrounded) _vertSpeed = InputSystem.actions["Jump"].ReadValue<float>()*jumpForce;
        
        //Otherwise, fall according to gravity.
        else _vertSpeed -= gravity * Time.deltaTime;
        return _vertSpeed;
    }
    
    void CameraManagement()
    {
        if (!menuInteracting)
        {//Read Mouse Input from input system as a Vector2
            _lookInput = InputSystem.actions["Look"].ReadValue<Vector2>();
            
            //Add additional mouse movement to values according to lookSpeed
            _lookDir.x += _lookInput.x * Time.deltaTime * lookSpeed;
            _lookDir.y -= _lookInput.y * Time.deltaTime * lookSpeed;
            
            //Ensure player is not able to do flips (Stop them from looking too far down or too far up)
            _lookDir.y = Mathf.Clamp(_lookDir.y, -90f, 90f);
            
            //Turn entire player on y axis according to _lookDir.x
            transform.localRotation = Quaternion.Euler(0f, _lookDir.x, 0f);
            
            //Turn camera on local x axis according to _lookDir.y
            playerCam.transform.localRotation = Quaternion.Euler(_lookDir.y, 0f, 0f);
        }
    }
}
