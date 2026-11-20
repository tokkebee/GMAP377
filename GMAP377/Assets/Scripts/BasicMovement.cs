using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : MonoBehaviour
{
    //Public
    public Camera playerCam;
    public bool menuInteracting; //Not used at the moment, only helpful for UI menus
    public bool bobEnabled;

    public enum MoveState
    {
        Crouching,
        Running,
        Walking
    }
    public MoveState currentState = MoveState.Walking;
    //Serialized Private
    [SerializeField] private CharacterController charCon;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpForce;
<<<<<<< Updated upstream
=======
    [SerializeField, Range(0, 30)] private float frequency; //amount the camera bobs
    [SerializeField, Range(0, 0.1f)] private float amplitude; //intensity of each bob
    [SerializeField] private float walkVolume, crouchVolume, runVolume;
>>>>>>> Stashed changes
    //Private
    private float _vertSpeed;
    private Vector2 _lookDir;
    private Vector3 _movement;
    private Vector2 _lookInput;
    private Vector2 _moveInput;
<<<<<<< Updated upstream
=======
    private AlertBroadcastManager _alert;
    private float standingHeight;
    private Coroutine crouchRoutine;
    private bool transitioning = false;
    //I'm a little iffy on the action stuff but, here's theoretically how it should work.
    //This creates the actions onPlayerCrouch and onPlayer Stand
    public static event Action onPlayerCrouch;
    public static event Action onPlayerStand;

    private void Awake()
    {
        _alert = FindAnyObjectByType<AlertBroadcastManager>();
    }
>>>>>>> Stashed changes

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
        if (menuInteracting) return;
        //Reads move Vector2 from Input actions (WASD by default)
        _moveInput = InputSystem.actions["Move"].ReadValue<Vector2>();
        //Decides speed based on crouching
        // float speed = currentState ? Mathf.Lerp(walkSpeed, runSpeed, InputSystem.actions["Sprint"]
        //         .ReadValue<float>()) //Interpolates between walking and running according to shift key
        //         :crouchSpeed;
        if (currentState != MoveState.Crouching)
        {
<<<<<<< Updated upstream
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
=======
            if (InputSystem.actions["Sprint"].ReadValue<float>() > 0) currentState = MoveState.Running;
            else currentState = MoveState.Walking;
>>>>>>> Stashed changes
        }
        
        float speed, volume;
        switch (currentState)
        {
            case MoveState.Running:
                speed = runSpeed;
                volume = runVolume;
                break;
            case MoveState.Crouching:
                speed = crouchSpeed;
                volume = crouchVolume;
                break;
            default:
                speed = walkSpeed;
                volume = walkVolume;
                break;
        }
        
        //Sets movement floats according to inputs*speed
        float moveForwards = _moveInput.y * speed * Time.deltaTime;
        float moveSide = _moveInput.x * speed * Time.deltaTime;
        
        //Packs movement into global Vector3, adds vertical calculation (from VertForceCalc())
        _movement = (transform.forward*moveForwards)+(transform.right*moveSide)
            +_vertSpeed*transform.up;
        
        //Moves player using CharacterController component
        charCon.Move(_movement);
        if (_movement.magnitude > 0.0001) _alert.Alert(transform.position,volume);
    }

    private float VertForceCalc()
    {
        //If the player is standing on the ground, their jump force 
        if (charCon.isGrounded) _vertSpeed = InputSystem.actions["Jump"].ReadValue<float>()*jumpForce;
        
        //Otherwise, fall according to gravity.
        else _vertSpeed -= gravity * Time.deltaTime;
        return _vertSpeed;
    }
<<<<<<< Updated upstream
=======

    private void Crouching()
    {
        //Checks if the player is grounded, not alreadying trying to crouch, and crouch is pressed
        //stops incomplete coroutine (might be redundant)
        if(crouchRoutine != null)
        {
            StopCoroutine(crouchRoutine);
        }
        //Decides wheter to crouch or stand
        float targetHeight = currentState == MoveState.Crouching ? standingHeight : standingHeight/2;
        if (currentState == MoveState.Crouching)
        {
            onPlayerStand?.Invoke(); //Invokes standing if the player is already crouching
        }
        else
        {
            onPlayerCrouch?.Invoke(); //Invokes crouching if the player isn't crouching yet
        }
        crouchRoutine = StartCoroutine(AdjustHeight(targetHeight));
    }

    private IEnumerator AdjustHeight(float targetHeight) //Lerps between the current height and the final height
    {
        float currentHeight = charCon.height;
        float elapsedTime = 0.0f; //time passed each frame
        transitioning = true;
        while (elapsedTime < crouchTime)
        {
            elapsedTime += Time.deltaTime;
            float timeScale = elapsedTime/crouchTime; //how quickly we should be lerping
            charCon.height = Mathf.Lerp(currentHeight, targetHeight, timeScale);
            yield return null;
        }
        charCon.height = targetHeight;
        currentState = currentState == MoveState.Crouching ? MoveState.Walking : MoveState.Crouching;
        transitioning = false;
    }
>>>>>>> Stashed changes
    
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
<<<<<<< Updated upstream
}
=======

    private void CameraBob()
    {
        //Increases/decreases the frequency based on sprinting/crouching/walking
        float finalFreq = currentState != MoveState.Crouching ? Mathf.Lerp(frequency, frequency*2, InputSystem.actions["Sprint"].ReadValue<float>())
        : frequency/2;
        finalFreq = Mathf.Clamp(finalFreq, 0, 30); //make sure we don't have the frequency go too high
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * finalFreq) * amplitude;
        pos.x += Mathf.Cos(Time.time * finalFreq/2) * amplitude * 2;
        playerCam.transform.localPosition+=pos;
    
    }

    private void ResetPosition() //Lerp to camera start position if the camera isn't at it
    {
        if (playerCam.transform.localPosition != startPosition)
        {
            playerCam.transform.localPosition = Vector3.Lerp(playerCam.transform.localPosition, startPosition, 1 * Time.deltaTime);
        }
    }

    private Vector3 FocusTarget() //Stablizing based on the position of the camera holder
    {
        Vector3 pos = new Vector3(playerCam.transform.position.x, 
        playerCam.transform.position.y + camHolder.transform.localPosition.y, 
        playerCam.transform.position.z);
        pos += camHolder.transform.forward * 15.0f;
        return pos;
    }
}
>>>>>>> Stashed changes
