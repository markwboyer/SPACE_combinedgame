using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.LSL4Unity.Scripts; 

public class PathGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    //float[,] path = new float[,] {{305,0},{280,0},{260,40},{220,10},{180,-30},{120,-50},{100,-80},{50,-30},{0,0},{-30,10}, {-70,30}, {-100,45},{-140,25},{-180,0}, {-220,-30},{-250,-50}/*,{-290,-30},{-305,-15}*/};
    private int total = 0;
    private int collected = 0;

    Queue<GameObject> nextPoint = new Queue<GameObject>();

    //LSL Markers
    private LSLMarkerStream triggers;

    void Awake()
    {
        //Find LSL Stream
        triggers = FindObjectOfType<LSLMarkerStream>();
    }


    public void createPath(List<float[]> path)
    {
        // Note that terrain edge is 305
        // Iterate through each path coordinate and place a sphere at that coordinate
        for (int i = 0; i < path.Count; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            float[] pathPoint = path[i];
            sphere.transform.position = new Vector3(pathPoint[0], 20f, pathPoint[1]);
            sphere.transform.localScale = new Vector3(3.6f, 3.6f, 3.6f);
            sphere.name = "path_" + i;
            
            // Add gravity so we don't have to worry about height map variability
            sphere.AddComponent<Rigidbody>();
            sphere.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            // Set default color to red and attach script to it
            sphere.GetComponent<Renderer>().material.color = new Color32(255,0,0,255);
            sphere.AddComponent<PathPoint>();

            if (i == 0) // Make the first path point a valid point
            {
                sphere.GetComponent<Renderer>().material.color = new Color32(0,255,0,255);
                sphere.GetComponent<PathPoint>().isNextPoint = true;
            }
            else // Otherwise, add it to the list of path points for later
            {
                nextPoint.Enqueue(sphere);
            }
        }
        total = path.Count;
    }

    public void updatePath()
    {
        collected += 1;
        Debug.Log("New score: " + collected + "/" + total);
        if (nextPoint.Count != 0) // Update the target path point as long as a next point exists in the queue (causes a crash otherwise)
        {
            GameObject newTarget = nextPoint.Dequeue();
            newTarget.GetComponent<PathPoint>().isNextPoint = true;
            newTarget.GetComponent<Renderer>().material.color = new Color32(0,255,0,255);
        }
        else // Indicates completion of the task
        {
            triggers.Write("Navigation Task Ended");
            Debug.Log("Finished the navigation task!");
        }

    }
}
