// A simple demo script for testing and interpreting various forms of input.
// All types of input will eventuall;y be added to the freecam script and/or any other script that needs that kind of input
// Author: Benjamin Peterson

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DemoInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // .ReadValue<float>()
    public void OnWASD(InputAction.CallbackContext context)
    {
        Debug.Log("Button pressed: " + context.ReadValue<Vector2>());
    }

    public void OnVertLocal(InputAction.CallbackContext context)
    {
        Debug.Log("Button pressed: " + context.ReadValue<float>());
    }

    public void OnVertGlobal(InputAction.CallbackContext context)
    {
        Debug.Log("Button pressed: " + context.ReadValue<float>());
    }

    public void OnMouseX(InputAction.CallbackContext context)
    {
        Debug.Log("X axis: " + context.ReadValue<float>());
    }

    public void OnMouseY(InputAction.CallbackContext context)
    {
        Debug.Log("Y axis: " + context.ReadValue<float>());
    }
}
