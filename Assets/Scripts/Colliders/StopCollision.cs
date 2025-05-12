using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StopCollision : MonoBehaviour
{
    int ctr = 0;
    bool slowDown = true;
    // bool roverEntered = false;
    // float timer = 0;

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.name == "Construction")
        {
            GameObject.Find("rover").GetComponent<RotationController>().enabled = true;
            // timer = 0;
            // roverEntered = true;
        }
    }

    void Update()
    {
        // if (roverEntered)
        // {
        //     timer += Time.deltaTime;
        // }
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.name == "Construction")
        {
            if (slowDown)
            {
                Rigidbody rb = collision.gameObject.transform.parent.gameObject.GetComponent<Rigidbody>();
                rb.velocity = rb.velocity * 0.9f;
            }

            if (ctr > 90)
            {
                slowDown = false;
                //gameObject.SetActive(false);
            }
            // ctr++;

            // if (timer > 35)
            // {
            //     SceneManager.LoadScene("Surveys");
            // }
        }
    }
}
