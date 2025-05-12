using UnityEngine;
using UnityEngine.UI; // Required for working with UI elements

public class HazardUIController : MonoBehaviour
{
    public RoverDriving rover; // Reference to the RoverDriving script
    private Image panelImage;  // Reference to the UI Panel's Image component
    private Color safeColor = Color.green; // Default color (green)
    private Color hazardColor = Color.red; // Hazard color (red)

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
        panelImage.color = safeColor;
    }

    void Update()
    {
        // Check the rover's hazard state and change the color accordingly
        if (rover != null)
        {
            if (rover.isInHazard)
            {
                panelImage.color = hazardColor;
                //Debug.Log("UI Panel turns red (Hazard state)");
            }
            else
            {
                panelImage.color = safeColor;
            }
        }
    }
}
