using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class roverFollow : MonoBehaviour
{
    GameObject roverIndicator; 
    bool hasDisabled = false;
    // Start is called before the first frame update
    
    void Start()
    {
        //this.enabled = false;
    }

    public void finishedInit()
    {
        this.enabled = true;
        //Debug.Log("Finished init called!");
        roverIndicator = GameObject.Find("RoverIndicator/RoverSite");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("RoverIndicator x position: " + roverIndicator.transform.localPosition.x);
        if (roverIndicator.transform.localPosition.x > -35 && roverIndicator.transform.localPosition.x < 35)
        {
            transform.position = new Vector3(roverIndicator.transform.position.x, -2980, transform.position.z);
        }
        // transform.position = new Vector3(roverIndicator.transform.position.x, transform.position.y, roverIndicator.transform.position.z);

    }
}
