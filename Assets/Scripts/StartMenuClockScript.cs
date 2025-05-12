using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuClockScript : MonoBehaviour
{
    public Text clockText;  // Reference to a UI Text element to display the time
    public float elapsedTime = 0f;  // Time elapsed since the start

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;  // Accumulate the time
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);

        // Update the UI text
        clockText.text = string.Format("{0:00}:{1:00}", minutes, seconds);


    }
}