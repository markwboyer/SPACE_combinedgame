using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuIterator : MonoBehaviour
{
    public GameObject[] menuButtons;
    public InputAction browseMenu;
    public InputAction click = new InputAction(type: InputActionType.Button);

    int idx = 0;
    bool hasReleased = true;
    // When changing scenes, button presses are preserved, so this boolean is here to ensure that 
    // button presses are updated at the change of the scene
    bool unpressed = false;

    GameObject focusButton;
    
    // Start is called before the first frame update
    void Start()
    {
        focusButton = menuButtons[0];
        focusButton.GetComponent<Image>().color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        float joyVal = browseMenu.ReadValue<float>();
        if (joyVal < 0 && hasReleased)
        {
            //Debug.Log("backwards idx: " + idx);
            if (idx == 0)
            {
                idx = menuButtons.GetLength(0) - 1;
                updateSelection(0);
            }
            else
            {
                idx -= 1;
                updateSelection(0);
            }
            hasReleased = false;
        }
        if (joyVal > 0 && hasReleased)
        {
            if (idx < menuButtons.GetLength(0) - 1)
            {
                idx += 1;
                updateSelection(1);
            }
            else
            {
                idx = 0;
                updateSelection(1);
            }
            hasReleased = false;
        }
        if (joyVal == 0)
        {
            hasReleased = true;
        }

        //bool clicked = (int)click.ReadValue<float>();
        //Debug.Log(click.ReadValue<bool>());
        if (click.triggered && unpressed)
        {
            // Debug.Log("fire!");
            
            focusButton.GetComponent<Button>().onClick.Invoke();
            
            unpressed = false;
        }
        if (!click.triggered)
        {
            unpressed = true;
        }
    }

    void updateSelection(int direction)
    {
        focusButton.GetComponent<Image>().color = Color.white;
        focusButton = menuButtons[idx];
        if (!focusButton.activeSelf)
        {
            if (direction == 0) // Direction was down
            {
                if (idx == 0)
                {
                    idx = menuButtons.GetLength(0) - 1;
                }
                else
                {
                    idx--;
                }
            }
            else // Direction was up
            {
                if (idx == menuButtons.GetLength(0) - 1)
                {
                    idx = 0;
                }
                else
                {
                    idx++;
                }
            }
        }
        //Debug.Log("Setting focus to: " + idx);
        focusButton = menuButtons[idx];
        focusButton.GetComponent<Image>().color = Color.green;
    }


    private void OnEnable() 
    {
        click.Enable();
        browseMenu.Enable();
    }

    private void OnDisable()
    {
        click.Disable();
        browseMenu.Disable();
    }
}
