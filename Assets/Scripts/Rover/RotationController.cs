using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Allows the player to rotate during the observation task so that they can see all of the rocks

public class RotationController : MonoBehaviour
{
    // Start is called before the first frame update
    public InputAction rotateBinding;
    float rotateState;
    float newRot;
    GameObject rover;

    void Start()
    {
        rotateBinding.Enable();
        rover = GameObject.Find("rover");
    }

    // Update is called once per frame
    void Update()
    {
        // Straight is y = 270 -> give +- 30 degrees each direction
        rotateState = rotateBinding.ReadValue<float>() * 0.25f;
        newRot = Mathf.Clamp(rover.transform.localEulerAngles.y + rotateState, 240, 300);
        rover.transform.localEulerAngles = new Vector3(rover.transform.localEulerAngles.x, newRot, rover.transform.localEulerAngles.z);
        //Debug.Log("Rotation Value: " + rotateState + " Y Value: " + rover.transform.localEulerAngles.y + rotateState);
    }
}
