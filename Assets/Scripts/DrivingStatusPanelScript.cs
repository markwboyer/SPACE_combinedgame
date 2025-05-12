using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for working with UI elements

public class DrivingStatusPanelScript : MonoBehaviour
{
    public RoverDriving rover; // Reference to the RoverDriving script
    public TextboxManager textboxManager; // Ref to the Warning Textbox Manager
    public BatterySliderCtrl batterySliderCtrl; // Ref to the Battery Slider Controller
    private Image panelImage;  // Reference to the UI Panel's Image component
    private Color moveColor = Color.green; // Default color (green)
    private Color stoppedColor = Color.red; // Hazard color (red)

    void Start()
    {
        // Get the Image component from the UI Panel this script is attached to
        panelImage = GetComponent<Image>();

        if (rover == null)
        {
            // Find the RoverDriving component if not assigned in the Inspector
            rover = FindObjectOfType<RoverDriving>();
        }

        // Set the Panel's initial color to green
        panelImage.color = stoppedColor;
    }

    void Update()
    {
        // Check the rover's hazard state and change the color accordingly
        if (rover != null)
        {
           
            if (rover.isMoving && !textboxManager.roverDriveWarning && batterySliderCtrl.batteryLevel>10)
            {
                panelImage.color = moveColor;
            }
            else
            {
                panelImage.color = stoppedColor;
            }
        }
    }
}
