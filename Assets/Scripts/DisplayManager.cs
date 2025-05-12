using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    private void Awake()
    {
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            Display.displays[i].SetRenderingResolution(Display.displays[i].systemWidth, Display.displays[i].systemHeight);
        }
    }

    public void SetMockupDisplays()
    {
        Camera left_cam = GameObject.Find("left_window_cam").GetComponent<Camera>();
        Camera right_cam = GameObject.Find("right_window_cam").GetComponent<Camera>();
        Camera front_cam = GameObject.Find("front_window_cam").GetComponent<Camera>();
        Camera pfd_cam = GameObject.Find("mockup_pfd_cam").GetComponent<Camera>();
        Camera mouse_cam = GameObject.Find("SPACE_mouse_cam").GetComponent<Camera>();

        // Mouse camera setup
        mouse_cam.transform.SetParent(null);
        mouse_cam.stereoTargetEye = StereoTargetEyeMask.None;
        mouse_cam.orthographic = true;
        mouse_cam.orthographicSize = 110.0f;
        mouse_cam.transform.position = new Vector3(0, 0, -20);
        //mouse_cam.transform.rotation = Quaternion.Euler(0, 0, -90f);

        Debug.LogFormat("Found {0} displays", Display.displays.Length);

        // Assign cameras to displays
        left_cam.enabled = false; // not used
        right_cam.enabled = false; // not used
        front_cam.enabled = true;
        pfd_cam.enabled = true;
        mouse_cam.enabled = true;

        // Set targetDisplay for each camera
        front_cam.targetDisplay = 1; // Display 2 - front view
        pfd_cam.targetDisplay = 2;   // Display 3 - PFD
        mouse_cam.targetDisplay = 3; // Display 4 - MOUSE

        // Optionally duplicate front_cam to another camera for Display 5
        Camera front_cam_clone = Instantiate(front_cam);
        front_cam_clone.name = "front_window_cam_clone";
        front_cam_clone.targetDisplay = 4; // Display 5
        front_cam_clone.enabled = true;

        // Audio listener on one camera only
        //foreach (var listener in FindObjectsOfType<AudioListener>())
        //{
        //    listener.enabled = false;
        //}
        //front_cam.GetComponent<AudioListener>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
