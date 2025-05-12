using UnityEngine;

public class InstantiateTextboxes : MonoBehaviour
{
    public GameObject customTextboxPrefab; // Assign the CustomTextbox prefab in the Inspector
    public Transform parentContainer; // Assign the CustomTextboxContainer panel in the Inspector

    void Start()
    {
        for (int i = 0; i < 15; i++)
        {
            // Instantiate a new CustomTextbox and set its parent to the container
            GameObject newTextbox = Instantiate(customTextboxPrefab, parentContainer);
            newTextbox.name = "CustomTextbox_" + (i + 1); // Name the instances for easier identification
        }
    }
}
