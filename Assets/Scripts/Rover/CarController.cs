using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class Axle {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;

    public float horizontalAxis;
    public float verticalAxis;
}

public class CarController : MonoBehaviour
{
    public List<Axle> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public float prevMotor = 0;
    public float calcMotor = 0;
    public float battery = 15;
    bool isBraking = false;
    Rigidbody rb;

    [HideInInspector] public float horizontalFloat;
    [HideInInspector] public float verticalFloat;

    public Button quitButton;

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    void Start()
    {
        // Change wheel substeps in order to remove wheelCollider jitter
        foreach (Axle axleInfo in axleInfos)
        {
            axleInfo.leftWheel.ConfigureVehicleSubsteps(5, 12, 15);
            axleInfo.rightWheel.ConfigureVehicleSubsteps(5, 12, 15);
        }

        // Lower center of gravity to avoid tipping issues
        rb = gameObject.GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, 1.8f, -5.747f); //-1.911005f for  (0,0.5, -5.747)

        // Disable the rotation controller for the time being until it is needed in the observation phase
        GetComponent<RotationController>().enabled = false;

        // Assign the QuitButtonClick method to the button's onClick event
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitButtonClick);
        }

    }

    public void HorizontalFire(InputAction.CallbackContext context)
    {
        //Debug.Log("Context: " + context.ReadValue<float>());
        horizontalFloat = context.ReadValue<float>();
    }

    public void VerticalFire(InputAction.CallbackContext context)
    {
        //Debug.Log("Context: " + context.ReadValue<float>());
        verticalFloat = context.ReadValue<float>();
        //Debug.Log("verical value: " + verticalFloat);
    }

    public void onBrake(InputAction.CallbackContext context)
    {
        if (context.performed)
            isBraking = true;
        if (context.canceled)
            isBraking = false;
    }

    public float getPowerEfficiency()
    {
        return Mathf.Clamp(1.0f - verticalFloat, 0.05f, 0.99f);
    }

    // This is used when the player flips the vehicle
    // Should result in a binary failure
    public void Reset()
    {
        //this.gameObject.transform.eulerAngles.z = 0;
        // Internal flag to activate binary failure
        GameObject.Find("Player").GetComponent<SimData>().flippedVehicle = true;
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }

    //  **ADDED COLLISION DETECTION HERE**
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Rover collided with object: " + collision.gameObject.name);

        //// Check if the collided object is tagged "Terrain Chunk"
        //if (collision.gameObject.name.Contains("Chunk"))
        //{
        //    Debug.Log("Rover collided with Terrain Chunk: " + collision.gameObject.name);
        //    // Check for high acceleration or significant rotation
        //    float collisionForce = collision.relativeVelocity.magnitude;
        //    float angle = Vector3.Angle(transform.up, Vector3.up);
        //    Debug.Log("Collision Force: " + collisionForce + " Angle: " + angle);

        //    if (collisionForce > 5f || angle > 10f) // Threshold values for force and angle
        //    {
        //        Debug.Log("High impact detected! Force: " + collisionForce + ", Angle: " + angle);
        //        // Handle crash logic here
        //        GameObject.Find("Player").GetComponent<SimData>().crashedVehicle = true;

        //    }
        //}
        // Check for high acceleration or significant rotation
        float collisionForce = collision.relativeVelocity.magnitude;
        float angle = Vector3.Angle(transform.up, Vector3.up);
        Debug.Log("Collision Force: " + collisionForce + " Angle: " + angle);
        if (collisionForce > 3f || angle > 5f) // Threshold values for force and angle
        {
            Debug.Log("High impact detected! Force: " + collisionForce + ", Angle: " + angle);
            // Handle crash logic here
            GameObject.Find("Player").GetComponent<SimData>().crashedVehicle = true;

        }
    }



    public void onSandboxReset()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void quickFinish()
    {
        SceneManager.LoadScene("Surveys");
    }

    public void FixedUpdate()
    {
        float motor = 0;
        if (rb.velocity.magnitude * 3.6f <= 30) // Set the maximum speed here
        {
            motor = maxMotorTorque * verticalFloat;
        }

        float steering = maxSteeringAngle * horizontalFloat;

        foreach (Axle axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            if (isBraking)
            {
                // Debug.Log("Braking!");
                axleInfo.leftWheel.brakeTorque = 1200;
                axleInfo.rightWheel.brakeTorque = 1200;
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

        // Due to all-wheel drive, this motor torque will be applied to all 6 wheels
        prevMotor = calcMotor;
        calcMotor = Mathf.Abs(motor * 6);

        // Calculate the angle between the vehicle's up direction and world up direction
        float angle = Vector3.Angle(transform.up, Vector3.up);

        // Check if the angle exceeds the flip threshold (40 degrees)
        if (angle > 40f)
        {
            Debug.Log("Vehicle is flipped more than " + 40f + " degrees!");
            GameObject.Find("Player").GetComponent<SimData>().crashedVehicle = true;
            QuitButtonClick();
            //GameObject.Find("Feedback").GetComponent<Feedback>().SendDataToServer();
            //GameObject.Find("Feedback").GetComponent<Feedback>().EndSession();
        }
    }

    // Method to handle Quit button click
    public void QuitButtonClick()
    {
        GameObject.Find("Feedback").GetComponent<Feedback>().SendDataToServer();      
        //GameObject.Find("Feedback").GetComponent<Feedback>().EndSession();
    }

}
