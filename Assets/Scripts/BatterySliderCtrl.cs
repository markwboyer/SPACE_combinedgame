using UnityEngine;
using UnityEngine.UI;

public class BatterySliderCtrl : MonoBehaviour
{
    public Slider batterySlider; // Reference to the slider
    public Text percentageText; // Reference to the text
    public float drainRate = 5f; // Default Rate at which battery drains
    public float movingDrainMultiplier = 5f; // Multiplier when the rover is moving

    public float batteryLevel = 100f; // Start battery at 100%
    private RoverDriving rover; // Reference to the RoverDriving script

    private Color normalColor = Color.green; // Normal slider and text color
    private Color lowBatteryColor = Color.red; // Low battery color

    private float updateInterval = 0.5f; // Interval in seconds
    private float timeSinceLastUpdate = 0f; // Time accumulator
    private float movingElapsedTime = 0f; // Time spent moving
    private float previousElapsedTime = 0f; // Previous elapsed time

    public SAGATPopupManager sagatManager; // Reference to the SAGATPopupManager script

    void Start()
    {
        // Get the RoverDriving component
        rover = FindObjectOfType<RoverDriving>();

        if (batterySlider != null)
        {
            batterySlider.maxValue = 100f;
            batterySlider.value = batteryLevel; // Set the initial slider value
        }
        UpdateBatteryDisplay();
    }

    void FixedUpdate()
    {
        if (sagatManager.SAGATrunning)
        {
            return; // Skip battery update if SAGAT is running
        }
        timeSinceLastUpdate += Time.fixedDeltaTime;
        

        if (batteryLevel > rover.finalBatteryLevel)  // Prevent the battery from jumping back up after completing a round of goals
        {
            Debug.Log("Error: Battery level is higher than final battery level.  Setting battery level to final battery level: batteryLevel: " + batteryLevel + " roverDriving final batt level: " + rover.finalBatteryLevel);
            batteryLevel = rover.finalBatteryLevel;
            return;
        }

        if (timeSinceLastUpdate >= updateInterval)  // Reduce update rate to only once every >0.5sec interval (can change as desired)
        {
            //Debug.Log("Battery level is: " + batteryLevel + ".  RoverDriving finalbattlevel is: " + rover.finalBatteryLevel);
            timeSinceLastUpdate = 0f; // Reset the timer

            if (batteryLevel > 0)
            {
                // Check if the rover is moving and adjust the drain rate
                float currentDrainRate = drainRate;
                //Debug.Log("Current drain rate: " + currentDrainRate);

                if (rover != null && !rover.isPaused)
                {
                    //Debug.Log("Rover is moving. Start batt is: " + batteryLevel);
                    currentDrainRate *= movingDrainMultiplier; // Increase the drain rate if the rover is moving
                    //Debug.Log("Current drain rate: " + currentDrainRate);
                    movingElapsedTime = rover.elapsedTime - previousElapsedTime;
                    //Debug.Log("Rover elapsedTime: " + rover.elapsedTime + ".  Batt movingElapsedTime: " + movingElapsedTime + " PreviousElapsedTime: " + previousElapsedTime);
                    previousElapsedTime = rover.elapsedTime;
                    // Decrease battery level based on the current drain rate
                    batteryLevel -= currentDrainRate * movingElapsedTime;
                    batteryLevel = Mathf.Clamp(batteryLevel, 0, 100); // Keep battery level within bounds
                    //Debug.Log("Current battery level: " + batteryLevel);
                }
                else
                {
                    batteryLevel -= currentDrainRate * updateInterval; // Decrease battery level based on the baseline drain rate * baseline time updates
                    batteryLevel = Mathf.Clamp(batteryLevel, 0, 100); // Keep battery level within bounds
                }

                //    // Decrease battery level based on the current drain rate
                //    batteryLevel -= currentDrainRate * movingElapsedTime;
                //batteryLevel = Mathf.Clamp(batteryLevel, 0, 100); // Keep battery level within bounds

                UpdateBatteryDisplay();
            }
        }
    }

    void UpdateBatteryDisplay()
    {
        // Update the slider and text display
        if (batterySlider != null)
        {
            batterySlider.value = batteryLevel;
        }
        // Change colors if battery is below 10%
        if (batteryLevel < 10f)
        {
            batterySlider.fillRect.GetComponent<Image>().color = lowBatteryColor;
            percentageText.color = lowBatteryColor;
            rover.isPaused = true;
            rover.isMoving = false;
        }
        else
        {
            batterySlider.fillRect.GetComponent<Image>().color = normalColor;
            percentageText.color = normalColor;
        }
        if (percentageText != null)
        {
            percentageText.text = $"{Mathf.RoundToInt(batteryLevel)}%"; // Round to nearest integer
        }
    }
}
