using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverIndicatorUpdate : MonoBehaviour
{
    GameObject rover;
    GameObject roverSite;

    // Populate from RoverIndicator
    public void populate(GameObject addRover, GameObject addRoverSite)
    {
        rover = addRover;
        roverSite = addRoverSite;
    }

    // Update rover indicator as player moves across the random terrain
    void FixedUpdate()
    {
        roverSite.transform.position = new Vector3(rover.transform.position.x + 2000, -2999, rover.transform.position.z);
        roverSite.transform.localEulerAngles = new Vector3(roverSite.transform.localEulerAngles.x, roverSite.transform.localEulerAngles.y, -(rover.transform.localEulerAngles.y));
    }
}
