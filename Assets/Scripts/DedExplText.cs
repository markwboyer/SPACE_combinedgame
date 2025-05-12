using UnityEngine;
using UnityEngine.UI;

public class DedExplText : MonoBehaviour
{
    // Reference to the single Text component
    public Text routeInfoText;

    // Method to update the text based on the route chosen
    public void UpdateDedExplInfo(int routeIndex, float hazardTime, float goalTime, float score, float batteryRequired)//, float altHazardTime1, float altHazardTime2, float altGoalTime1, float altGoalTime2)
    {
        string infoText = "";

        switch (routeIndex)
        {
            case 3:
                //infoText += "This route avoids hazards as much as possible.\n";
                infoText += $"Hzrd time: {hazardTime:F1}s\n";
                infoText += $"Total time: {goalTime:F1}s\n";
                infoText += $"AT score: {score:F1}.\n";
                infoText += $"Batt req'd: {batteryRequired:F1}";
                break;
            case 2:
                //infoText += "This route balances hazards vs. time.\n";
                infoText += $"Hzrd time: {hazardTime:F1}s\n";
                infoText += $"Total time: {goalTime:F1}s\n";
                infoText += $"AT score: {score:F1}.\n";
                infoText += $"Batt req'd: {batteryRequired:F1}";
                break;
            case 1:
                //infoText += "This route is the most direct and doesn't avoid hazards.\n";
                infoText += $"Hzrd time: {hazardTime:F1}s\n";
                infoText += $"Total time: {goalTime:F1}s\n";
                infoText += $"AT score: {score:F1}.\n";
                infoText += $"Batt req'd: {batteryRequired:F1}";
                break;
            case 4:
                //infoText += "Manual route chosen.\n";
                infoText += $"Hzrd time: {hazardTime:F1}s\n";
                infoText += $"Total time: {goalTime:F1}s\n";
                infoText += $"AT score: {score:F1}.\n";
                infoText += $"Batt req'd: {batteryRequired:F1}";
                break;
            default:
                infoText = "No route chosen.";
                break;
        }

        // Set the combined text to the single Text component
        routeInfoText.text = infoText;
    }
}
