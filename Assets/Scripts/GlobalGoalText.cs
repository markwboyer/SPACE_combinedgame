using UnityEngine;
using UnityEngine.UI;

public class GlobalGoalText : MonoBehaviour
{
    // Reference to the Text component
    public Text routeInfoText;

    // Method to update the text based on the route chosen
    public void UpdateRouteInfo(int routeIndex)
    {
        switch (routeIndex)
        {

            case 1:
                routeInfoText.text = "Minimize time.";
                break;
            case 2:
                routeInfoText.text = "Balance time vs. hazards";
                break;
            case 3:
                routeInfoText.text = "Avoid hazards.";
                break;
            case 4:
                routeInfoText.text = "Manual Route Chosen";
                break;
            default:
                routeInfoText.text = "No route chosen.";
                break;
        }
    }
}