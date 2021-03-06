using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerController : MonoBehaviourPunCallbacks
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
    public GameObject playerHitEffect;
    public int maxHealth = 100;
    public Animator animator;
    public Transform modelWeaponPoint;
    public Transform weaponHolder;
    public GameObject playerModel;
    public float adsSpeed;
    public Transform adsOutPoint;
    public Transform adsInPoint;
    public AudioSource footstepSlow;
    public AudioSource footstepFast;

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
    private int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CreateCamera();

        currentHealth = maxHealth;
        UIController.instance.healthDisplay.SetActive(true);
        UIController.instance.overheatSlider.maxValue = maxHeatValue;
        UIController.instance.healthAmountText.text = currentHealth.ToString();

        PositionWeaponOnPlayer();
        photonView.RPC("SetWeapon", RpcTarget.All, selectedWeapon);

        // set players skin
        playerModel.GetComponent<Renderer>().material = SkinSelector.instance.allSkins[SkinSelector.instance.SkinIndex];
    }

    private void CreateCamera()
    {
        //camera = photonView.gameObject.AddComponent<Camera>();
        //Instantiate(camera, photonView.transform.position, photonView.transform.rotation);
        camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {       
        if (photonView.IsMine)
        {
            HandleMouseCursor();
            if (!Cursor.visible)
            {
                HandlePlayerLookMovement();
                HandlePlayerMovement();
                HandlePlayerAnimations();
                HandleWeaponActions();
                HandleWeaponSwitch();
                HandleMenuActions();
            }            
        }  
    }

    // Happens after Update
    private void LateUpdate()
    {
        //if (photonView.IsMine)
        //{
            if (MatchManager.instance.currentState == MatchManager.GameState.Playing)
            {
                HandleCameraPostion();
            } else
            {
                // set to view map
                camera.transform.position = MatchManager.instance.mapCameraPoint.position;
                camera.transform.rotation = MatchManager.instance.mapCameraPoint.rotation;
            }
        //}
    }

    /// <summary>
    /// Sets up the players weapon position
    /// </summary>
    private void PositionWeaponOnPlayer()
    {
        weaponHolder.parent = modelWeaponPoint;
        weaponHolder.localPosition = Vector3.zero;
        weaponHolder.localRotation = Quaternion.identity;
    }

    private void HandleMenuActions()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UIController.instance.ToggleUI();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIController.instance.ToggleDisplayLeaderboards();
        }
    }

    private void HandlePlayerAnimations()
    {
        animator.SetBool("grounded", isGrounded);
        // how much distance we are covering, always a positive number
        animator.SetFloat("speed", movement.magnitude); 
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
            if (!footstepFast.isPlaying && moveDirection != Vector3.zero)
            {
                footstepFast.Play();
                footstepSlow.Stop();
            }
        } else
        {
            activeMoveSpeed = movementSpeed;
            if (!footstepSlow.isPlaying && moveDirection != Vector3.zero)
            {
                footstepSlow.Play();
                footstepFast.Stop();
            }
        }

        // Off the ground or not moving
        if (moveDirection == Vector3.zero || !isGrounded)
        {
            footstepFast.Stop();
            footstepSlow.Stop();
        }

        float yVelocity = movement.y;
        // based on players camera direction, then remove the sideways speed increase
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed;
        movement.y = yVelocity;

        if (characterController.isGrounded)
        {
            // set velocity when NOT on ground
            movement.y = 0f;
        }

        // start, direction, how far, what to hit
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .4f, groundLayers);

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
            if (Input.GetMouseButtonDown(0) && !UIController.instance.optionsScreen.activeInHierarchy)
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

        // ADS
        if (Input.GetMouseButtonDown(1))
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, weapons[selectedWeapon].adsZoom, adsSpeed * Time.deltaTime);
            weaponHolder.position = Vector3.Lerp(weaponHolder.position, adsInPoint.position, adsSpeed * Time.deltaTime);
        } else
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 60f, adsSpeed * Time.deltaTime);
            weaponHolder.position = Vector3.Lerp(weaponHolder.position, adsOutPoint.position, adsSpeed * Time.deltaTime);
        }
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
            photonView.RPC("SetWeapon", RpcTarget.All, selectedWeapon);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f) // MouseWheel Down
        {
            selectedWeapon--;
            if (selectedWeapon < 0)
            {
                // wrap around to last weapon
                selectedWeapon = weapons.Length - 1;
            }
            photonView.RPC("SetWeapon", RpcTarget.All, selectedWeapon);
        }

        // handle number row press
        for (int i = 0; i < weapons.Length; i++)
        {            
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedWeapon = i;
                photonView.RPC("SetWeapon", RpcTarget.All, selectedWeapon);
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
            if (hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(playerHitEffect.name, hit.point, Quaternion.identity);

                // Calls DamagePlayer()
                hit.collider.gameObject.GetPhotonView().RPC("DamagePlayer", RpcTarget.All, photonView.Owner.NickName, weapons[selectedWeapon].weaponDamage, PhotonNetwork.LocalPlayer.ActorNumber); // sending nickname across the RPC call
            } else
            {
                GameObject bulletImpactObject = Instantiate(bulletImpactEffect, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactObject, 10f);
            }
            
        }

        // Overheating
        shotCounter = weapons[selectedWeapon].timeBetweenShots;

        heatCounter += weapons[selectedWeapon].heatPerShot;

        if (heatCounter >= maxHeatValue)
        {
            heatCounter = maxHeatValue;
            isOverheated = true;

            UIController.instance.overheatMessage.gameObject.SetActive(true);
        }

        // TODO: would like to set rotation rng rotation of x axis
        //weapons[selectedWeapon].muzzleFlash.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));

        weapons[selectedWeapon].muzzleFlash.SetActive(true);

        muzzleCounter = muzzleDisplayTime;

        weapons[selectedWeapon].shotSound.Stop();
        weapons[selectedWeapon].shotSound.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageDealer"></param>
    /// <param name="damageAmount"></param>
    [PunRPC] // can call this function to run at the same time across network
    public void DamagePlayer(string damageDealer, int damageAmount, int actorId)
    {
        Damage(damageDealer, damageAmount, actorId);
    }

    /// <summary>
    ///  Handled as an RPC
    /// </summary>
    /// <param name="damageDealer"></param>
    /// <param name="damageAmount"></param>
    public void Damage(string damageDealer, int damageAmount, int actorId)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damageAmount;
            Mathf.Clamp(currentHealth, 0, maxHealth);

            UIController.instance.healthAmountText.text = currentHealth.ToString();
            // TODO: maybe make heart shrink?


            if (currentHealth <= 0)
            {
                currentHealth = 0;
                PlayerSpawner.instance.Died(damageDealer);
                MatchManager.instance.UpdateStatsSend(actorId, 0, 1); // increase kill stat
            }

            UIController.instance.healthAmountText.text = currentHealth.ToString();
        }        
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
    private void SwitchWeapon()
    {
        foreach (Weapon weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
        }

        weapons[selectedWeapon].gameObject.SetActive(true);        
        weapons[selectedWeapon].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    private void SetWeapon(int weaponIndex)
    {
        if (weaponIndex < weapons.Length)
        {
            selectedWeapon = weaponIndex;
            SwitchWeapon();
        }
    }
}
