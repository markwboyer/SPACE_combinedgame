// NOTE: this script is a heavily modified version from Ashley Davis' github
// Link: https://gist.github.com/ashleydavis/f025c03a9221bc840a2b
// Updated for new input system by: Benjamin Peterson

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A simple free camera to be added to a Unity game object.
/// 
/// Keys:
///	wasd / arrows	- movement
///	q/e 			- up/down (local space)
///	r/f 			- up/down (world space)
///	hold shift		- enable fast movement mode
///	right mouse  	- enable free look
///	mouse			- free look / rotation
///     
/// </summary>
public class FreeCam : MonoBehaviour
{
    /// <summary>
    /// Normal speed of camera movement.
    /// </summary>
    public float movementSpeed = 10f;

    /// <summary>
    /// Speed of camera movement when shift is held down,
    /// </summary>
    public float fastMovementSpeed = 100f;

    /// <summary>
    /// Sensitivity for free look.
    /// </summary>
    public float freeLookSensitivity = 3f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel.
    /// </summary>
    public float zoomSensitivity = 10f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel (fast mode).
    /// </summary>
    public float fastZoomSensitivity = 50f;

    /// <summary>
    /// Set to true when free looking (on right mouse button).
    /// </summary>
    private bool looking = false;
    
    private float mouseX;
    private float mouseY;

    private Vector2 moveVal;
    float vertVal;
    float moveMult;

    void Update()
    {
        /*var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;*/

        if (looking)
        {
            float newRotationX = transform.localEulerAngles.y + mouseX * freeLookSensitivity;
            float newRotationY = transform.localEulerAngles.x - mouseY * freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        transform.Translate(new Vector3(moveVal.x, vertVal, moveVal.y) * (movementSpeed + (movementSpeed * moveMult)) * Time.deltaTime);
    }

    void OnDisable()
    {
        StopLooking();
    }

    public void OnShift(InputAction.CallbackContext context)
    {   
        moveMult = context.ReadValue<float>();
    }

    public void OnMouseX(InputAction.CallbackContext context)
    {
        //Debug.Log("Button pressed: " + context.ReadValue<float>());
        mouseX = context.ReadValue<float>() / 12;

    }

    public void OnMouseY(InputAction.CallbackContext context)
    {
        //Debug.Log("Button pressed: " + context.ReadValue<float>());
        mouseY = context.ReadValue<float>() / 12;

    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        //Debug.Log("Button pressed: " + context.ReadValue<float>());
        if (context.ReadValue<float>() == 1)
        {
            StartLooking();
        }
        else
        {
            StopLooking();
        }

    }

    public void OnWASD(InputAction.CallbackContext context)
    {
        //Debug.Log("Button pressed: " + context.ReadValue<Vector2>());
        Vector2 readValue = context.ReadValue<Vector2>();
        moveVal = readValue;
    }

    public void OnVertLocal(InputAction.CallbackContext context)
    {
        //Debug.Log("Button pressed: " + context.ReadValue<float>());
        float readValue = context.ReadValue<float>();
        vertVal = readValue;
    }

    public void OnVertGlobal(InputAction.CallbackContext context)
    {
        //Debug.Log("Button pressed: " + context.ReadValue<float>());
        float readValue = context.ReadValue<float>();

        if (readValue == 1) // R was pressed
        {
            movementSpeed = movementSpeed * 2;
        }
        if (readValue == -1) // F was pressed
        {
           movementSpeed = movementSpeed / 2;
        }
    }

    /// <summary>
    /// Enable free looking.
    /// </summary>
    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Disable free looking.
    /// </summary>
    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}