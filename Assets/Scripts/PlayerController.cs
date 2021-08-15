using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    public bool IsInverted = false;
    public float movementSpeed = 5f;
    public float runSpeed = 8f;
    public CharacterController characterController;
    public float jumpForce = 12f;
    public float gravityModifier = 2.5f;
    public Transform groundCheckPoint;
    public LayerMask groundLayers;

    private Camera camera;
    private float verticalRotation;
    private Vector2 mouseInput;
    private Vector3 moveDirection;
    private Vector3 movement;
    private float activeMoveSpeed;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerLookMovement();
        HandlePlayerMovement();
    }

    // Happens after Update
    private void LateUpdate()
    {
        HandleCameraPostion();
    }

    private void HandleCameraPostion()
    {
        camera.transform.position = viewPoint.position;
        camera.transform.rotation = viewPoint.rotation;
    }

    private void HandlePlayerMovement()
    {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        // running
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        } else
        {
            activeMoveSpeed = movementSpeed;
        }

        float velocity = movement.y;

        // based on players camera direction, then remove the sideways speed increase
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed; // added here to prevent jumping sped up
        movement.y = velocity;

        if (characterController.isGrounded)
        {
            // set velocity when NOT on ground
            movement.y = 0f;
        }

        // start, direction, how far, what to hit
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

        // jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityModifier; // apply gravity
    
        characterController.Move(movement * Time.deltaTime);


        if (Input.GetMouseButtonDown(0))
        {            
            Shoot();
        }

        // Mouse Cursor Logic
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // player clicked on the window
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }        
    }

    private void Shoot()
    {
        // use a raycast for shooting
        Ray ray = camera.ViewportPointToRay(new Vector3(.5f, .5f, 0)); // center
        ray.origin = camera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Shot collided with: {hit.collider.gameObject.name}");
        }
    }

    private void HandlePlayerLookMovement()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

        // rotation to the camera
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotation += mouseInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);

        if (IsInverted)
        {
            // rotating the view port, clamp to maxs
            viewPoint.rotation = Quaternion.Euler(verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
    }
}
