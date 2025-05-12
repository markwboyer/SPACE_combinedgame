using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SAGATPopupManager : MonoBehaviour
{
    public RoverDriving roverDriving; // Reference to the RoverDriving script
    public BatterySliderCtrl batterySliderCtrl; // Reference to the BatterySliderCtrl script
    public TextboxManager textboxManager; // Reference to the TextboxManager script
    public ScoreDisplay scoreDisplay; // Reference to the ScoreDisplay script
    public AStarPathfinder pathfinder; // Reference to the AStarPathfinder script

    public GameObject questionPanel; // Assign in Inspector
    public Text questionText1;        // Assign in Inspector
    public Button[] answerButtons1;   // Assign in Inspector
    public Text questionText2;        // Assign in Inspector
    public Button[] answerButtons2;   // Assign in Inspector
    public Button submitButton;      // Assign in Inspector
    public Text questionText3;        // Assign in Inspector
    public Button[] answerButtons3;   // Assign in Inspector
    public Text trustText1;
    public Slider trustSlider1;
    public Text trust1LowText;
    public Text trust1HighText;
    public Text trustText2;
    public Slider trustSlider2;
    public Text trust2LowText;
    public Text trust2HighText;

    public Button SAGATAcknowledgeButton; // Assign in Inspector

    private string trust1 = "I TRUST this autonomous system.";  // Overall trust
    private string lowDescriptorTrust1 = "Not at all";
    private string highDescriptorTrust1 = "Completely";

    private string trust2 = "I would RELY ON this autonomous system.";  // Overall reliance
    private string lowDescriptorTrust2 = "Not at all";
    private string highDescriptorTrust2 = "Completely";

    public List<string> sagatLogEntries = new List<string>();   // List to log SAGAT answers
    public List<string> sagatTrustLogEntries = new List<string>();  // List to log trust answers

    private string selectedAnswer1;
    private string selectedAnswer2;
    private string selectedAnswer3;
    public bool SAGATrunning = false;
    private string correctAnswer1;
    private string correctAnswer2;
    private string correctAnswer3;
    private bool isCorrect1 = false;
    private bool isCorrect2 = false;
    private bool isCorrect3 = false;

    public Color selectedColor = Color.green;
    public Color defaultColor = Color.white;

    void Start()
    {
        questionPanel.SetActive(false);
        trustText1.text = trust1;
        trust1LowText.text = lowDescriptorTrust1;
        trust1HighText.text = highDescriptorTrust1;

        trustText2.text = trust2;
        trust2LowText.text = lowDescriptorTrust2;
        trust2HighText.text = highDescriptorTrust2;


        // Attach listeners to answer buttons for Question 1
        foreach (Button button in answerButtons1)
        {
            button.onClick.AddListener(() => OnAnswerSelected1(button));
        }

        // Attach listeners to answer buttons for Question 2
        foreach (Button button in answerButtons2)
        {
            button.onClick.AddListener(() => OnAnswerSelected2(button));
        }

        // Attach listeners to answer buttons for Question 3
        foreach (Button button in answerButtons3)
        {
            button.onClick.AddListener(() => OnAnswerSelected3(button));
        }

        submitButton.onClick.AddListener(OnSubmit);
        SAGATAcknowledgeButton.onClick.AddListener(OnAcknowledgeSAGAT);
    }

    public void runSAGAT(int localtargetIndex)
    {
        //float battPercent = batterySliderCtrl.batteryLevel;
        //float drainRate = batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier;
        //List<Node> remainingPath = roverPath.Skip(targetIndex).ToList();
        //Debug.Log("Remaining nodes on path " + remainingPath.Count);
        //float remainingTime = remainingPath.Count / (speed);
        //Debug.Log("Remaining time is " + remainingTime);
        //float batt2finish = battPercent - remainingTime * drainRate;
        //Debug.Log("Expected battery at finish is " + batt2finish);

        string[] questionsLvl1 = {
            "What is the current battery percentage?",
            "What are the current warnings?",
            "What are the current cautions?",
            "How many goals has MOUSE visited so far?",
            "What is the current AT Score?"
        };

        string[] questionsLvl2 = {

            "Is MOUSE currently at risk? (In Hazard OR WARN/CAUTION present)",
            "Is MOUSE able to collect scientific data?",
            "Is MOUSE able to move?",
            "Does MOUSE have enough battery to complete the route?",
            "How far along the route is MOUSE?"
        };

        string[] questionsLvl3 = {
            "What is MOUSE's predicted score at the end of the current route?",
            "What percent of remaining route will MOUSE spend in hazard?",
            "What time will MOUSE finish the current route?",
            "How much battery will MOUSE finish the route with?"
        };

        string[] battAnswers = FillBattAnswers();
        string[] warningAnswers = FillWarningAnswers();
        string[] cautionAnswers = FillCautionAnswers();
        string[] goalsVisitedAnswers = FillGoalsVisitedAnswers();
        string[] currentScoreAnswers = FillCurrentScoreAnswers();

        string[] hazardStatusAnswers = FillHazardStatusAnswers();
        string[] sciAbilityAnswers = FillSciAbilityAnswers();
        string[] driveAbilityAnswers = FillDriveAbilityAnswers();
        string[] batteryToFinishAnswers = FillBatt2FinishAnswers(localtargetIndex);
        string[] roverProgressAnswers = FillRoverProgAnswers(localtargetIndex);

        string[] predscoreAnswers = FillPredScoreAnswers();
        string[] predHazardTimeAnswers = FillPredHazardAnswers(localtargetIndex);
        string[] predTimeFinishAnswers = FillTimeFinishAnswers(localtargetIndex);
        string[] battFinishAnswers = FillBattFinishAnswers(localtargetIndex);

        //string[] battAnswers = { "1", "2", "3", "4" };
        //string[] warningAnswers = { "1", "2", "3", "4" };
        //string[] cautionAnswers = { "1", "2", "3", "4" };
        //string[] goalsVisitedAnswers = { "1", "2", "3", "4" };
        //string[] currentScoreAnswers = { "1", "2", "3", "4" };

        //string[] hazardStatusAnswers = { "1", "2", "3", "4" };
        //string[] sciAbilityAnswers = { "1", "2", "3", "4" };
        //string[] driveAbilityAnswers = { "1", "2", "3", "4" };
        //string[] batteryToFinishAnswers = { "1", "2", "3", "4" };
        //string[] roverProgressAnswers = { "1", "2", "3", "4" };

        //string[] predscoreAnswers = { "1", "2", "3", "4" };
        //string[] predHazardTimeAnswers = { "1", "2", "3", "4" };
        //string[] predTimeFinishAnswers = { "1", "2", "3", "4" };
        //string[] battFinishAnswers = { "1", "2", "3", "4" };


        string[][] answersLvl1 = {
            battAnswers,
            warningAnswers,
            cautionAnswers,
            goalsVisitedAnswers,
            currentScoreAnswers
        };
        string[][] answersLvl2 = {
            hazardStatusAnswers,
            sciAbilityAnswers,
            driveAbilityAnswers,
            batteryToFinishAnswers,
            roverProgressAnswers
        };
        string[][] answersLvl3 = {
            predscoreAnswers,
            predHazardTimeAnswers,
            predTimeFinishAnswers,
            battFinishAnswers
        };

        // Pick a random question and display the panel
        int randomIndex1 = UnityEngine.Random.Range(0, questionsLvl1.Length);
        int randomIndex2 = UnityEngine.Random.Range(0, questionsLvl2.Length);
        int randomIndex3 = UnityEngine.Random.Range(0, questionsLvl3.Length);

        Debug.Log("Question 1: " + randomIndex1 + " question2 " + randomIndex2 + " question 3 " + randomIndex3);
        ShowQuestionPanel(questionsLvl1[randomIndex1], answersLvl1[randomIndex1], questionsLvl2[randomIndex2], answersLvl2[randomIndex2], questionsLvl3[randomIndex3], answersLvl3[randomIndex3]);
        //Time.timeScale = 1f;  // Gets game going again after questionnaires complete
    }

    // Level 1 answers
    string[] FillBattAnswers()
    {
        string[] answers = new string[4];
        float battPercent = batterySliderCtrl.batteryLevel;
        // Ensure the correct answer is added first
        answers[0] = $"{battPercent:F0}%";

        // Create a HashSet to avoid duplicate random numbers
        HashSet<float> uniqueAnswers = new HashSet<float> { battPercent };

        // Generate three random, unique numbers between 0 and 100
        float minValue = 0f;

        // Calculate the quartile ranges
        float quartileSize = 25f;
        float[] quartileRanges = new float[4];
        for (int i = 0; i < 4; i++)
        {
            quartileRanges[i] = minValue + quartileSize * i;
        }

        // Determine the quartile of the actual score
        int actualScoreQuartile = (int)((battPercent - minValue) / quartileSize);
        HashSet<int> quartilesFilled = new HashSet<int> { actualScoreQuartile };
        while (quartilesFilled.Count < 4)
        {
            int randomQuartile;
            do
            {
                randomQuartile = UnityEngine.Random.Range(0, 4);
            } while (quartilesFilled.Contains(randomQuartile));

            quartilesFilled.Add(randomQuartile);
            float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

            if (!uniqueAnswers.Contains(randomValue))
            {
                uniqueAnswers.Add(randomValue);
                Debug.Log("BattAnswers random value is " + randomValue + "  real value: " + battPercent);

            }
        }

        // Convert the unique numbers into the answers array, skipping the first correct answer
        int index = 1;
        foreach (float value in uniqueAnswers)
        {
            if (value != battPercent)
            {
                answers[index] = $"{value:F0}%";
                index++;
            }
        }

        // Shuffle the answers array to randomize the position of the correct answer
        //ShuffleArray(answers);
        return answers;
    }
    string[] FillWarningAnswers()
    {
        string[] answers = new string[4];
        int[] textboxStatuses = textboxManager.textboxesStatus;
        // Find all indices where the value equals 2
        int[] twosIndices = textboxStatuses
            .Select((value, index) => new { value, index })
            .Where(item => item.value == 2)
            .Select(item => item.index)
            .ToArray();

        int selectedIndex = 0;
        if (twosIndices.Length == 0)
        {
            //Debug.Log("No element with value 2 found in the array.");
            answers[0] = "None";
            //return null;
        }
        else
        {
            // Randomly select one instance where the value equals 1
            selectedIndex = twosIndices[UnityEngine.Random.Range(0, twosIndices.Length)];
            answers[0] = $"{textboxManager.textboxes[selectedIndex].GetText()}";  // Set correct answer to 0 position
        }

        // Find all indices where the value equals 0 and exclude the chosen index
        int[] zerosIndices = textboxStatuses
            .Select((value, index) => new { value, index })
            .Where(item => item.value == 0 && item.index != selectedIndex)
            .Select(item => item.index)
            .ToArray();

        if (zerosIndices.Length < 3)
        {
            //Debug.LogError("Not enough elements with value 0 found in the array.");
            return null;
        }

        // Randomly select three indices for value 0
        zerosIndices = zerosIndices.OrderBy(x => UnityEngine.Random.value).ToArray();
        for (int i = 1; i <= 3; i++)
        {
            answers[i] = textboxManager.textboxes[zerosIndices[i - 1]].GetText();
        }

        return answers;


    }
    string[] FillCautionAnswers()
    {
        string[] answers = new string[4];
        int[] textboxStatuses = textboxManager.textboxesStatus;
        // Find all indices where the value equals 1
        int[] onesIndices = textboxStatuses
            .Select((value, index) => new { value, index })
            .Where(item => item.value == 1)
            .Select(item => item.index)
            .ToArray();
        int selectedIndex = 0;
        if (onesIndices.Length == 0)
        {
            //Debug.Log("No element with value 1 found in the array.");
            answers[0] = "None";
            //return null;
        }
        else
        {
            // Randomly select one instance where the value equals 1
            selectedIndex = onesIndices[UnityEngine.Random.Range(0, onesIndices.Length)];
            answers[0] = $"{textboxManager.textboxes[selectedIndex].GetText()}";  // Set correct answer to 0 position
        }

        // Find all indices where the value equals 0 and exclude the chosen index
        int[] zerosIndices = textboxStatuses
            .Select((value, index) => new { value, index })
            .Where(item => item.value == 0 && item.index != selectedIndex)
            .Select(item => item.index)
            .ToArray();

        if (zerosIndices.Length < 3)
        {
            //Debug.LogError("Not enough elements with value 0 found in the array.");
            return null;
        }

        // Randomly select three indices for value 0
        zerosIndices = zerosIndices.OrderBy(x => UnityEngine.Random.value).ToArray();
        for (int i = 1; i <= 3; i++)
        {
            answers[i] = textboxManager.textboxes[zerosIndices[i - 1]].GetText();
        }

        return answers;


    }
    string[] FillGoalsVisitedAnswers()
    {
        string[] answers = new string[4];
        int goalCount = roverDriving.goalCount;
        answers[0] = $"{goalCount:F0}";
        // Create a HashSet to avoid duplicate random numbers
        HashSet<int> uniqueAnswers = new HashSet<int> { goalCount };

        // Generate three random, unique numbers between -1000 and 1000
        while (uniqueAnswers.Count < 4)
        {
            int randomValue = UnityEngine.Random.Range(1, roverDriving.totalGoals + 1);
            uniqueAnswers.Add(randomValue);
        }
        int index = 1;
        foreach (int value in uniqueAnswers)
        {
            if (value != goalCount)
            {
                answers[index] = $"{value:F0}";
                index++;
            }
        }
        if (goalCount > 0)
        {
            answers[1] = "0";  //Always have a "0" answer
        }
        return answers;

    }
    string[] FillCurrentScoreAnswers()
    {
        string[] answers = new string[4];
        float actualScore = scoreDisplay.actualScore;
        // Ensure the correct answer is added first
        answers[0] = $"{actualScore:F0}";

        // Create a HashSet to avoid duplicate random numbers
        HashSet<float> uniqueAnswers = new HashSet<float> { actualScore };

        float minValue;
        float maxValue;
        float range;
        // Define the range for the random values
        if (actualScore > 50)
        {
            minValue = actualScore - actualScore * 1.5f;
            maxValue = actualScore + actualScore * 1.5f;
            range = maxValue - minValue;
        }
        else if (actualScore < -50)
        {
            minValue = actualScore + actualScore * 1.5f;
            maxValue = actualScore - actualScore * 1.5f;
            range = maxValue - minValue;
        }
        else
        {
            minValue = -50;
            maxValue = 50;
            range = 100;
        }

        Debug.Log("FillCurrentScoreAnswers range is: " + range + " minValue: " + minValue + " maxValue: " + maxValue);

        // Calculate the quartile ranges
        float quartileSize = range / 4;
        float[] quartileRanges = new float[4];
        for (int i = 0; i < 4; i++)
        {
            quartileRanges[i] = minValue + quartileSize * i;
        }

        // Determine the quartile of the actual score
        int actualScoreQuartile = (int)((actualScore - minValue) / quartileSize);

        HashSet<int> quartilesFilled = new HashSet<int> { actualScoreQuartile };
        while (quartilesFilled.Count < 4)
        {
            int randomQuartile;
            do
            {
                randomQuartile = UnityEngine.Random.Range(0, 4);
            } while (quartilesFilled.Contains(randomQuartile));

            quartilesFilled.Add(randomQuartile);
            float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

            if (!uniqueAnswers.Contains(randomValue))
            {
                uniqueAnswers.Add(randomValue);
                Debug.Log("CurrentScore random value is " + randomValue + "  real value: " + actualScore);

            }
        }

        //// Generate three random, unique numbers in different quartiles
        //while (uniqueAnswers.Count < 4)
        //{
        //    int randomQuartile;
        //    do
        //    {
        //        randomQuartile = UnityEngine.Random.Range(0, 4);
        //    } while (randomQuartile == actualScoreQuartile);

        //    float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));
        //    Debug.Log("CurrentScoreAnswers random value is " + randomValue + "  real value: " + actualScore);
        //    if (!uniqueAnswers.Contains(randomValue) && Mathf.Abs(randomValue-actualScore) > 10)
        //    {
        //        uniqueAnswers.Add(randomValue);
        //    }
        //}

        // Convert the unique numbers into the answers array, skipping the first correct answer
        int index = 1;
        foreach (float value in uniqueAnswers)
        {
            if (value != actualScore)
            {
                answers[index] = $"{value:F0}";
                index++;
            }
        }

        return answers;
    }

    // Level 2 answers
    string[] FillHazardStatusAnswers()
    {
        string[] answers = new string[4];
        bool isAtRisk = false;
        int[] textboxStatuses = textboxManager.textboxesStatus;
        for (int i = 0; i < textboxStatuses.Length; i++)
        {
            if (textboxStatuses[i] == 1 || textboxStatuses[i] == 2 || roverDriving.isInHazard)
            {
                isAtRisk = true;
            }
        }
        if (isAtRisk)
        {
            answers[0] = "Yes";
            answers[1] = "No";
        }
        else
        {
            answers[0] = "No";
            answers[1] = "Yes";
        }

        answers[2] = "Unknown";
        answers[3] = "Unknown";
        return answers;
    }
    string[] FillSciAbilityAnswers()
    {
        string[] answers = new string[4];
        int[] textboxStatuses = textboxManager.textboxesStatus;
        if (textboxStatuses[11] == 2 || textboxStatuses[12] == 2 || textboxStatuses[13] == 2 || textboxStatuses[14] == 2)
        {
            answers[0] = "No";
            answers[1] = "Yes";

        }
        else
        {
            answers[0] = "Yes";
            answers[1] = "No";
        }
        answers[2] = "Unknown";
        answers[3] = "Unknown";
        return answers;
    }
    string[] FillDriveAbilityAnswers()
    {
        string[] answers = new string[4];
        int[] textboxStatuses = textboxManager.textboxesStatus;
        for (int i = 0; i < 11; i++)
        {
            if (textboxStatuses[i] == 2)
            {
                answers[0] = "No";
                answers[1] = "Yes";

            }
            else
            {
                answers[0] = "Yes";
                answers[1] = "No";
            }
        }
        answers[2] = "Unknown";
        answers[3] = "Unknown";
        return answers;
    }
    string[] FillBatt2FinishAnswers(int targetIndex)
    {
        string[] answers = new string[4];
        float battPercent = batterySliderCtrl.batteryLevel;
        float drainRate = batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier;
        float batt2finish = battPercent;

        // Add if statement incase route no route generated yet
     
        if (roverDriving.roverPath.Count > 0)
        {
            List<Node_mouse> remainingPath = roverDriving.roverPath.Skip(targetIndex).ToList();
            //Debug.Log("Remaining nodes on path " +  remainingPath.Count);
            float remainingTime = roverDriving.CalculateRouteTime(remainingPath, roverDriving.speed);
            //Debug.Log("Remaining time is " +  remainingTime);
            batt2finish = battPercent - remainingTime * drainRate;
            Debug.Log("Batt2Finish: Expected battery at finish is " + batt2finish);
        }



        if (batt2finish > 10)
        {
            answers[0] = "Yes";
            answers[1] = "No";
        }
        else
        {
            answers[0] = "No";
            answers[1] = "Yes";
        }
        answers[2] = "Unknown";
        answers[3] = "Unknown";
        return answers;
    }
    string[] FillRoverProgAnswers(int targetIndex)
    {
        string[] answers = new string[4];
        float completionPercent = 0f;
        if (roverDriving.roverPath.Count > 0)
        {
            List<Node_mouse> remainingPath = roverDriving.roverPath.Skip(targetIndex).ToList();
            Debug.Log("Remaining nodes on path " + remainingPath.Count + " total path count " + roverDriving.roverPath.Count + " targetIndex: " + targetIndex);
            completionPercent = (roverDriving.roverPath.Count - remainingPath.Count) * 100f / (roverDriving.roverPath.Count);
            Debug.Log("RoverProgress: Completion percent is " + completionPercent);
        }


        answers[0] = $"{completionPercent:F0}%";

        // Create a HashSet to avoid duplicate random numbers
        HashSet<float> uniqueAnswers = new HashSet<float> { completionPercent };

        // Generate three random, unique numbers between 0 and 100
        while (uniqueAnswers.Count < 4)
        {
            // Define the range for the random values
            float minValue = 0f;
            float maxValue = 100f;
            float range = maxValue - minValue;

            // Calculate the quartile ranges
            float quartileSize = range / 4;
            float[] quartileRanges = new float[4];
            for (int i = 0; i < 4; i++)
            {
                quartileRanges[i] = minValue + quartileSize * i;
            }

            // Determine the quartile of the completionPercent
            int completionPercentQuartile = (int)((completionPercent - minValue) / quartileSize);


            HashSet<int> quartilesFilled = new HashSet<int> { completionPercentQuartile };
            while (quartilesFilled.Count < 4)
            {
                int randomQuartile;
                do
                {
                    randomQuartile = UnityEngine.Random.Range(0, 4);
                } while (quartilesFilled.Contains(randomQuartile));

                quartilesFilled.Add(randomQuartile);
                float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

                if (!uniqueAnswers.Contains(randomValue))
                {
                    uniqueAnswers.Add(randomValue);
                    Debug.Log("Rover progress random value is " + randomValue + "  real value: " + completionPercent);

                }
            }
            // Generate a random, unique number in a different quartile
            //int randomQuartile;
            //do
            //{
            //    randomQuartile = UnityEngine.Random.Range(0, 4);
            //} while (randomQuartile == completionPercentQuartile);

            //float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));
            //if (!uniqueAnswers.Contains(randomValue))
            //{
            //    uniqueAnswers.Add(randomValue);
            //}
        }

        // Convert the unique numbers into the answers array, skipping the first correct answer
        int index = 1;
        foreach (float value in uniqueAnswers)
        {
            if (value != completionPercent)
            {
                //Debug.Log("Rover progress index " + index + " + value " + value);
                answers[index] = $"{value:F0}%";
                index++;
            }
        }
        return answers;
    }

    // Level 3 answers
    string[] FillPredScoreAnswers()
    {
        string[] answers = new string[4];
        float predictedScore = roverDriving.predictedScoreAtEndOfRoute;
        Debug.Log("Predicted score " + predictedScore);
        // Ensure the correct answer is added first
        answers[0] = $"{predictedScore:F0}";

        // Define the quartile sizes
        float quartileSize = 100f;   //100 "points"
        float quartileCount = 4f;   //4 quartiles

        // Pick random quartile for correct answer to fall within

        int correctQuartile = UnityEngine.Random.Range(0, 4);
        float minValue = predictedScore - (quartileSize * correctQuartile + 1);
        float maxValue = predictedScore + (quartileSize * (quartileCount - correctQuartile));

        // Define the range for the random values
        float range = maxValue - minValue;

        Debug.Log("Predicted score range is: " + range + " minValue: " + minValue + " maxValue: " + maxValue);

        // Calculate the quartile ranges  
        float EndQuartileSize = range / 4;
        float[] quartileRanges = new float[4];
        for (int i = 0; i < 4; i++)
        {
            quartileRanges[i] = minValue + EndQuartileSize * i;
        }

        // Determine the quartile of the correct answer  
        int correctAnswerQuartile = (int)((predictedScore - minValue) / EndQuartileSize);
        Debug.Log("Predicted score quartile is: " + correctAnswerQuartile);

        // Create a HashSet to avoid duplicate random numbers
        HashSet<float> uniqueAnswers = new HashSet<float> { predictedScore };

        while (uniqueAnswers.Count < 4)
        {

            // Generate a random, unique number in a different quartile  
            int randomQuartile;
            do
            {
                randomQuartile = UnityEngine.Random.Range(0, 4);
            } while (randomQuartile == correctAnswerQuartile);

            float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + EndQuartileSize));

            // Ensure the random value is at least 10 different from the correct answer  
            if (Mathf.Abs(randomValue - predictedScore) >= 10 && !uniqueAnswers.Contains(randomValue))
            {
                uniqueAnswers.Add(randomValue);
                Debug.Log("Predicted Score random value is " + randomValue + "  real value: " + predictedScore);
            }
        }

        // Convert the unique numbers into the answers array, skipping the first correct answer
        int index = 1;
        foreach (float value in uniqueAnswers)
        {
            if (value != predictedScore)
            {
                answers[index] = $"{value:F0}";
                index++;
            }
        }

        return answers;
    }
    string[] FillPredHazardAnswers(int targetIndex)
    {
        string[] answers = new string[4];
        float hazardTimePrediction = 0.0f;
        float hazardPercentRemaining = 0.0f;
        float remainingPathTime = 1.0f;
        if (roverDriving.roverPath.Count > 0)
        {
            List<Node_mouse> remainingPath = roverDriving.roverPath.Skip(targetIndex).ToList();
            remainingPathTime = roverDriving.CalculateRouteTime(remainingPath, roverDriving.speed);
            if (remainingPathTime == 0.0f)
            {
                remainingPathTime = 1.0f;
            }
            hazardTimePrediction = pathfinder.FindPredictedHazardTime(remainingPath);
            hazardPercentRemaining = hazardTimePrediction * 100f / remainingPathTime;
            Debug.Log($"Total path count is {roverDriving.roverPath.Count}, remaining path count is {remainingPath.Count}, remaining time is {remainingPathTime}, predicted hazard time is {hazardTimePrediction}, pred hazard percent is {hazardPercentRemaining}");
        }
        else
        {
            hazardPercentRemaining = 0.0f;
        }

        answers[0] = $"{hazardPercentRemaining:F0}";
        // Create a HashSet to avoid duplicate random numbers
        HashSet<float> uniqueAnswers = new HashSet<float> { hazardPercentRemaining };

        // Generate three random, unique numbers between 0 and 100 in quartiles that are separate from the true value and separate from each other
        while (uniqueAnswers.Count < 4)
        {
            // Define the range for the random values
            float minValue = 0f;
            float maxValue = 100f;
            float range = maxValue - minValue;

            // Calculate the quartile ranges
            float quartileSize = range / 4;
            float[] quartileRanges = new float[4];
            for (int i = 0; i < 4; i++)
            {
                quartileRanges[i] = minValue + quartileSize * i;
            }

            // Determine the quartile of the true value
            int trueValueQuartile = (int)((hazardPercentRemaining - minValue) / quartileSize);

            // Generate a random, unique number in a different quartile
            int randomQuartile;
            do
            {
                randomQuartile = UnityEngine.Random.Range(0, 4);
            } while (randomQuartile == trueValueQuartile || uniqueAnswers.Any(value => (int)((value - minValue) / quartileSize) == randomQuartile));

            float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));
            if (!uniqueAnswers.Contains(randomValue) && Mathf.Abs(randomValue - hazardPercentRemaining) > 10)
            {
                uniqueAnswers.Add(randomValue);
            }
        }
        

        // Convert the unique numbers into the answers array, skipping the first correct answer
        int index = 1;
        foreach (float value in uniqueAnswers)
        {
            if (value != hazardPercentRemaining)
            {
                answers[index] = $"{value:F0}";
                index++;
            }
        }
        return answers;
    }
    string[] FillTimeFinishAnswers(int targetIndex)
    {
        string[] answers = new string[4];
        float finishTimePrediction = 0.0f;
        if (roverDriving.roverPath.Count > 0)
        {
            List<Node_mouse> remainingPath = roverDriving.roverPath.Skip(targetIndex).ToList();
            finishTimePrediction = roverDriving.elapsedTimeForScore + roverDriving.CalculateRouteTime(remainingPath, roverDriving.speed); ;
        }
        else
        {
            finishTimePrediction = roverDriving.elapsedTimeForScore;
        }
        Debug.Log("Finish time predicted is " + finishTimePrediction);
        answers[0] = $"{finishTimePrediction:F0}";

        // Define the quartile sizes
        float quartileSize = 15f;   //15 seconds
        float quartileCount = 4f;   //4 quartiles

        // Pick random quartile for correct answer to fall within
        int correctQuartile = 0;
        float minValue = 0f;
        float maxValue = quartileSize * 4;

        // 3 cases if finish time is less than 15 seconds (unlikely), between 15 and 45 seconds, or greater than 45 seconds
        if (finishTimePrediction < quartileSize)
        {
            correctQuartile = 0;
        }
        else if (finishTimePrediction < quartileSize * 2)
        {
            correctQuartile = UnityEngine.Random.Range(0, 2);
        }
        else if (finishTimePrediction < quartileSize * 3)
        {
            correctQuartile = UnityEngine.Random.Range(0, 3);
        }
        else
        {
            correctQuartile = UnityEngine.Random.Range(0, 4);
            minValue = finishTimePrediction - (quartileSize * correctQuartile + 1);
            maxValue = finishTimePrediction + (quartileSize * (correctQuartile - quartileCount + 2));
        }
        Debug.Log("Correct quartile is " + correctQuartile + " min value is " + minValue + " max value is " + maxValue);

        // Define the range for the random values
        float range = maxValue - minValue;

        // Calculate the quartile ranges  
        float EndQuartileSize = range / 4;
        float[] quartileRanges = new float[4];
        for (int i = 0; i < 4; i++)
        {
            quartileRanges[i] = minValue + EndQuartileSize * i;
        }

        // Create a HashSet to avoid duplicate random numbers
        HashSet<float> uniqueAnswers = new HashSet<float> { finishTimePrediction };

        while (uniqueAnswers.Count < 4)
        {

            // Generate a random, unique number in a different quartile  
            int randomQuartile;
            do
            {
                randomQuartile = UnityEngine.Random.Range(0, 4);
            } while (randomQuartile == correctQuartile);

            float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + EndQuartileSize));

            // Ensure the random value is at least 10 different from the correct answer  
            if (Mathf.Abs(randomValue - finishTimePrediction) >= 10 && !uniqueAnswers.Contains(randomValue))
            {
                uniqueAnswers.Add(randomValue);
                Debug.Log("Predicted finish time random value is " + randomValue + "  real value: " + finishTimePrediction);
            }
        }

        //// Generate three random, unique numbers between 0 and 100
        //while (uniqueAnswers.Count < 4)
        //{
        //    //float randomValue = Mathf.Round(UnityEngine.Random.Range(finishTimePrediction- finishTimePrediction*1.5f, finishTimePrediction+ finishTimePrediction*1.5f)); // Rounded to 1 decimal place
        //    float randomValue = Mathf.Round(UnityEngine.Random.Range(gameClock.elapsedTime, finishTimePrediction + finishTimePrediction * 2f)); // Rounded to 1 decimal place

        //    // Make the difference at least 5 to help make it not just random guesses close to real value
        //    if (Mathf.Abs(randomValue - finishTimePrediction) >= 2 && !uniqueAnswers.Contains(randomValue))
        //    {
        //        uniqueAnswers.Add(randomValue);
        //    }
        //}

        // Convert the unique numbers into the answers array, skipping the first correct answer
        int index = 1;
        foreach (float value in uniqueAnswers)
        {
            if (value != finishTimePrediction)
            {
                answers[index] = $"{value:F0}";
                index++;
            }
        }
        return answers;
    }
    string[] FillBattFinishAnswers(int targetIndex)
    {
        string[] answers = new string[4];
        float battPercent = batterySliderCtrl.batteryLevel;
        Debug.Log("Batt percent is " + battPercent);
        float drainRate = batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier;
        Debug.Log("Drain rate is " + drainRate);
        float batt2finish = battPercent;
        // Add if statement incase route no route generated yet
        if (roverDriving.roverPath.Count > 0)
        {
            List<Node_mouse> remainingPath = roverDriving.roverPath.Skip(targetIndex).ToList();
            Debug.Log("Remaining nodes on path " + remainingPath.Count);
            float remainingTime = roverDriving.CalculateRouteTime(remainingPath, roverDriving.speed);
            Debug.Log("Remaining time is " + remainingTime);
            batt2finish = battPercent - remainingTime * drainRate;
            Debug.Log("Expected battery at finish is " + batt2finish);
        }

        answers[0] = $"{batt2finish:F0}%";

        // Create a HashSet to avoid duplicate random numbers
        HashSet<float> uniqueAnswers = new HashSet<float> { batt2finish };

        // Define the range for the random values  
        float minValue = 0f;


        // Calculate the quartile ranges  
        float quartileSize = 25f;
        float[] quartileRanges = new float[4];
        for (int i = 0; i < 4; i++)
        {
            quartileRanges[i] = minValue + quartileSize * i;
        }

        // Determine the quartile of the correct answer  
        int correctAnswerQuartile = (int)((batt2finish - minValue) / quartileSize);

        HashSet<int> quartilesFilled = new HashSet<int> { correctAnswerQuartile };
        while (quartilesFilled.Count < 4)
        {
            int randomQuartile;
            do
            {
                randomQuartile = UnityEngine.Random.Range(0, 4);
            } while (quartilesFilled.Contains(randomQuartile));

            quartilesFilled.Add(randomQuartile);
            float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

            if (!uniqueAnswers.Contains(randomValue) && Mathf.Abs(randomValue - batt2finish) > 10)
            {
                uniqueAnswers.Add(randomValue);
                Debug.Log("Predicted battery at finish random value is " + randomValue + "  real value: " + batt2finish);

            }
        }
        

        // Convert the unique numbers into the answers array, skipping the first correct answer
        int index = 1;
        foreach (float value in uniqueAnswers)
        {
            if (value != batt2finish)
            {
                Debug.Log("Rover batt2finish index " + index + " + value " + value);
                answers[index] = $"{value:F0}%";
                index++;
            }
        }
        return answers;
    }

    public void ShowQuestionPanel(string question1, string[] answers1, string question2, string[] answers2, string question3, string[] answers3)
    {
        questionPanel.SetActive(true);
        questionPanel.transform.SetAsLastSibling(); // Bring the panel to the front
        SAGATrunning = true;
        SAGATAcknowledgeButton.gameObject.SetActive(true);
        

        string logEntryStart = $"{Time.time}, SAGAT Popup Start";
        sagatLogEntries.Add(logEntryStart);

        questionText1.text = question1;
        correctAnswer1 = answers1[0];
        ShuffleArray(answers1);
        questionText2.text = question2;
        correctAnswer2 = answers2[0];
        ShuffleArray(answers2);
        questionText3.text = question3;
        correctAnswer3 = answers3[0];
        ShuffleArray(answers3);



        // Set button text for Question 1
        for (int i = 0; i < answerButtons1.Length; i++)
        {
            if (i < answers1.Length)
            {
                answerButtons1[i].gameObject.SetActive(true);
                answerButtons1[i].GetComponentInChildren<Text>().text = answers1[i];
                answerButtons1[i].image.color = defaultColor; // Reset color
            }
            else
            {
                answerButtons1[i].gameObject.SetActive(false);
            }
        }

        // Set button text for Question 2
        for (int i = 0; i < answerButtons2.Length; i++)
        {
            if (i < answers2.Length)
            {
                answerButtons2[i].gameObject.SetActive(true);
                answerButtons2[i].GetComponentInChildren<Text>().text = answers2[i];
                answerButtons2[i].image.color = defaultColor; // Reset color
            }
            else
            {
                answerButtons2[i].gameObject.SetActive(false);
            }
        }

        // Set button text for Question 3
        for (int i = 0; i < answerButtons3.Length; i++)
        {
            if (i < answers3.Length)
            {
                answerButtons3[i].gameObject.SetActive(true);
                answerButtons3[i].GetComponentInChildren<Text>().text = answers3[i];
                answerButtons3[i].image.color = defaultColor; // Reset color
            }
            else
            {
                answerButtons3[i].gameObject.SetActive(false);
            }
        }
    }
    void ShuffleArray(string[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            string temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
        //return array;
    }

    private void OnAnswerSelected1(Button selectedButton)
    {
        selectedAnswer1 = selectedButton.GetComponentInChildren<Text>().text;

        // Reset all button colors to default for Question 1
        foreach (Button button in answerButtons1)
        {
            button.image.color = defaultColor;
        }

        // Highlight the selected button
        selectedButton.image.color = selectedColor;
        Debug.Log("Selected Answer for Question 1: " + selectedAnswer1);
        string logEntryClick1 = $"{Time.time}, Question1 Click";
        sagatLogEntries.Add(logEntryClick1);
    }

    private void OnAnswerSelected2(Button selectedButton)
    {
        selectedAnswer2 = selectedButton.GetComponentInChildren<Text>().text;

        // Reset all button colors to default for Question 2
        foreach (Button button in answerButtons2)
        {
            button.image.color = defaultColor;
        }

        // Highlight the selected button
        selectedButton.image.color = selectedColor;
        Debug.Log("Selected Answer for Question 2: " + selectedAnswer2);
        string logEntryClick2 = $"{Time.time}, Question2 Click";
        sagatLogEntries.Add(logEntryClick2);
    }
    private void OnAnswerSelected3(Button selectedButton)
    {
        selectedAnswer3 = selectedButton.GetComponentInChildren<Text>().text;

        // Reset all button colors to default for Question 3
        foreach (Button button in answerButtons3)
        {
            button.image.color = defaultColor;
        }

        // Highlight the selected button
        selectedButton.image.color = selectedColor;
        Debug.Log("Selected Answer for Question 3: " + selectedAnswer3);
        string logEntryClick3 = $"{Time.time}, Question3 Click";
        sagatLogEntries.Add(logEntryClick3);
    }
    private void OnSubmit()
    {
        if (string.IsNullOrEmpty(selectedAnswer1) || string.IsNullOrEmpty(selectedAnswer2))
        {
            Debug.Log("Please select an answer for both questions before submitting.");
            return;
        }

        if (selectedAnswer1 == correctAnswer1) { isCorrect1 = true; }
        if (selectedAnswer2 == correctAnswer2) { isCorrect2 = true; }
        if (selectedAnswer3 == correctAnswer3) { isCorrect3 = true; }
        // Log answers and handle submission logic
        Debug.Log("Correct Answer1: " + correctAnswer1 + " | Correct Answer2: " + correctAnswer2 + " | Correct Answer3: " + correctAnswer3);
        Debug.Log("Submitted Answer1: " + selectedAnswer1 + " | Submitted Answer2: " + selectedAnswer2);
        Debug.Log("IsCorrect 1: " + isCorrect1 + " | IsCorrect 2: " + isCorrect2 + " | IsCorrect 3: " + isCorrect3);
        // Log the activation event
        string logEntry1 = $"{Time.time}, '{questionText1.text}', '{correctAnswer1}', {isCorrect1}";
        sagatLogEntries.Add(logEntry1);
        string logEntry2 = $"{Time.time}, '{questionText2.text}', '{correctAnswer2}', {isCorrect2}";
        sagatLogEntries.Add(logEntry2);
        string logEntry3 = $"{Time.time}, '{questionText3.text}', '{correctAnswer3}', {isCorrect3}";
        sagatLogEntries.Add(logEntry3);

        // Log the trust scale responses
        float trustSlider1value = trustSlider1.value;
        string logEntry4 = $"Trust, {trustSlider1value}";
        sagatTrustLogEntries.Add(logEntry4);
        float trustSlider2value = trustSlider2.value;
        string logEntry5 = $"Reliance, {trustSlider2value}";
        sagatTrustLogEntries.Add(logEntry5);

        // Hide the panel and resume the game
        questionPanel.SetActive(false);
        selectedAnswer1 = null;
        selectedAnswer2 = null;
        selectedAnswer3 = null;
        SAGATrunning = false;
        roverDriving.hasRunSAGAT = true; // Set the flag to true after running
    }

    public void OnAcknowledgeSAGAT()
    {
        string logEntrySAGATAcknolwedge = $"{Time.time}, SAGAT Acknowledge";
        sagatLogEntries.Add(logEntrySAGATAcknolwedge);
        SAGATAcknowledgeButton.gameObject.SetActive(false);
    }
    public void ExportLogToCSV(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Time, Question, Answer, Correct?"); // CSV header
                foreach (string entry in sagatLogEntries)
                {
                    writer.WriteLine(entry);
                }
            }
            Debug.Log($"SAGAT Answers exported to {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to write log to CSV: {ex.Message}");
        }
    }

    public void ExportTrustLogToCSV(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Question, Value"); // CSV header
                foreach (string entry in sagatTrustLogEntries)
                {
                    writer.WriteLine(entry);
                }
            }
            Debug.Log($"SAGAT Trust Answers exported to {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to write log to CSV: {ex.Message}");
        }
    }
}
