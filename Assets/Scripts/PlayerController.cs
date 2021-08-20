using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    public bool isInverted = false;
    public float movementSpeed = 5f;
    public float runSpeed = 8f;
    public CharacterController characterController;
    public float jumpForce = 12f;
    public float gravityModifier = 2.5f;
    public Transform groundCheckPoint;
    public LayerMask groundLayers;
    public GameObject bulletImpactEffect;
    public float shotCounter;
    public float maxHeatValue = 10f;
    public float cooldownRate = 4f;
    public float overHeatCooldown = 5f;
    public Weapon[] weapons;
    public float muzzleDisplayTime;

    private Camera camera;
    private float verticalRotation;
    private Vector2 mouseInput;
    private Vector3 moveDirection;
    private Vector3 movement;
    private float activeMoveSpeed;
    private bool isGrounded;
    private float heatCounter;
    private bool isOverheated;
    private int selectedWeapon = 0;
    private float muzzleCounter;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camera = Camera.main;

        UIController.instance.overheatSlider.maxValue = maxHeatValue;

        SetWeapon();

        Transform newSpawnLocation = SpawnManager.instance.GetRandomSpawnPoint();
        transform.position = newSpawnLocation.position;
        transform.rotation = newSpawnLocation.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerLookMovement();
        HandlePlayerMovement();
        HandleMouseCursor();
        HandleWeaponActions();
        HandleWeaponSwitch();
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
    }

    /// <summary>
    /// Handles Mouse Cursor clicking in and out of game window
    /// </summary>
    private void HandleMouseCursor()
    {
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

    /// <summary>
    ///  Handles Weapons Main Fire
    /// </summary>
    private void HandleWeaponActions()
    {

        if (weapons[selectedWeapon].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;

            if (muzzleCounter <= 0)
            {
                weapons[selectedWeapon].muzzleFlash.SetActive(false);
            }
        }


        if (!isOverheated)
        {
            // Shooting
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            // full auto shooting
            if (Input.GetMouseButton(0) && weapons[selectedWeapon].isFullAuto)
            {
                shotCounter -= Time.deltaTime; // count down
                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }
            heatCounter -= cooldownRate * Time.deltaTime; // down 4 per sec
        }
        else
        {
            heatCounter -= overHeatCooldown * Time.deltaTime;
            if (heatCounter <= 0)
            {
                isOverheated = false;
                UIController.instance.overheatMessage.gameObject.SetActive(false);
            }
        }

        if (heatCounter < 0)
        {
            heatCounter = 0f;
        }

        // setting overheat ui
        UIController.instance.overheatSlider.value = heatCounter;
    }

    /// <summary>
    /// Mouse Wheel and number keys, to Switch Weapons
    /// </summary>
    private void HandleWeaponSwitch()
    {
        // switching guns uaing mouse wheel, up
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedWeapon++;
            if (selectedWeapon >= weapons.Length)
            {
                // wrap around to first weapon
                selectedWeapon = 0;
            }
            SetWeapon();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f) // MouseWheel Down
        {
            selectedWeapon--;
            if (selectedWeapon < 0)
            {
                // wrap around to last weapon
                selectedWeapon = weapons.Length - 1;
            }
            SetWeapon();
        }

        // handle number row press
        for (int i = 0; i < weapons.Length; i++)
        {            
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedWeapon = i;
                SetWeapon();
            }
        }
    }

    /// <summary>
    ///  Shoots Primary weapons Main fire
    /// </summary>
    private void Shoot()
    {
        // use a raycast for shooting
        Ray ray = camera.ViewportPointToRay(new Vector3(.5f, .5f, 0)); // center
        ray.origin = camera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log($"Shot collided with: {hit.collider.gameObject.name}");

            GameObject bulletImpactObject = Instantiate(bulletImpactEffect, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));

            // Cleanup
            Destroy(bulletImpactObject, 10f);
        }

        // Overheating
        shotCounter = weapons[selectedWeapon].timeBetweenShots;

        heatCounter = weapons[selectedWeapon].heatPerShot;

        if (heatCounter >= maxHeatValue)
        {
            heatCounter = maxHeatValue;
            isOverheated = true;

            UIController.instance.overheatMessage.gameObject.SetActive(true);
        }

        weapons[selectedWeapon].muzzleFlash.SetActive(true);

        muzzleCounter = muzzleDisplayTime;
        // TODO: would like to set rotation rng rotation of x axis

        //weapons[selectedWeapon].muzzleFlash.transform.rotation.x = UnityEngine.Random.rotation.x; 
    }

    /// <summary>
    /// Handles Mouse Movements for Camera
    /// </summary>
    private void HandlePlayerLookMovement()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

        // rotation to the camera
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotation += mouseInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);

        if (isInverted)
        {
            // rotating the view port, clamp to maxs
            viewPoint.rotation = Quaternion.Euler(verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
    }    

    /// <summary>
    ///  Activeat current selected weapon and deactivate all others
    /// </summary>
    private void SetWeapon()
    {
        foreach (Weapon weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
        }

        weapons[selectedWeapon].gameObject.SetActive(true);        
        weapons[selectedWeapon].muzzleFlash.SetActive(false);
    }
}
