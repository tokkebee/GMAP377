using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : MonoBehaviour
{
    //Public
    public Camera playerCam;
    public GameObject camHolder;
    public bool menuInteracting; //Not used at the moment, only helpful for UI menus
    //Serialized Private
    [SerializeField] private CharacterController charCon;
    private Vector3 startPosition;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] float crouchTime;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpForce;
    public bool bobEnabled;
    private bool isCrouching = false;
    [SerializeField, Range(0, 30)] private float frequency; //amount the camera bobs
    [SerializeField, Range(0, 0.1f)] private float amplitude; //intensity of each bob
    //Private
    private float _vertSpeed;
    private Vector2 _lookDir;
    private Vector3 _movement;
    private Vector2 _lookInput;
    private Vector2 _moveInput;
    private float standingHeight;
    private Coroutine crouchRoutine;
    private bool transitioning = false;

    private void Start()//Snag CharacterController component on object
    {
        charCon = GetComponent<CharacterController>();
        startPosition = playerCam.transform.localPosition; //set camera start position to the local
        standingHeight = charCon.height; //gets the height of the player
    }
    
    private void FixedUpdate()//Calls movement methods on fixed update
    {
        VertForceCalc();
        Crouching();
        Movement();
    }

    private void Update() => CameraManagement(); //Manages camera on update
    
    private void Movement()//Input handling and basic movement calculations
    {
        if (!menuInteracting)
        {
            //Reads move Vector2 from Input actions (WASD by default)
            _moveInput = InputSystem.actions["Move"].ReadValue<Vector2>();
            //Decides speed based on crouching
            float speed = !isCrouching ? Mathf.Lerp(walkSpeed, runSpeed, InputSystem.actions["Sprint"]
                    .ReadValue<float>()) //Interpolates between walking and running according to shift key
                    :crouchSpeed; 
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

    private void Crouching()
    {
        //Checks if the player is grounded, not alreadying trying to crouch, and crouch is pressed
        if (InputSystem.actions["Crouch"].WasPressedThisFrame() && !transitioning && charCon.isGrounded)
        {
            //stops incomplete coroutine (might be redundant)
            if(crouchRoutine != null)
            {
                StopCoroutine(crouchRoutine);
            }
            //Decides wheter to crouch or stand
            float targetHeight = isCrouching ? standingHeight : standingHeight/2;
            crouchRoutine = StartCoroutine(AdjustHeight(targetHeight));
        }
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
        isCrouching = isCrouching ? false : true;
        transitioning = false;
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
            _lookDir.y = Mathf.Clamp(_lookDir.y, -80f, 90f);
            
            //Turn entire player on y axis according to _lookDir.x
            transform.localRotation = Quaternion.Euler(0f, _lookDir.x, 0f);
            
            //Turn camera on local x axis according to _lookDir.y
            camHolder.transform.localRotation = Quaternion.Euler(_lookDir.y, 0f, 0f);
        }
        //camera bobbing is enabled to start the camera bob
        if (bobEnabled)
        {
            if (InputSystem.actions["Move"].ReadValue<Vector2>() != Vector2.zero) //Bob if moving
            {
                CameraBob();
            }
            ResetPosition(); //put the camera back
            playerCam.transform.LookAt(FocusTarget()); //stablinzing
        }
    }

    private void CameraBob()
    {
        //Increases/decreases the frequency based on sprinting/crouching/walking
        float finalFreq = !isCrouching ? Mathf.Lerp(frequency, frequency*2, InputSystem.actions["Sprint"].ReadValue<float>())
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