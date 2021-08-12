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

    private float verticalRotation;
    private Vector2 mouseInput;
    private Vector3 moveDirection;
    private Vector3 movement;
    private float activeMoveSpeed;



    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerLookMovement();

        HandlePlayerMovement();



    }

    private void HandlePlayerMovement()
    {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        // 
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        } else
        {
            activeMoveSpeed = movementSpeed;
        }

        
        // based on players camera direction, then remove the sideways speed increase
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed; // added here to prevent jumping sped up

        characterController.Move(movement * Time.deltaTime);
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
