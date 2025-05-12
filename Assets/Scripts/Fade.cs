using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.LSL4Unity.Scripts;

public class Fade : MonoBehaviour
{
    Image img;
    bool fading_out = true;
    bool task_performed = false;
    bool done = false;
    float timer = 0.0f;

    //LSL Markers
    private LSLMarkerStream triggers; //For

    void Awake()
    {
        //Find LSL Stream
        triggers = FindObjectOfType<LSLMarkerStream>();
    }

    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
     * Fade screen to black, perform the given task, then fade back in to scene.
     */
    public IEnumerator FadeOutIn(System.Action task, float task_duration = 1.0f, int speed = 1) {
        float fade_amount;
        while (fading_out && img.color.a < 1)
        {
            fade_amount = img.color.a + (speed * Time.deltaTime);
            Color new_color = new Color(img.color.r, img.color.g, img.color.b, fade_amount);
            img.color = new_color;
            if (fade_amount >= 1.0f)
            {
                fading_out = false;
                triggers.Write("Fail"); 
            }
            yield return new WaitForEndOfFrame();
        }
        if (!fading_out && !task_performed)
        {
            task();
            task_performed = true;
            triggers.Write("Success");
            timer += Time.deltaTime;
            if (timer < task_duration)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        if (!done && task_performed)
        {
            while (img.color.a > 0)
            {
                float t = Time.deltaTime;
                fade_amount = img.color.a - (speed * t);
                Color new_color = new Color(img.color.r, img.color.g, img.color.b, fade_amount);
                img.color = new_color;
                if (fade_amount <= 0.0f)
                {
                    done = true;
                    triggers.Write("Success");
                }
                yield return new WaitForEndOfFrame();
            }
        }
        if (done)
        {
            // reset variables for next call of this function
            fading_out = true;
            task_performed = false;
            done = false;
            timer = 0.0f;
        }
    }
}
