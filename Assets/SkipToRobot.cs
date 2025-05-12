using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Diagnostics;
using UnityEngine.UI;

// Allows the player to rotate during the observation task so that they can see all of the rocks

public class SkipToRobot : MonoBehaviour
{
    // Start is called before the first frame update
    public InputAction skipBinding;
    private bool skipped = false;

    Stopwatch pfd_stopwatch;
    Text pfd_phase;

    void Start()
    {
        skipBinding.Enable();
        pfd_phase = GameObject.Find("mp_text").GetComponent<Text>();
        pfd_stopwatch = GameObject.Find("PFD_Upper").GetComponent<PFDScript>().phase_stopwatch;
    }

    // Update is called once per frame
    void Update()
    {
        if (!skipped && (skipBinding.ReadValue<float>() > 0.0f || ShouldEndEarly()))
        {
            StartRobotTask();
        }
    }

    bool ShouldEndEarly()
    {
        string cur_phase = pfd_phase.text;
        double time_elapsed = pfd_stopwatch.Elapsed.TotalSeconds;
        return cur_phase == "Navigation" && time_elapsed >= 180;
    }

    // moves the rover into the collider that triggers the robot task
    public void StartRobotTask()
    {
        transform.position = new Vector3(-200f, 11.6f, 3.3f);
        skipped = true;
    }
}
