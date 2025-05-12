using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public Button testButton;

    void Start()
    {
        testButton.onClick.AddListener(OnButtonClicked);
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("Screen touched at: " + touch.position);
            }
        }
    }
    void OnButtonClicked()
    {
        Debug.Log("Test Button Clicked!");
        testButton.gameObject.GetComponent<Image>().color = Color.red; // Change button color to red
    }
}
