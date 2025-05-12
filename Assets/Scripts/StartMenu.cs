using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public InputField trialNumField;    // Ref to InputField for TrialNum
    public InputField userIDInputField; // Reference to the InputField for UserID
    public Dropdown configDropdown; // Reference to the Dropdown for configuration
    public Button startButton; // Reference to the Start button
    public int fontSize = 20; // Desired font size
    public Dropdown saveDataDropdown; // Reference to the Dropdown for saving data

    private string selectedConfig;
    private string userID;
    private string trialNum;
    private bool saveDataOption;

    public static StartMenu instance;

    private void Awake()
    {
        instance = this; // Ensure that the instance is set
    }

    void Start()
    {
        // Only auto-start if this is standalone (optional)
        if (Application.isEditor || SceneManager.GetActiveScene().name == "MOUSE")
        {
            StartMouseGame();
        }

        // Set the listener for the Start button
        startButton.onClick.AddListener(OnStartButtonClicked);
        // Change the font size of the Label
        Text label = configDropdown.transform.Find("Label").GetComponent<Text>();
        if (label != null)
        {
            label.fontSize = fontSize;
        }

        // Change the font size of the dropdown options
        Transform template = configDropdown.transform.Find("Template");
        if (template != null)
        {
            Text itemText = template.Find("Viewport/Content/Item/Item Label").GetComponent<Text>();
            if (itemText != null)
            {
                itemText.fontSize = fontSize;
            }
        }
    }

    // Start running MOUSE portion of the overall simulator
    public void StartMouseGame()
    {
        OnStartButtonClicked();
    }

    // This function is called when the Start button is clicked
    public void OnStartButtonClicked()
    {
        userID = userIDInputField.text; // Get the UserID from the InputField
        selectedConfig = configDropdown.options[configDropdown.value].text; // Get the selected config
        trialNum = trialNumField.text;  // Get the trial number from input field
        if (saveDataDropdown.value == 1)
        {
            PlayerPrefs.SetString("SaveData", "true");
        }
        else
        {
            PlayerPrefs.SetString("SaveData", "false");
            Debug.Log("Data will not be saved!");
        }

        if (string.IsNullOrEmpty(userID))
        {
            Debug.LogError("User ID cannot be empty!");
            return;
        }

        // Store the user ID and selected configuration if needed for later use
        PlayerPrefs.SetString("UserID", userID);
        PlayerPrefs.SetString("SelectedConfig", selectedConfig);
        PlayerPrefs.SetString("TrialNumber", trialNum);
        Debug.Log("Stored UserID: " + PlayerPrefs.GetString("UserID"));
        Debug.Log("Stored Config: " + PlayerPrefs.GetString("SelectedConfig"));

        // Load the GameScene after the start button is clicked
        SceneManager.LoadScene("MOUSE GameScene");
    }
}
