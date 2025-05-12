using UnityEngine;
using UnityEngine.UI; // Use TMPro if you're using TextMeshPro
public class GoalNumberText : MonoBehaviour

{
    public Text numberText; // Reference to the Text component (or use TMPro.TextMeshProUGUI for TextMeshPro)

    public void SetGoalNumber(int number)
    {
        if (numberText != null)
        {
            numberText.text = number.ToString(); // Set the text to display the number
        }
        else
        {
            Debug.LogError("Number Text component not assigned.");
        }
    }
}

