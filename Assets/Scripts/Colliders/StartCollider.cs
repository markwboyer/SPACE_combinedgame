using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCollider : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject rover;
    void Start()
    {
        rover = GameObject.Find("rover");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.name == "Construction")
        {
            //rover.GetComponent<PowerController>().enabled = true;
            rover.GetComponent<PowerController>().mult = 0.02f; // Enable power draw when driving
        }
    }
}
