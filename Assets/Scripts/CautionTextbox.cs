using UnityEngine;
using UnityEngine.UI;

public class CautionTextbox : MonoBehaviour
{
    public Image backgroundImage; // Assign the button's image in the Inspector
    public Text buttonText; // Assign the button's text component in the Inspector
    public Color buttonTextColor = Color.yellow;
    public Color backgroundColor = Color.black;

    private void Awake()
    {
        buttonText.color = buttonTextColor;
        backgroundImage.color = backgroundColor;

        if (backgroundImage == null || buttonText == null)
        {
            Debug.LogError("Please assign the background image and text in the Inspector.");
        }
    }
    public string GetText()
    {
        return buttonText.text;
    }
    public void ActivateTextbox(bool isRed)
    {
        if (isRed)
        {
            // Red background with white text
            backgroundImage.color = Color.red;
            buttonText.color = Color.white;
        }
        else
        {
            // Yellow background with black text
            backgroundImage.color = Color.yellow;
            buttonText.color = Color.black;
        }
    }

    public void DeactivateTextbox()
    {
        backgroundImage.color = Color.black;
        buttonText.color = Color.yellow;
    }
    public void DeactivateTextboxToCaution()
    {
        backgroundImage.color = Color.yellow;
        buttonText.color = Color.black;
    }
}
