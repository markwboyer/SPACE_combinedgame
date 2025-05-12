using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    public bool isNextPoint = false;

    // Used to detect landing on the ground
    void OnCollisionEnter(Collision collision)
    {
        // Freeze y position so sphere doesn't fall through the ground
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        // Change path point to trigger so that it is non-collidable
        gameObject.GetComponent<Collider>().isTrigger = true;

        //Debug.Log("Collided with: " + collision.gameObject.name);
    }
    // Used when player enters path
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.name == "RoverBack" && isNextPoint)
        {
            // Call to overhead function to update the valid path nodes
            GameObject pathController = GameObject.Find("Terrain Generation");
            pathController.GetComponent<PathGenerator>().updatePath();
            
            Destroy(gameObject);
        }
        //Debug.Log("Collision entity: " + collision.gameObject.name);
    }
}
