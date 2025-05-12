using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PostTrialSurveys : MonoBehaviour
{
    public ScoreDisplay scoreDisplay; // Reference to the ScoreDisplay script
    public RoverDriving roverDriving; // Reference to the RoverDriving script

    public GameObject questionPanel; // Assign in Inspector
    public Text questionText1;        // Assign in Inspector
    public Slider answerSlider1; // Reference to the UI Slider
    public Text lowDescriptorText1; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText1; // Reference to the UI Text for the high descriptor
    public Text questionText2;        // Assign in Inspector
    public Slider answerSlider2; // Reference to the UI Slider
    public Text lowDescriptorText2; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText2; // Reference to the UI Text for the high descriptor
    public Text questionText3;        // Assign in Inspector
    public Slider answerSlider3; // Reference to the UI Slider
    public Text lowDescriptorText3; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText3; // Reference to the UI Text for the high descriptor
    public Text questionText4;        // Assign in Inspector
    public Slider answerSlider4; // Reference to the UI Slider
    public Text lowDescriptorText4; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText4; // Reference to the UI Text for the high descriptor
    public Text questionText5;        // Assign in Inspector
    public Slider answerSlider5; // Reference to the UI Slider
    public Text lowDescriptorText5; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText5; // Reference to the UI Text for the high descriptor
    public Text questionText6;        // Assign in Inspector
    public Slider answerSlider6; // Reference to the UI Slider
    public Text lowDescriptorText6; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText6; // Reference to the UI Text for the high descriptor
    public Text questionText7;        // Assign in Inspector
    public Slider answerSlider7; // Reference to the UI Slider
    public Text lowDescriptorText7; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText7; // Reference to the UI Text for the high descriptor
    public Text questionText8;        // Assign in Inspector
    public Slider answerSlider8; // Reference to the UI Slider
    public Text lowDescriptorText8; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText8; // Reference to the UI Text for the high descriptor
    public Text questionText9;        // Assign in Inspector
    public Slider answerSlider9; // Reference to the UI Slider
    public Text lowDescriptorText9; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText9; // Reference to the UI Text for the high descriptor
    public Text questionText10;        // Assign in Inspector
    public Slider answerSlider10; // Reference to the UI Slider
    public Text lowDescriptorText10; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText10; // Reference to the UI Text for the high descriptor
    public Text questionText11;        // Assign in Inspector
    public Slider answerSlider11; // Reference to the UI Slider
    public Text lowDescriptorText11; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText11; // Reference to the UI Text for the high descriptor
    public Text questionText12;        // Assign in Inspector
    public Slider answerSlider12; // Reference to the UI Slider
    public Text lowDescriptorText12; // Reference to the UI Text for the low descriptor
    public Text highDescriptorText12; // Reference to the UI Text for the high descriptor

    // 6x TLX questions - mental demand, phys demand, temp demand, perf, effort, frustration
    private string question1 = "How mentally demanding was the task?";
    private string lowDescriptor1 = "Very Low";
    private string highDescriptor1 = "Very High";

    private string question2 = "How physically demanding was the task?";
    private string lowDescriptor2 = "Very Low";
    private string highDescriptor2 = "Very High";

    private string question3 = "How hurried or rushed was the pace of the task?";
    private string lowDescriptor3 = "Very Low";
    private string highDescriptor3 = "Very High";

    private string question4 = "How successful were you in accomplishing what you were asked to do?";
    private string lowDescriptor4 = "Perfect";  //Reversed order
    private string highDescriptor4 = "Failure";

    private string question5 = "How hard did you have to work to accomplish your level of performance?";
    private string lowDescriptor5 = "Very Low";
    private string highDescriptor5 = "Very High";

    private string question6 = "How insecure; discouraged; irritated; stressed; and annoyed were you?";
    private string lowDescriptor6 = "Very Low";
    private string highDescriptor6 = "Very High";

    // 6x Questions regarding explanations (x3), trust (x3)
    private string question7 = "I understand what the agent's goals are."; // Global explanations
    private string lowDescriptor7 = "Perfectly";  // Reversed order
    private string highDescriptor7 = "Not at all";

    private string question8 = "I know how the agent makes decisions."; // Deductive explanations
    private string lowDescriptor8 = "Very Low";
    private string highDescriptor8 = "Very High";

    private string question9 = "I can see what alternative decisions are available."; // Contrastive explanations
    private string lowDescriptor9 = "Very Low";
    private string highDescriptor9 = "Very High";

    private string question10 = "I TRUST this autonomous system.";  // Overall trust
    private string lowDescriptor10 = "Not at all";
    private string highDescriptor10 = "Comple-tely";

    private string question11 = "I feel positively towards the autonomous system."; // Affective trust - Leary 2024
    private string lowDescriptor11 = "Not at all";
    private string highDescriptor11 = "Comple-tely";

    private string question12 = "I believe the system is a competent performer."; // Cognitive trust - Leary 2024
    private string lowDescriptor12 = "Not at all";
    private string highDescriptor12 = "Comple-tely";

    public Text ATScoreTextBox; // Score readout of participant AT Score and Percent of Ideal AT Score

    private string csvFilePath;
    public Button submitButton;      // Assign in Inspector

    List<string> postTrialData = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        questionPanel.SetActive(false);

        // Initialize the question and descriptors
        questionText1.text = question1;
        lowDescriptorText1.text = lowDescriptor1;
        highDescriptorText1.text = highDescriptor1;

        // Set up slider values
        answerSlider1.minValue = 0;
        answerSlider1.maxValue = 100;
        answerSlider1.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText2.text = question2;
        lowDescriptorText2.text = lowDescriptor2;
        highDescriptorText2.text = highDescriptor2;

        // Set up slider values
        answerSlider2.minValue = 0;
        answerSlider2.maxValue = 100;
        answerSlider2.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText3.text = question3;
        lowDescriptorText3.text = lowDescriptor3;
        highDescriptorText3.text = highDescriptor3;

        // Set up slider values
        answerSlider3.minValue = 0;
        answerSlider3.maxValue = 100;
        answerSlider3.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText4.text = question4;
        lowDescriptorText4.text = lowDescriptor4;
        highDescriptorText4.text = highDescriptor4;

        // Set up slider values
        answerSlider4.minValue = 0;
        answerSlider4.maxValue = 100;
        answerSlider4.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText5.text = question5;
        lowDescriptorText5.text = lowDescriptor5;
        highDescriptorText5.text = highDescriptor5;

        // Set up slider values
        answerSlider5.minValue = 0;
        answerSlider5.maxValue = 100;
        answerSlider5.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText6.text = question6;
        lowDescriptorText6.text = lowDescriptor6;
        highDescriptorText6.text = highDescriptor6;

        // Set up slider values
        answerSlider6.minValue = 0;
        answerSlider6.maxValue = 100;
        answerSlider6.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText7.text = question7;
        lowDescriptorText7.text = lowDescriptor7;
        highDescriptorText7.text = highDescriptor7;

        // Set up slider values
        answerSlider7.minValue = 0;
        answerSlider7.maxValue = 100;
        answerSlider7.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText8.text = question8;
        lowDescriptorText8.text = lowDescriptor8;
        highDescriptorText8.text = highDescriptor8;

        // Set up slider values
        answerSlider8.minValue = 0;
        answerSlider8.maxValue = 100;
        answerSlider8.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText9.text = question9;
        lowDescriptorText9.text = lowDescriptor9;
        highDescriptorText9.text = highDescriptor9;

        // Set up slider values
        answerSlider9.minValue = 0;
        answerSlider9.maxValue = 100;
        answerSlider9.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText10.text = question10;
        lowDescriptorText10.text = lowDescriptor10;
        highDescriptorText10.text = highDescriptor10;

        // Set up slider values
        answerSlider10.minValue = 0;
        answerSlider10.maxValue = 100;
        answerSlider10.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText11.text = question11;
        lowDescriptorText11.text = lowDescriptor11;
        highDescriptorText11.text = highDescriptor11;

        // Set up slider values
        answerSlider11.minValue = 0;
        answerSlider11.maxValue = 100;
        answerSlider11.value = 50; // Start slider in the middle

        // Initialize the question and descriptors
        questionText12.text = question12;
        lowDescriptorText12.text = lowDescriptor12;
        highDescriptorText12.text = highDescriptor12;

        // Set up slider values
        answerSlider12.minValue = 0;
        answerSlider12.maxValue = 100;
        answerSlider12.value = 50; // Start slider in the middle

        

        // Add a listener to the submit button
        submitButton.onClick.AddListener(OnSubmitButtonClick);
    }

    public void ShowQuestionPanel()
    {
        questionPanel.SetActive(true);
        // Fill the AT Score Box
        float ATscore = scoreDisplay.actualScore;
        float idealScore = roverDriving.idealATScore;
        ATScoreTextBox.text = $"AT Score: {ATscore:F1}   Percent of Ideal AT Score: {(ATscore / idealScore) * 100:F1}%";
    }
    private void OnSubmitButtonClick()
    {
        // Set up the CSV file path
        Debug.Log(roverDriving.filePath);
        // Add userID and selectedConfig to filenames
        string filenameSuffix = $"_User_{roverDriving.userID}_Trial_{roverDriving.trialNum}_{roverDriving.selectedConfig}.csv";
        string csvFilePath = System.IO.Path.Combine(roverDriving.filePath, "PostTrialResponses" + filenameSuffix);
        Debug.Log("Post trial file path " + csvFilePath);

        // Get the slider value
        float sliderValue1 = answerSlider1.value;
        float sliderValue2 = answerSlider2.value;
        float sliderValue3 = answerSlider3.value;
        float sliderValue4 = answerSlider4.value;
        float sliderValue5 = answerSlider5.value;
        float sliderValue6 = answerSlider6.value;
        float sliderValue7 = answerSlider7.value;
        float sliderValue8 = answerSlider8.value;
        float sliderValue9 = answerSlider9.value;
        float sliderValue10 = answerSlider10.value;
        float sliderValue11 = answerSlider11.value;
        float sliderValue12 = answerSlider12.value;

        // Add the question and answer to the list
        postTrialData.Add($"{question1},{sliderValue1}");
        postTrialData.Add($"{question2},{sliderValue2}");
        postTrialData.Add($"{question3},{sliderValue3}");
        postTrialData.Add($"{question4},{100 - sliderValue4}");
        postTrialData.Add($"{question5},{sliderValue5}");
        postTrialData.Add($"{question6},{sliderValue6}");
        postTrialData.Add($"{question7},{100 - sliderValue7}");
        postTrialData.Add($"{question8},{sliderValue8}");
        postTrialData.Add($"{question9},{sliderValue9}");
        postTrialData.Add($"{question10},{sliderValue10}");
        postTrialData.Add($"{question11},{sliderValue11}");
        postTrialData.Add($"{question12},{sliderValue12}");

        // Write the question and answer to the CSV file
        using (StreamWriter sw = new StreamWriter(csvFilePath, false))
        {
            sw.WriteLine($"{question1},{sliderValue1}");
            sw.WriteLine($"{question2},{sliderValue2}");
            sw.WriteLine($"{question3},{sliderValue3}");
            sw.WriteLine($"{question4},{100 - sliderValue4}");
            sw.WriteLine($"{question5},{sliderValue5}");
            sw.WriteLine($"{question6},{sliderValue6}");
            sw.WriteLine($"{question7},{100 - sliderValue7}");
            sw.WriteLine($"{question8},{sliderValue8}");
            sw.WriteLine($"{question9},{sliderValue9}");
            sw.WriteLine($"{question10},{sliderValue10}");
            sw.WriteLine($"{question11},{sliderValue11}");
            sw.WriteLine($"{question12},{sliderValue12}");
        }

        //Debug.Log($"Question: {question1} | Response: {sliderValue1}");

        // Export to GitHub
        roverDriving.UploadCSVtoGitHub(csvFilePath, postTrialData, "PostTrialResponses", filenameSuffix);

        // Exit the game and return to editor
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
