using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoverIndicator : MonoBehaviour
{
    public Texture2D rover_tex;

    GameObject rover;
    GameObject roverSite;

    RoverIndicatorUpdate UpdateIndicator;

    float transform_adjust = Mathf.Sqrt(615)/2; //totalMap.Length

    int ctr;

    // Start is called before the first frame update
    void Start()
    {
        UpdateIndicator = GameObject.Find("RoverIndicator").GetComponent<RoverIndicatorUpdate>();
        UpdateIndicator.enabled = false;
        rover = GameObject.Find("rover");

        roverSite = new GameObject(string.Format("RoverSite"));
        Sprite rover_sprite= Sprite.Create(rover_tex, new Rect(0.0f,0.0f, rover_tex.width,rover_tex.height), Vector2.zero);
        roverSite.AddComponent<Image>();
        roverSite.GetComponent<Image>().sprite = rover_sprite;
        roverSite.transform.SetParent(this.transform, false);
        roverSite.GetComponent<RectTransform>().sizeDelta = new Vector2(rover_tex.width/3, rover_tex.height/3);
        roverSite.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        roverSite.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        roverSite.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        // Transform rover site into proper starting position
        roverSite.transform.localPosition = new Vector2(175, 0); 
        roverSite.transform.localEulerAngles = new Vector3(0, 0, 90);

        GameObject camInit = GameObject.Find("Terrain Generation/Topo Camera");
        camInit.GetComponent<roverFollow>().finishedInit();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ctr += 1;
        // Blink indicator until rover gets onto map
        if (rover.transform.position.x > 175) 
        {
            if (ctr == 30)
            {
                roverSite.GetComponent<Image>().enabled = false;
            }
            else if (ctr == 60)
            {
                roverSite.GetComponent<Image>().enabled = true;
                ctr = 0;
            }
        }
        else
        {
            // disable initialization script
            roverSite.GetComponent<Image>().enabled = true;
            UpdateIndicator.enabled = true;
            UpdateIndicator.populate(rover, roverSite);
            this.enabled = false;
        }
    }


}
