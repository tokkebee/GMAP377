using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : MonoBehaviour
{
    public Camera playerCam;
    public bool menuInteracting;
    [SerializeField] private CharacterController charCon;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpForce;
    private float _vertSpeed;
    private Vector2 _lookDir;
    private Vector3 _movement;
    private Vector2 _lookInput;
    private Vector2 _moveInput;

    private void Start()
    {
        charCon = GetComponent<CharacterController>();
    }
    
    private void FixedUpdate()
    {
        VertForceCalc();
        Movement();
    }

    private void Update()
    {
        CameraManagement();
    }
    

    private void Movement()
    {
        if (!menuInteracting)
        {
            _moveInput = InputSystem.actions["Move"].ReadValue<Vector2>();
            float speed = Mathf.Lerp(walkSpeed, runSpeed, InputSystem.actions["Sprint"]
                .ReadValue<float>());
            float moveForwards = _moveInput.y * speed * Time.deltaTime;
            float moveSide = _moveInput.x * speed * Time.deltaTime;
            _movement = (transform.forward*moveForwards)+(transform.right*moveSide)
                +_vertSpeed*transform.up;
            charCon.Move(_movement);
        }
        
    }

    private float VertForceCalc()
    {
        if (charCon.isGrounded) 
        {
            _vertSpeed = 0f;
            _vertSpeed += InputSystem.actions["Jump"].ReadValue<float>()*jumpForce;
        }
        else
        {
            _vertSpeed -= gravity * Time.deltaTime;
        }
        return _vertSpeed;
    }
    
    void CameraManagement()
    {
        if (!menuInteracting)
        {
            _lookInput = InputSystem.actions["Look"].ReadValue<Vector2>();
            _lookDir.x += _lookInput.x * Time.deltaTime * lookSpeed;
            _lookDir.y -= _lookInput.y * Time.deltaTime * lookSpeed;
            _lookDir.y = Mathf.Clamp(_lookDir.y, -90f, 90f);
            transform.localRotation = Quaternion.Euler(0f, _lookDir.x, 0f);
            playerCam.transform.localRotation = Quaternion.Euler(_lookDir.y, 0f, 0f);
        }
    }
    
}
