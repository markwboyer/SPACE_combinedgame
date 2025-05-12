using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ObsTimer : MonoBehaviour
{
    // Start is called before the first frame update
    
    Stopwatch stopwatch = new Stopwatch();
    Text timer_text;

    void Start()
    {
        timer_text = GetComponent<Text>();
    }

    public void StartTimer()
    {
        stopwatch.Start();
    }

    // Update is called once per frame
    void Update()
    {
        timer_text.text = stopwatch.Elapsed.ToString(@"mm\:ss");
        if (stopwatch.Elapsed.TotalSeconds >= 30)
        {
            SceneManager.LoadScene("Surveys");
        }
    }
}
