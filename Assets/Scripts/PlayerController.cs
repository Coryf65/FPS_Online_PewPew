using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    public bool IsInverted = false;

    private float verticalRotation;
    private Vector2 mouseInput;



    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
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
        } else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }



    }
}
