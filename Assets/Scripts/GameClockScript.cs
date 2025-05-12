using UnityEngine;
using UnityEngine.UI;

public class GameClockScript : MonoBehaviour
{
    public Text clockText;  // Reference to a UI Text element to display the time
    public float elapsedTime = 0f;  // Time elapsed since the start
    public RoverDriving roverDriving;  // Reference to the RoverDriving script

    // Update is called once per frame
    void FixedUpdate()
    {
        
        elapsedTime = roverDriving.elapsedTimeForScore;  // Get the total elapsed time from the RoverDriving script
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);

        // Update the UI text
        clockText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        

    }
}
