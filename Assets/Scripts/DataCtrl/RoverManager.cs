using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls settings for different environments
public class RoverManager : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject player;
    Transform VRCam;
    Transform ScreenCam;
    public int gamemode = 0; // 0 = VR, 1 = Screen, 2 = Mockup

    void Start()
    {
        VRCam = transform.Find("VR Cam");
        ScreenCam = transform.Find("RoverCam");
        player = GameObject.Find("Player");

        if (player != null)
        {
            gamemode = player.GetComponent<SimData>().environment;
            if (gamemode == 2) // VR Mode setup
            {
                VRCam.gameObject.SetActive(true);
                ScreenCam.gameObject.SetActive(false);
            }
            else if (gamemode == 1) // Screen Mode setup
            {
                VRCam.gameObject.SetActive(false);
                ScreenCam.gameObject.SetActive(true);
            }
            else
            {
                VRCam.gameObject.SetActive(false);
                ScreenCam.gameObject.SetActive(false);
                GameObject.Find("MockupCams").GetComponent<DisplayManager>().SetMockupDisplays();
            }
        }
    }
}
