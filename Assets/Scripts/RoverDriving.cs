using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
//using Unity.VisualScripting.Antlr3.Runtime.Misc;


public class RoverDriving : MonoBehaviour
{
    //Modifiable factors
    public float lowHazardCost = 0.0f;
    public float defaultHazardCost = 2.0f;
    public float highHazardCost = 100.0f;
    public float speed = 100.0f;
    public float timeFactor = 5.0f;
    public float goalFactor = 100.0f;
    public float hazardFactor = 7.0f;
    public float minTimeStartSAGAT = 40f; //seconds to start
    public float maxTimeStopSAGAT = 100;  //max time to do the SAGAT (seconds)

    // Export logs to CSV
    string baseFilePath = "C:\\Users\\markw\\OneDrive\\Documents\\GitHub\\SPACE1_Results\\Demos\\";
    string userFolderPath;
    string trialFolderPath;

    // GitHub API variables
    private string githubToken = "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";
    private string repoOwner = "markwboyer";
    private string repoName = "SPACE1_Results";
    private string githubFilePath; // Path in repo

    // Display on display 5 in Rover scene
    public Canvas mouseCanvas;
    public Canvas SAGATCanvas;

    // Connections to LFANT (MATRIKS)
    public RoverManager roverManager;

    // Start menu
    public string userID;      // from start menu
    public string selectedConfig;   //from start menu
    public string trialNum;     //from start menu
    public string saveData;     //from start menu
    public GameObject[] configurationPanels;  // Panels/buttons for different configurations
    private bool routeViewEnabled = false;
    private bool contrastViewEnabled = false;
    private bool dedViewEnabled = false;
    private bool globalViewEnabled = false;
    private bool routeButtonLabels = false;

    // Logging
    public Button stopGameButton;   // stop game, export logs, and begin surveys
    private List<string> clickLog = new List<string>();
    public Button[] buttons;

    public BuildMap buildMap;
    public GameObject Rover1;
    public GameObject BuildMapGameObj;
    private Vector2[] goalLocations;
    private List<Goal> goalList;
    private List<Goal> unvisitedGoalList = new List<Goal>(); // List of goals yet to be visited
    private List<Goal> visitedGoalList = new List<Goal>();   // List of goals already visited

    // Caution panel
    public TextboxManager textboxManager;
    public BatterySliderCtrl batterySliderCtrl;
    private float batteryMovingUsageMultiplier = 3.8f;  //multiply this * drain rate * moving multiplier * routeTime = battery used on route
                                                      // found empirically with speed = 150, drain rate = 0.1, moving multiplier = 2
    public GameClockScript gameClock;

    // Pause button
    public Button roverPause;
    public Text buttonText;
    private bool isProcessingToggle = false;

    // Colors to change when paused and unpaused
    public Color pausedColor = Color.red;     // Color when paused
    public Color unpausedColor = Color.green; // Color when unpaused

    // Generate path button
    public Button pathGenButton;

    // SEE ALTERNATIVES button
    public Button generateAltsButton;
    public Button seeOptionsButton;
    private bool isProcessingContrastToggle = false;
    bool contrastDisplayIsActive = false;
    public Color unselectedColor = Color.gray;  // Color when route view disabled
    private int routeChosen = 0;

    public int extraHazardDistance = 2;
    public List<Node_mouse> lowCostRoverPath = new List<Node_mouse>();
    public List<Node_mouse> defaultCostRoverPath = new List<Node_mouse>();
    public List<Node_mouse> highCostRoverPath = new List<Node_mouse>();
    public GameObject linePrefab;
    public Color lowCostColor = Color.green;
    public Color mediumCostColor = Color.blue;
    public Color highCostColor = Color.red;
    public Color endLineColor = Color.white;
    private List<GameObject> lineObjects = new List<GameObject>();  // Store line objects

    // Global Goal text box
    public GlobalGoalText globalGoalText;
    public GameObject GlobalGoalTextPanel;
    public GameObject GlobalGoalTextHeader;
    public GameObject GlobalGoalTextWords;

    // Deductive Explanation text box
    public DedExplText dedExplText;
    public GameObject DedExplTextPanel;
    public GameObject DedExplTextHeader;
    public GameObject DedExplTextWords;

    // Score displays
    public ScoreDisplay scoreDisplay;

    // 3 buttons for zero cost, default cost, high cost
    public Button zeroCostButton;
    public Button defaultCostButton;
    public Button highCostButton;

    // Button to clear the paths. Clears displayed paths and actual route Lists
    public Button clearPathsButton;

    // Button to Confirm Route. Must be clicked before able to unpause the game
    public Button confirmRouteButton;
    public Color confirmButtonReadyColor = Color.yellow;
    public Color confirmButtonSelectedColor = Color.green;
    bool isConfirmRouteSelected = false;
    bool isRouteConfirmed = false;
    bool isRouteComplete = true;

    // Manual Route Creation
    public ManualRoute manualRoute;
    public Button ManualRouteButton;
    public Button CompleteManRteButton;
    public Button ClearManPathButton;

    private Vector3 targetPoint;
    //private int currentGoalIndex = 0;
    private float totalStepSize;
    private float stepIncrement;
    private int stepsPerNode = 3;

    // Astar call for path planning
    public AStarPathfinder pathfinder;
    public List<Node_mouse> roverPath = new List<Node_mouse>();        // Path rover will follow from AStar
    private int roverPathCount;         // Length of returned optimal path
    private int targetIndex;        // Index of current target node along the path - always increasing
    private int storedTargetIndex; // Index of target node at end of each path (updates upon route complete/new route)
    private Coroutine movementCoroutine; // To stop the current movement
    public LineRenderer lineRenderer;     // LineRenderer component to display the path

    //// FloydWarshall to find overall most efficient path
    //public FloydWarshall floydWarshall; //F-W algorithm implentation
    //private List<Vector3> optimalPath;
    //public List<Node> fullPath;        // Full path with optimal routing

    public int numGoals;
    public int totalGoals;
    private GameObject R1instantiated;
    public Rigidbody2D R1rigidbody;        // Rigidbody2D component of gameobject for Rover

    public Vector2 RoverStartPosition = new Vector3(0f, -95f, 0f);
    public bool isMoving = false;
    public float elapsedTime = 0f;
    private float ATScore;
    public bool isInHazard = false;
    public float finalBatteryLevel = 100f;
    public float elapsedTimeForScore = 0f;  // Total time in the game for AT Score

    private Vector2 closestGoal;  //Position of closest goal
    public bool isPaused = true;
    public int goalCount = 0;      //Count the number of goals reached
    private List<Tuple<int, float>> goalTimeLog = new List<Tuple<int, float>>(); // List to store goal index and the time it was reached
    private HashSet<int> loggedGoals = new HashSet<int>();// HashSet to keep track of which goals have been logged
    public List<int> goalNumbers = new List<int>();

    // Factors to compute AT score and move along path
    public float hazardTime = 0f;
    public float predictedScoreAtEndOfRoute = 0f;
    private float startTime;
    private Vector3 targetPosition;
    private int stepCounter = 0;

    public float hazardDistance; // Distance threshold for being in a hazard zone
    private Vector2[] hazardLocations; // List of hazard locations
    private float[] hazardScales;     // List of corresponding hazard scales

    // Calculate best possible AT Score for performance measurement
    public HashSet<float> bestPossibleATScore = new HashSet<float>();           // What the score would be if selecting optimal route with zero delays each time
    private HashSet<float> bestATScore_batteryRemaining = new HashSet<float>();  // How much battery would be left if executing perfect AT strategy
    public float idealATScore;
    public float idealBattRemaining = 100f;

    // No explanation randomization variable
    public int NoExplanationChoice = 0;  // 0 = zero cost, 1 = default cost, 2 = high cost route
    private bool NoExplanationReplan = false;  // Flag to indicate if first planning or replanning. False = first planning, true = replanning (add delay and "slot machine")
    public float cumulativeTimePenalties_NoExplanation = 0f; // Cumulative time penalties for the No Explanation condition

    // SAGAT implementation

    private float SAGATStartTime = 10f;     //time game will stop and survey will pop up - randomized in Start
    public SAGATPopupManager sagatManager;
    public bool hasRunSAGAT = false;
    private float routeConfirmedTime = 0f; // Time when the route was confirmed by the user
    private bool readyForSAGAT = false;

    // Post trial surveys
    public PostTrialSurveys postTrialSurveys;
    public string filePath;


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Rover")  // Wait until Rover scene is loaded
        {
            //Debug.Log("loaded Rover, now assigning camera");
            //StartCoroutine(AssignCameraWhenAvailable());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the stored user ID and configuration
        userID = PlayerPrefs.GetString("UserID", "DefaultUserID");
        selectedConfig = PlayerPrefs.GetString("SelectedConfig", "DefaultConfig");
        trialNum = PlayerPrefs.GetString("TrialNumber", "DefaultTrialNum");
        saveData = PlayerPrefs.GetString("SaveData", "false");
        // Use the userID and selectedConfig for whatever gameplay logic you need
        Debug.Log("Trial number: "+ trialNum);
        Debug.Log("UserID: " + userID);
        Debug.Log("Selected Config: " + selectedConfig);

        //////////////// Apply the configuration settings///////////////////// 
        ConfigureGameBasedOnSelection(selectedConfig);

        //R1instantiated = Instantiate(Rover1, RoverStartPosition, Quaternion.identity);
        //SetLayerRecursively(R1instantiated, LayerMask.NameToLayer("MOUSELayer"));
        //R1rigidbody = R1instantiated.GetComponent<Rigidbody2D>();

        // Get the BuildMap component from the BuildMap GameObject
        BuildMap buildMap = BuildMapGameObj.GetComponent<BuildMap>();
        numGoals = buildMap.NumGoals;  // ENSURE SET CORRECT NUMBER OF GOALS IN BUILDMAP
        totalGoals = numGoals;
        for (int i = 0; i < numGoals; i++) { goalNumbers.Add(i); }

        hazardLocations = buildMap.GetHazardLocations();
        hazardScales = buildMap.GetHazardScales();

        goalList = buildMap.GetGoalList();
        unvisitedGoalList = buildMap.GetGoalList();

        // Assign the listener to the button, to call TogglePause when the button is clicked
        roverPause.onClick.AddListener(TogglePause);
        
        // Set the initial button color to unpausedColor
        roverPause.GetComponent<Image>().color = pausedColor;
        // Set the initial text to "GO!"
        roverPause.GetComponentInChildren<Text>().text = "PUSH TO GO!";
        roverPause.gameObject.SetActive(false);

        // Add a listener for button clicks to the route generator
        //pathGenButton.onClick.AddListener(PathButtonClick);

        // Add a listener for button clicks on GENERATE ROUTES button
        generateAltsButton.onClick.AddListener(() => StartCoroutine(GenerateAltRoutesOnClick2()));

        // Add a listener for button clicks for SEE OPTIONS button
        seeOptionsButton.onClick.AddListener(ToggleContrastViews);

        // Add a listener for button clicks on FASTEST ROUTE button
        zeroCostButton.onClick.AddListener(SetRouteZeroCost);
        // Add a listener for button clicks on BALANCED ROUTE button
        defaultCostButton.onClick.AddListener(SetRouteDefaultCost);
        // Add a listener for button clicks on AVOIDANCE ROUTE button
        highCostButton.onClick.AddListener(SetRouteHighCost);

        // Add a listener for button clicks on CONFIRM ROUTE button
        confirmRouteButton.onClick.AddListener(ConfirmRoute);

        // Assign random colors to the buttons
        // Generate primary colors
        List<Color> primaryColors = new List<Color> { Color.red, Color.green, Color.blue };

        // Shuffle the colors
        ShuffleColors(primaryColors);

        // Assign colors to buttons
        SetButtonColor(zeroCostButton, primaryColors[0]);
        SetButtonColor(defaultCostButton, primaryColors[1]);
        SetButtonColor(highCostButton, primaryColors[2]);

        lowCostColor = primaryColors[0];
        mediumCostColor = primaryColors[1];
        highCostColor = primaryColors[2];

        // Get the current positions of the buttons
        Vector3 zeroCostPos = zeroCostButton.transform.position;
        Vector3 defaultCostPos = defaultCostButton.transform.position;
        Vector3 highCostPos = highCostButton.transform.position;

        //Debug.Log("Route button labels is " + routeButtonLabels);

        SetRouteButtonGoalText(routeButtonLabels);

        // Create a list of the y positions
        List<float> yPositions = new List<float> { zeroCostPos.y, defaultCostPos.y, highCostPos.y };

        // Shuffle the y positions
        ShuffleList(yPositions);

        // Assign the shuffled y positions back to the buttons
        zeroCostButton.transform.position = new Vector3(zeroCostPos.x, yPositions[0], zeroCostPos.z);
        defaultCostButton.transform.position = new Vector3(defaultCostPos.x, yPositions[1], defaultCostPos.z);
        highCostButton.transform.position = new Vector3(highCostPos.x, yPositions[2], highCostPos.z);

        // Add a listener for button clicks on CLEAR PATHS button
        clearPathsButton.onClick.AddListener(ClearPathsOnClick);

        ManualRouteButton.onClick.AddListener(manualRoute.StartManualRoute);
        CompleteManRteButton.onClick.AddListener(manualRoute.CompleteManualRoute);
        ClearManPathButton.onClick.AddListener(manualRoute.StartManualRoute);

        // Add a listener to end the game
        stopGameButton.onClick.AddListener(EndGameAndExportLogs);

        // Set the initial startPoint to the initial rover position
        transform.position = RoverStartPosition;

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => LogButtonClick(button.name));
        }

        SAGATStartTime = Mathf.Round(UnityEngine.Random.Range(minTimeStartSAGAT, maxTimeStopSAGAT));
        Debug.Log("SAGAT time will be " + SAGATStartTime);

        // Initialize targetIndex = 0 (which node the rover is going to next) in MoveAlongPath
        targetIndex = 0;
        storedTargetIndex = 0;

        //pathfinder.PrecomputePathsBetweenGoals(buildMap.goalLocations);
    }

    // Update is called once per frame
    void Update()
    {
        //if (isMoving && isConfirmRouteSelected && Time.time >= routeConfirmedTime+ Mathf.Round(UnityEngine.Random.Range(10 , 20)))
        //{
        //    readyForSAGAT = true;
        //}
        ////Debug.Log(Time.time);
        //if (!hasRunSAGAT && Time.time >= SAGATStartTime && readyForSAGAT) // Check the flag and time
        //{
        //    //Debug.Log(Time.time);
        //    //Time.timeScale = 0f; // Pause the game
        //    //storedTargetIndex = targetIndex; // Store the current target index
        //    Debug.Log("Running SAGAT at " + Time.time);
        //    if (isPaused == false) { TogglePause(); }
        //    Debug.Log("roverPath count is " + roverPath.Count + " targetIndex: " + targetIndex);
        //    runSAGAT(targetIndex - storedTargetIndex); // Call the SAGAT function. Nodes along path is targetIndex - storedTargetIndex
        //    hasRunSAGAT = true; // Set the flag to true after running
        //}

        //// Collect elapsed time while not driving
        //if (isPaused)
        //{
        //    elapsedTimeForScore += Time.deltaTime;
        //    //Debug.Log("Elapsed time NOT MOVING: " + elapsedTime);
        //    scoreDisplay.UpdateActualScore(goalCount, elapsedTimeForScore, hazardTime);
        //}

        //if (batterySliderCtrl.batteryLevel < 10)
        //{
        //    isMoving = false;
        //    roverPause.gameObject.SetActive(false);
        //    clearPathsButton.gameObject.SetActive(false);
        //    generateAltsButton.gameObject.SetActive(false);
        //}
    }
    public IEnumerator AssignCameraWhenAvailable()
    {
        Camera display5Cam = null;
        float timeout = 5f; // seconds
        float elapsed = 0f;

        while (display5Cam == null && elapsed < timeout)
        {
            display5Cam = GameObject.Find("SPACE_mouse_cam")?.GetComponent<Camera>();
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (display5Cam == null)
        {
            Debug.LogWarning("SPACE_mouse_cam not found within timeout. SAGAT and MOUSE canvases not assigned.");
            yield break; // or assign fallback/default camera if you want
        }

        // Assign the camera to both canvases
        mouseCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        mouseCanvas.worldCamera = display5Cam;
        mouseCanvas.planeDistance = 1;

        SAGATCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        SAGATCanvas.worldCamera = display5Cam;
        SAGATCanvas.planeDistance = 1;

        Debug.Log("Assigned SPACE_mouse_cam to MOUSE and SAGAT canvases.");
    }
    public void InitializeRoverDrivingScene()
    {
        R1instantiated = Instantiate(Rover1, RoverStartPosition, Quaternion.identity, buildMap.mouse2DWorldParent);
        SetLayerRecursively(R1instantiated, LayerMask.NameToLayer("MOUSELayer"));
        R1rigidbody = R1instantiated.GetComponent<Rigidbody2D>();
    }
    void ShuffleColors(List<Color> colors)
    {
        for (int i = 0; i < colors.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, colors.Count);
            // Swap the colors
            Color temp = colors[i];
            colors[i] = colors[randomIndex];
            colors[randomIndex] = temp;
        }
    }

    void SetRouteButtonGoalText(bool routeButtonLabels)
    {
        if (routeButtonLabels)
        {
            zeroCostButton.transform.GetChild(0).GetComponent<Text>().text = "FASTEST";
            defaultCostButton.transform.GetChild(0).GetComponent<Text>().text = "BALANCED";
            highCostButton.transform.GetChild(0).GetComponent<Text>().text = "AVOID";
        }
    }
    void SetButtonColor(Button button, Color color)
    {
        // Ensure the button has an Image component for its background
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
        else
        {
            Debug.LogWarning("Button does not have an Image component.");
        }
    }
    void ShuffleList(List<float> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            // Swap the values
            float temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void LogButtonClick(string buttonName)
    {
        float timeStamp = Time.time;
        string logEntry = $"{timeStamp:F2}, Button Clicked: {buttonName}";
        clickLog.Add(logEntry);

        //Debug.Log(logEntry); // Optional: log to the Unity console for testing
    }
    private void ConfigureGameBasedOnSelection(string selectedConfig)
    {
        generateAltsButton.gameObject.SetActive(false);
        seeOptionsButton.gameObject.SetActive(false);
        zeroCostButton.gameObject.SetActive(false);
        defaultCostButton.gameObject.SetActive(false);
        highCostButton.gameObject.SetActive(false);
        clearPathsButton.gameObject.SetActive(false);
        pathGenButton.gameObject.SetActive(false);   // NEW ROUTE button - default cost - should always be false (not used)
        confirmRouteButton.gameObject.SetActive(false);
        GlobalGoalTextPanel.SetActive(false);
        GlobalGoalTextHeader.SetActive(false);
        GlobalGoalTextWords.SetActive(false);
        DedExplTextPanel.SetActive(false);
        DedExplTextHeader.SetActive(false);
        DedExplTextWords.SetActive(false);
        zeroCostButton.GetComponent<Image>().color = unpausedColor;
        defaultCostButton.GetComponent<Image>().color = unpausedColor;
        highCostButton.GetComponent<Image>().color = unpausedColor;


        switch (selectedConfig)
        {
            case "Condition A":
                // Manual Path Planning only
                Debug.Log("Configuration A selected.");
                break;
            case "Condition B":
                // No Explanation condition
                routeViewEnabled = false;
                contrastViewEnabled = false;
                globalViewEnabled = false;
                dedViewEnabled = false;
                routeButtonLabels = false;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(false);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(false);
                GlobalGoalTextPanel.SetActive(false);
                GlobalGoalTextHeader.SetActive(false);
                GlobalGoalTextWords.SetActive(false);
                Debug.Log("Configuration B (NO EXPLANATION) selected.");
                NoExplanationChoice = UnityEngine.Random.Range(0, 2);  // initialize which random route to begin displaying in Condition B
                break;
            case "Condition C":
                // Global goal textbox only
                routeViewEnabled = false;
                contrastViewEnabled = false;
                globalViewEnabled = true;
                dedViewEnabled = false;
                routeButtonLabels = true;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(true);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(true);
                GlobalGoalTextPanel.SetActive(true);
                GlobalGoalTextHeader.SetActive(true);
                GlobalGoalTextWords.SetActive(true);
                Debug.Log("Configuration C (global only w/ final selection displayed) selected.");
                break;
            case "Condition D":
                // Contrastive explanation enabled. No global or deductive explanations
                routeViewEnabled = true;
                contrastViewEnabled = true;
                globalViewEnabled = false;
                dedViewEnabled = false;
                routeButtonLabels = false;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(true);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(true);
                Debug.Log("Configuration D (contrastive only) selected.");
                break;
            case "Condition E":
                // Deductive explanation only
                routeViewEnabled = false;
                contrastViewEnabled = false;
                globalViewEnabled = false;
                dedViewEnabled = true;
                routeButtonLabels = false;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(true);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(true);
                DedExplTextPanel.SetActive(true);
                DedExplTextHeader.SetActive(true);
                DedExplTextWords.SetActive(true);
                Debug.Log("Configuration E (deductive only) selected.");
                break;
            case "Condition F":
                // Global goal textbox plus contrastive view
                routeViewEnabled = true;
                contrastViewEnabled = true;
                globalViewEnabled = true;
                dedViewEnabled = false;
                routeButtonLabels = true;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(true);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(true);
                GlobalGoalTextPanel.SetActive(true);
                GlobalGoalTextHeader.SetActive(true);
                GlobalGoalTextWords.SetActive(true);
                Debug.Log("Configuration F (global + contrastive) selected.");
                break;
            case "Condition G":
                // Global goal textbox plus deductive view
                routeViewEnabled = false;
                contrastViewEnabled = false;
                globalViewEnabled = true;
                dedViewEnabled = true;
                routeButtonLabels = true;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(true);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(true);
                GlobalGoalTextPanel.SetActive(true);
                GlobalGoalTextHeader.SetActive(true);
                GlobalGoalTextWords.SetActive(true);
                DedExplTextPanel.SetActive(true);
                DedExplTextHeader.SetActive(true);
                DedExplTextWords.SetActive(true);
                Debug.Log("Configuration G (global + deductive) selected.");
                break;
            case "Condition H":
                // Contrastive view plus deductive view
                routeViewEnabled = true;
                contrastViewEnabled = true;
                globalViewEnabled = false;
                dedViewEnabled = true;
                routeButtonLabels = false;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(true);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(true);
                DedExplTextPanel.SetActive(true);
                DedExplTextHeader.SetActive(true);
                DedExplTextWords.SetActive(true);
                Debug.Log("Configuration H (Contrast + deductive) selected.");
                break;
            case "Condition I":
                // All explanations available to user
                routeViewEnabled = true;
                contrastViewEnabled = true;
                globalViewEnabled = true;
                dedViewEnabled = true;
                routeButtonLabels = false;
                generateAltsButton.gameObject.SetActive(true);
                seeOptionsButton.gameObject.SetActive(false);
                //zeroCostButton.gameObject.SetActive(true);
                //defaultCostButton.gameObject.SetActive(true);
                //highCostButton.gameObject.SetActive(true);
                clearPathsButton.gameObject.SetActive(true);
                GlobalGoalTextPanel.SetActive(true);
                GlobalGoalTextHeader.SetActive(true);
                GlobalGoalTextWords.SetActive(true);
                DedExplTextPanel.SetActive(true);
                DedExplTextHeader.SetActive(true);
                DedExplTextWords.SetActive(true);
                Debug.Log("Configuration I (ALL explanations) selected.");
                break;

            default:
                Debug.LogError("Unknown configuration selected.");
                break;
        }
    }

    // Toggle the pause state when the button is pressed
    public void TogglePause()
    {
        //Debug.Log("TogglePause pressed" + isProcessingToggle + " isPaused " + isPaused);
        if (isProcessingToggle) return; // Prevent re-entry
        isProcessingToggle = true;

        if (roverPath.Count == 0 || !isConfirmRouteSelected)
        {
            Debug.Log("No path to follow. Cannot unpause.");
            return;
        }
        isPaused = !isPaused;
        //storedTargetIndex = targetIndex; // Store the current target index

        //Debug.Log("roverdrivewarning " + textboxManager.roverDriveWarning);
        if (batterySliderCtrl.batteryLevel < 10)
        {
            isMoving = false;
            roverPause.gameObject.SetActive(false);
            clearPathsButton.gameObject.SetActive(false);
            generateAltsButton.gameObject.SetActive(false);
        }
        else if (isPaused || textboxManager.roverDriveWarning || !isConfirmRouteSelected)
        {
            roverPause.GetComponentInChildren<Text>().text = "PUSH TO GO!";
            roverPause.GetComponent<Image>().color = pausedColor;  // Set button to paused color
            isMoving = false;
            //Debug.Log("Paused or roverdrivewarning");
            clearPathsButton.gameObject.SetActive(true);
        }
        else
        {
            roverPause.GetComponentInChildren<Text>().text = "PUSH TO STOP";
            roverPause.GetComponent<Image>().color = unpausedColor;  // Set button to unpaused color
            isMoving = true;
            clearPathsButton.gameObject.SetActive(false);
            //Debug.Log("NOT Paused and NO roverdrivewarning");
        }

        // If unpaused and the rover is not moving, resume the coroutine
        //if (movementCoroutine == null && roverPath != null && roverPath.Count > 0)
        //{
        //    //Debug.Log("roverpath to start coroutine " +  roverPath.Count);
        //    movementCoroutine = StartCoroutine(MoveAlongPath());
        //}
        //Debug.Log("Paused state: " + isPaused + " || IsMoving: " + isMoving);

        StartMovingAlongPath();

        // Reset the processing flag after execution
        StartCoroutine(ResetToggleProcessing());
    }
    private IEnumerator ResetToggleProcessing()
    {
        yield return new WaitForSeconds(0.2f); // Add a small delay before allowing toggling again
        isProcessingToggle = false;
    }
    public void ToggleContrastViews()
    {
        if (isProcessingContrastToggle) return; // Prevent re-entry
        isProcessingContrastToggle = true;
        //Debug.Log("Toggle view button pressed. ContrastiveDisplayIsActive: " + contrastDisplayIsActive);
        if (contrastViewEnabled)
        {
            if (!contrastDisplayIsActive)
            {
                contrastDisplayIsActive = true;
                // Display the routes as colored lines
                DisplayAltRoute(lowCostRoverPath, lowCostColor, 0f);
                DisplayAltRoute(defaultCostRoverPath, mediumCostColor, 1.5f);
                DisplayAltRoute(highCostRoverPath, highCostColor, 3f);
                seeOptionsButton.GetComponentInChildren<Text>().text = "OPTIONS DISPLAYED";
                seeOptionsButton.GetComponent<Image>().color = unpausedColor;  // Set button to paused color
            }
            else
            {
                Debug.Log("Toggle view button pressed to turn off. ContrastiveDisplayIsActive: " + contrastDisplayIsActive);
                Debug.Log("Route chosen " + routeChosen + " lineObjects count: " + lineObjects.Count);
                if (routeChosen == 3) // High Cost route is chosen
                {
                    if (lineObjects.Count > 1)
                    {
                        Destroy(lineObjects[0]);
                        Destroy(lineObjects[1]);
                    }
                }
                else if (routeChosen == 2)  // Default cost route is chosen
                {
                    if (lineObjects.Count > 1)
                    {
                        Destroy(lineObjects[0]);
                        Destroy(lineObjects[2]);
                    }
                }
                else if (routeChosen == 1)  // Zero cost route chosen
                {
                    if (lineObjects.Count > 1)
                    {
                        Destroy(lineObjects[1]);
                        Destroy(lineObjects[2]);
                    }
                }
                else
                {
                    ClearLineRenderers();
                }
                if (lineObjects.Count > 4 && routeChosen > 0)
                {
                    for (int i = 1; i < lineObjects.Count; i++)
                    {
                        Destroy(lineObjects[i]);
                    }
                }
                contrastDisplayIsActive = false;
                seeOptionsButton.GetComponentInChildren<Text>().text = "SEE OPTIONS";
                seeOptionsButton.GetComponent<Image>().color = unselectedColor;  // Set button to paused color
            }

        }
        // Reset the processing flag after execution
        StartCoroutine(ResetToggleContrastProcessing());
    }

    private IEnumerator ResetToggleContrastProcessing()
    {
        yield return new WaitForSeconds(0.1f); // Add a small delay before allowing toggling again
        isProcessingContrastToggle = false;
    }

    public void StartMovingAlongPath()
    {
        if (roverPath == null || roverPath.Count == 0 || R1rigidbody == null)
        {
            Debug.LogError("Cannot start movement: missing path or rigidbody.");
            return;
        }

        hazardLocations = buildMap.GetHazardLocations();
        hazardScales = buildMap.GetHazardScales();
        startTime = Time.time;
        isMoving = true;
        //if (hasRunSAGAT || !isRouteComplete)
        //{
        //    targetIndex = storedTargetIndex; // Use the stored target index from SAGAT
        //    Debug.Log("Target index after SAGAT is " + targetIndex);
        //}
        //else
        //{
        //    targetIndex = 0; // Reset to the first target index
        //    Debug.Log("Reset targetIndex to: " + targetIndex);
        //}
        //elapsedTime = 0f;
        hazardTime = 0f;
        targetPosition = roverPath[targetIndex - storedTargetIndex].worldPosition;

        
    }



    public IEnumerator GenerateAltRoutesOnClick2()
    {
        // Log when participant starts to generate routes
        float timeStamp = Time.time;
        string logEntry = $"{timeStamp:F2}, Routes Generated";
        clickLog.Add(logEntry);
        //Debug.Log(logEntry);

        // Reset the target index for the MoveAlongPath routine back to zero when gen new paths
        isRouteComplete = false;
        

        // Condition B: No explanation condition
        int previousNoExplanationChoice = NoExplanationChoice;  // Store the previous choice
        while (NoExplanationChoice == previousNoExplanationChoice)  // Ensure a different choice is made
        {
            NoExplanationChoice = UnityEngine.Random.Range(0, 3);  // Choose which random route to begin displaying in Condition B
        }
        
        //Debug.Log("NoExplanationChoice is " + NoExplanationChoice);

        if (NoExplanationReplan && selectedConfig == "Condition B")   // After the first plan, now display the "slot machine" before displaying new options

        {
            Debug.Log("Displaying slot machine");
            HideRouteOptionButtons();

            // Wait for "slot machine" to finish
            yield return StartCoroutine(DisplayPathsTemporarily());
        }
        // Once clicked the first time, set NoExplanationReplan to true
        NoExplanationReplan = true;


        // Iterate over each line object in the list
        foreach (GameObject lineObjectToDestroy in lineObjects)
        {
            // Destroy the GameObject associated with the line renderer
            Destroy(lineObjectToDestroy);
        }
        //Debug.Log("LineObjects count second " + lineObjects.Count);
        if (lineObjects.Count > 0)
        {
            lineObjects.Clear();
        }
        //Debug.Log("LineObjects count third " + lineObjects.Count);

        // Assume your current position is the rover's current position
        Vector3 currentPosition = R1rigidbody.position;
        Debug.Log("Current position when SEE ALTS clicked is " + currentPosition);

        if (lowCostRoverPath.Count == 0 && selectedConfig != "Condition B")
        {
            // Start the coroutine and pass a callback to handle the result for 3x hazard costs
            yield return StartCoroutine(FindPathToAllGoalsCoroutine3(currentPosition, lowHazardCost, (fullPath_low) =>
            {
                if (fullPath_low != null)
                {
                    // Handle the fullPath here, for example, display it
                    lowCostRoverPath = fullPath_low;
                    //Debug.Log("Low cost path made with cost " + pathfinder.FindPathCost(fullPath_low, 0f));
                    CheckAndDisplayRoutes();
                    zeroCostButton.GetComponent<Image>().color = lowCostColor;
                }
                else
                {
                    Debug.LogError("Pathfinding failed or no path found.");
                }
            }));
        }
        if (defaultCostRoverPath.Count == 0 && selectedConfig != "Condition B")
        {

            yield return StartCoroutine(FindPathToAllGoalsCoroutine3(currentPosition, defaultHazardCost, (fullPath_default) =>
            {
                if (fullPath_default != null)
                {
                    // Handle the fullPath here, for example, display it
                    defaultCostRoverPath = fullPath_default;
                    //Debug.Log("default cost path made with cost "+ pathfinder.FindPathCost(fullPath_default, defaultHazardCost));
                    CheckAndDisplayRoutes();
                    defaultCostButton.GetComponent<Image>().color = mediumCostColor;
                }
                else
                {
                    Debug.LogError("Pathfinding failed or no path found.");
                }
            }));
        }
        if (highCostRoverPath.Count == 0 && selectedConfig != "Condition B")
        {
            yield return StartCoroutine(FindPathToAllGoalsCoroutine3(currentPosition, highHazardCost, (fullPath_high) =>
            {
                if (fullPath_high != null)
                {
                    // Handle the fullPath here, for example, display it
                    highCostRoverPath = fullPath_high;
                    //Debug.Log("high cost path made with cost " + pathfinder.FindPathCost(fullPath_high, highHazardCost));
                    CheckAndDisplayRoutes();
                    highCostButton.GetComponent<Image>().color = highCostColor;

                }
                else
                {
                    Debug.LogError("Pathfinding failed or no path found.");
                }
            }));
        }

        // If the selected configuration is "Condition B", generate paths based on the NoExplanationChoice
        if (selectedConfig == "Condition B")
        {
            if (lowCostRoverPath.Count == 0)
            {
                // Start the coroutine and pass a callback to handle the result for 3x hazard costs
                yield return StartCoroutine(FindPathToAllGoalsCoroutine3(currentPosition, lowHazardCost, (fullPath_low) =>
                {
                    if (fullPath_low != null)
                    {
                        // Handle the fullPath here, for example, display it
                        lowCostRoverPath = fullPath_low;
                        //Debug.Log("Low cost path made with cost " + pathfinder.FindPathCost(fullPath_low, 0f));
                        if (NoExplanationChoice == 0)
                        {
                            DisplayAltRoute(lowCostRoverPath, lowCostColor, 0f);
                            zeroCostButton.GetComponent<Image>().color = lowCostColor;
                            zeroCostButton.gameObject.SetActive(true);
                        }
                        
                    }
                    else
                    {
                        Debug.LogError("Pathfinding failed or no path found.");
                    }
                }));
            }
            


            if (defaultCostRoverPath.Count == 0)
            {
                yield return StartCoroutine(FindPathToAllGoalsCoroutine3(currentPosition, defaultHazardCost, (fullPath_default) =>
                {
                    if (fullPath_default != null)
                    {
                        // Handle the fullPath here, for example, display it
                        defaultCostRoverPath = fullPath_default;
                        //Debug.Log("default cost path made with cost "+ pathfinder.FindPathCost(fullPath_default, defaultHazardCost));
                        if (NoExplanationChoice == 1)
                        {
                            DisplayAltRoute(defaultCostRoverPath, mediumCostColor, 0f);
                            defaultCostButton.GetComponent<Image>().color = mediumCostColor;
                            defaultCostButton.gameObject.SetActive(true);
                        }
                        
                    }
                    else
                    {
                        Debug.LogError("Pathfinding failed or no path found.");
                    }
                }));
            }
            if (highCostRoverPath.Count == 0)
            {
                yield return StartCoroutine(FindPathToAllGoalsCoroutine3(currentPosition, highHazardCost, (fullPath_high) =>
                {
                    if (fullPath_high != null)
                    {
                        // Handle the fullPath here, for example, display it
                        highCostRoverPath = fullPath_high;
                        //Debug.Log("high cost path made with cost " + pathfinder.FindPathCost(fullPath_high, highHazardCost));
                        if (NoExplanationChoice ==2 )
                        {
                            DisplayAltRoute(highCostRoverPath, highCostColor, 0f);
                            highCostButton.GetComponent<Image>().color = highCostColor;
                            highCostButton.gameObject.SetActive(true);
                        }
                        highCostButton.GetComponent<Image>().color = highCostColor;
                    }
                    else
                    {
                        Debug.LogError("Pathfinding failed or no path found.");
                    }
                }));
            }

            if (NoExplanationReplan == true)  // Once replanning, now already have paths so need to just display the correct buttons/routes
            {
                if (NoExplanationChoice == 0)
                {
                    DisplayAltRoute(lowCostRoverPath, mediumCostColor, 0f);        // Use the same color to prevent knowing which route is which solely by color
                    zeroCostButton.GetComponent<Image>().color = mediumCostColor;
                    zeroCostButton.gameObject.SetActive(true);
                }
                else if (NoExplanationChoice == 1)
                {
                    DisplayAltRoute(defaultCostRoverPath, mediumCostColor, 0f);
                    defaultCostButton.GetComponent<Image>().color = mediumCostColor;
                    defaultCostButton.gameObject.SetActive(true);
                }
                else if (NoExplanationChoice == 2)
                {
                    DisplayAltRoute(highCostRoverPath, mediumCostColor, 0f);
                    highCostButton.GetComponent<Image>().color = mediumCostColor;
                    highCostButton.gameObject.SetActive(true);
                }
            }
        }
        

    }

    private IEnumerator DisplayPathsTemporarily()
    {
        float displayDuration = 0.1f; // Duration to display each path
        float totalDuration = 4f; // Total duration for displaying paths
        float slotMachineElapsedTime = 0f;
        cumulativeTimePenalties_NoExplanation += totalDuration;
        Debug.Log("Total time penalties for No Explanation condition is " + cumulativeTimePenalties_NoExplanation);

        // Temporarily combine all paths into a single dictionary
        // Create a list to store all dictionary entries
        List<KeyValuePair<(int, int), List<Node_mouse>>> combinedPaths = new List<KeyValuePair<(int, int), List<Node_mouse>>>();

        // Add entries from pathsBetweenGoals
        foreach (var entry in pathfinder.pathsBetweenGoals)
        {
            combinedPaths.Add(entry);
        }

        // Add entries from lowCostPathsBetweenGoals
        foreach (var entry in pathfinder.lowCostPathsBetweenGoals)
        {
            combinedPaths.Add(entry);
        }

        // Add entries from highCostPathsBetweenGoals
        foreach (var entry in pathfinder.highCostPathsBetweenGoals)
        {
            combinedPaths.Add(entry);
        }


        // Iterate through the paths in the dictionary
        while (slotMachineElapsedTime < totalDuration)
        {
            foreach (var pathEntry in combinedPaths)
            {
                     
                // Display the path
                DisplayPath(pathEntry.Value, Color.yellow);

                // Wait for the display duration
                yield return new WaitForSeconds(displayDuration);

                // Clear the displayed path
                ClearLineRenderers();

                // Increment elapsed time
                slotMachineElapsedTime += displayDuration;

                // Break if total duration is exceeded
                if (slotMachineElapsedTime >= totalDuration)
                {
                    break;
                }
            }
        }

        // Ensure all paths are cleared after the total duration
        ClearLineRenderers();
    }

    // Call this method to start the coroutine
    public void StartDisplayPathsTemporarily()
    {
        StartCoroutine(DisplayPathsTemporarily());
    }

    // Helper method to display routes once all paths are complete
    private void CheckAndDisplayRoutes()
    {

        //Debug.Log("Check and display route. ");
        if (lowCostRoverPath.Count > 0 && defaultCostRoverPath.Count > 0 && highCostRoverPath.Count > 0 )
        {

            if (contrastViewEnabled )
            {
                //Debug.Log("All routes generated, now displaying.");
                // Display the routes as colored lines
                DisplayAltRoute(lowCostRoverPath, lowCostColor, 0f);
                DisplayAltRoute(defaultCostRoverPath, mediumCostColor, 1.5f);
                DisplayAltRoute(highCostRoverPath, highCostColor, 3f);
                contrastDisplayIsActive = true;
                //ToggleContrastViews();
            }
             DisplayRouteOptionButtons();  
        }
        // Best possible score calculations
        //Debug.Log("Default cost rover path count is: " + defaultCostRoverPath.Count);
        //idealATScore = AddBestPossibleATScore(pathfinder.FindPredictedRouteScore(defaultCostRoverPath));
        //float goalTimePrediction = defaultCostRoverPath.Count / (.12f * speed);
        //float batteryRequired = goalTimePrediction * batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier * batteryMovingUsageMultiplier;
        //idealBattRemaining = AddBestPossibleBatteryUsage(batteryRequired);

        //Debug.Log("Best possible AT score is " + idealATScore + " with battery remaining " + idealBattRemaining);

    }

    private float AddBestPossibleATScore(float newATScore)
    {
        // Add the best possible AT score to the list

        bestPossibleATScore.Add(Mathf.Round(newATScore*100f) / 100f);
        //Debug.Log("Best possible AT score is " + bestPossibleATScore);
        return bestPossibleATScore.Sum();
    }
    private float AddBestPossibleBatteryUsage(float batteryUsed)
    {
        Debug.Log("Ideal batt remaining before: " + (100- bestATScore_batteryRemaining.Sum()));

        // Add the battery remaining to the hash set
        bestATScore_batteryRemaining.Add(Mathf.Round(batteryUsed *100f) / 100f);
        Debug.Log("Ideal batt used is " + batteryUsed+ " battery remaining is: " + (100-bestATScore_batteryRemaining.Sum()));
        return 100f - bestATScore_batteryRemaining.Sum();
    }

    public void ConfirmRoute()
    {
        storedTargetIndex = targetIndex; // Store the current target index
        // Log when final route is chosen
        float timeStamp = Time.time;
        string logEntry = $"{timeStamp:F2}, Route Confirmed";
        //Debug.Log(logEntry);
        clickLog.Add(logEntry);
        confirmRouteButton.gameObject.GetComponent<Image>().color = confirmButtonSelectedColor;
        confirmRouteButton.GetComponentInChildren<Text>().text = "ROUTE CONFIRMED";
        isConfirmRouteSelected = true;
        HideRouteOptionButtons();
        predictedScoreAtEndOfRoute = scoreDisplay.actualScore + scoreDisplay.predictedScore;
        Debug.Log("Predicted score at end of route: " + predictedScoreAtEndOfRoute);
        routeConfirmedTime = Time.time;
        isRouteConfirmed = true;
        roverPause.gameObject.SetActive(true);
        seeOptionsButton.gameObject.SetActive(false);
        generateAltsButton.gameObject.SetActive(false);
        clearPathsButton.gameObject.SetActive(true);

        idealATScore = AddBestPossibleATScore(pathfinder.FindPredictedRouteScore(defaultCostRoverPath));
        float goalTimePrediction = CalculateRouteTime(defaultCostRoverPath, speed);
        float batteryRequired = CalculateRouteBattery(goalTimePrediction);
        idealBattRemaining = AddBestPossibleBatteryUsage(batteryRequired);

        stepsPerNode = Mathf.RoundToInt((CalculateRouteDistance(roverPath) / roverPath.Count()) / (speed * Time.fixedDeltaTime));
        //Debug.Log("Steps per node is " + stepsPerNode);
        Debug.Log("Best possible AT score is " + idealATScore + " with battery remaining " + idealBattRemaining);
        Debug.Log($"Confirmed route at time: {elapsedTimeForScore} with idle time {elapsedTimeForScore - elapsedTime}");

    }

    public void HideRouteOptionButtons()
    {
        zeroCostButton.gameObject.SetActive(false);
        defaultCostButton.gameObject.SetActive(false);
        highCostButton.gameObject.SetActive(false);
        clearPathsButton.gameObject.SetActive(false);
    }

    public void DisplayRouteOptionButtons()
    {

            zeroCostButton.gameObject.SetActive(true);
            defaultCostButton.gameObject.SetActive(true);
            highCostButton.gameObject.SetActive(true);
      

    }

    public void DisplayConfirmRouteButton()
    {
        confirmRouteButton.gameObject.SetActive(true);
        confirmRouteButton.gameObject.GetComponent<Image>().color = confirmButtonReadyColor;
        confirmRouteButton.GetComponentInChildren<Text>().text = "CONFIRM ROUTE";
        isConfirmRouteSelected = false;
    }

    public void SetRouteZeroCost()  // For the fastest route where hazard cost is zero
    {
        roverPath = lowCostRoverPath;
        //Debug.Log("Remaining lineObjects:::" + lineObjects.Count);
        // Destroy all line objects
        foreach (var lineObject in lineObjects)
        {
            Destroy(lineObject);
        }
        lineObjects.Clear();
        //Debug.Log("Remaining lineObjects:::" + lineObjects.Count);

        // Removes the other lines
        //if (lineObjects.Count > 1)
        //{
        //    Destroy(lineObjects[1]);
        //    Destroy(lineObjects[2]);
        //    //Debug.Log("Remaining lineObjects" + lineObjects.Count);
        //}
        //if (lineObjects.Count > 3)
        //{
        //    Destroy(lineObjects[lineObjects.Count - 1]);
        //}
        float scorePrediction = pathfinder.FindPredictedRouteScore(roverPath);
        float hazardTimePrediction = pathfinder.FindPredictedHazardTime(roverPath);
        float goalTimePrediction = CalculateRouteTime(roverPath, speed);
        float batteryRequired = CalculateRouteBattery(goalTimePrediction);
        routeChosen = 1;

        OnRouteChosen(routeChosen, hazardTimePrediction, goalTimePrediction, scorePrediction, roverPath, batteryRequired);

        DisplayConfirmRouteButton();
    }
    public void SetRouteDefaultCost()    // For the optimal route where hazard costs are balanced vs. time
    {
        roverPath = defaultCostRoverPath;
        //Debug.Log("Remaining lineObjects:::" + lineObjects.Count);
        // Destroy all line objects
        foreach (var lineObject in lineObjects)
        {
            Destroy(lineObject);
        }
        lineObjects.Clear();
        //Debug.Log("Remaining lineObjects:::" + lineObjects.Count);
        // Removes the other lines
        //if (lineObjects.Count > 1)
        //{
        //    Destroy(lineObjects[0]);
        //    Destroy(lineObjects[2]);
        //    //Debug.Log("Remaining lineObjects" + lineObjects.Count);
        //}
        //if (lineObjects.Count > 3)
        //{
        //    Destroy(lineObjects[lineObjects.Count - 1]);
        //}
        float scorePrediction = pathfinder.FindPredictedRouteScore(roverPath);
        float hazardTimePrediction = pathfinder.FindPredictedHazardTime(roverPath);
        float goalTimePrediction = CalculateRouteTime(roverPath, speed);
        float batteryRequired = CalculateRouteBattery(goalTimePrediction);
        routeChosen = 2;
        OnRouteChosen(routeChosen, hazardTimePrediction, goalTimePrediction, scorePrediction, roverPath, batteryRequired);
        DisplayConfirmRouteButton();
    }
    public void SetRouteHighCost()      //For the route where hazard costs are greatly increased
    {
        roverPath = highCostRoverPath;
        //Debug.Log("Remaining lineObjects:::" + lineObjects.Count);
        // Destroy all line objects
        foreach (var lineObject in lineObjects)
        {
            Destroy(lineObject);
        }
        lineObjects.Clear();
        //Debug.Log("Remaining lineObjects:::" + lineObjects.Count);
        // Removes the other lines
        //if (lineObjects.Count > 1)
        //{
        //    Destroy(lineObjects[0]);
        //    Destroy(lineObjects[1]);

        //    //Debug.Log("Remaining lineObjects" + lineObjects.Count);
        //}
        //if (lineObjects.Count > 3)
        //{
        //    Destroy(lineObjects[lineObjects.Count - 1]);
        //}
        float scorePrediction = pathfinder.FindPredictedRouteScore(roverPath);
        float hazardTimePrediction = pathfinder.FindPredictedHazardTime(roverPath);
        float goalTimePrediction = CalculateRouteTime(roverPath, speed);
        float batteryRequired = CalculateRouteBattery(goalTimePrediction);
        routeChosen = 3;
        OnRouteChosen(routeChosen, hazardTimePrediction, goalTimePrediction, scorePrediction, roverPath, batteryRequired);
        DisplayConfirmRouteButton();
    }
    void OnRouteChosen(int chosenRoute, float hazardTimePrediction, float goalTimePrediction, float scorePrediction, List<Node_mouse> chosenPath, float batteryRequired)
    {

        // Update the Global Goal Text with the chosen route
        if (globalViewEnabled)
        {
            globalGoalText.UpdateRouteInfo(chosenRoute);
        }


        // Update the deductive explanation text with the chosen route
        if (dedViewEnabled)
        {
            dedExplText.UpdateDedExplInfo(chosenRoute, hazardTimePrediction, goalTimePrediction, scorePrediction, batteryRequired);
        }

        // Update the predicted score UI
        scoreDisplay.SetPredictedScore(scorePrediction);


        if (routeViewEnabled)
        {
            if (chosenRoute == 1)
            {
                DisplayPath(chosenPath, lowCostColor);
                float timeStamp = Time.time;
                string logEntry = $"{timeStamp:F2}, Chosen Route: Fastest ,Predicted Score:, {scorePrediction}, Hazard Prediction Time, {hazardTimePrediction}, Predicted Route Time:, {goalTimePrediction}";
                clickLog.Add(logEntry);
            }
            else if (chosenRoute == 2)
            {
                DisplayPath(chosenPath, mediumCostColor);
                float timeStamp = Time.time;
                string logEntry = $"{timeStamp:F2}, Chosen Route: Optimal ,Predicted Score:, {scorePrediction}, Hazard Prediction Time, {hazardTimePrediction}, Predicted Route Time:, {goalTimePrediction}";
                clickLog.Add(logEntry);
            }
            else if (chosenRoute == 3)
            {
                DisplayPath(chosenPath, highCostColor);
                float timeStamp = Time.time;
                string logEntry = $"{timeStamp:F2}, Chosen Route: Avoidance ,Predicted Score:, {scorePrediction}, Hazard Prediction Time, {hazardTimePrediction}, Predicted Route Time:, {goalTimePrediction}";
                clickLog.Add(logEntry);
            }
            else
            {
                DisplayPath(chosenPath, mediumCostColor);
                float timeStamp = Time.time;
                string logEntry = $"{timeStamp:F2}, Chosen Route: Default ,Predicted Score:, {scorePrediction}, Hazard Prediction Time, {hazardTimePrediction}, Predicted Route Time:, {goalTimePrediction}";
                clickLog.Add(logEntry);
            }
        }
    }

    public void ClearPathsOnClick()
    {
        //Debug.Log("Clearing paths");
        ClearLineRenderers();
        lowCostRoverPath.Clear();
        defaultCostRoverPath.Clear();
        highCostRoverPath.Clear();
        roverPath.Clear();
        HideRouteOptionButtons();
        confirmRouteButton.gameObject.GetComponent<Image>().color = confirmButtonReadyColor;
        confirmRouteButton.gameObject.SetActive(false);
        roverPause.gameObject.SetActive(false);
        seeOptionsButton.gameObject.SetActive(false);
        generateAltsButton.gameObject.SetActive(true);

    }

    // Method to display the path as a line
    void DisplayAltRoute(List<Node_mouse> path, Color lineColor, float altPathOffset)
    {
        if (path == null || path.Count == 0) return;

        // Create a new LineRenderer object from the prefab
        GameObject lineObject = Instantiate(linePrefab, buildMap.mouse2DWorldParent);
        SetLayerRecursively(lineObject, LayerMask.NameToLayer("MOUSELayer"));


        // Store the instantiated line object for later removal
        lineObjects.Add(lineObject);

        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

        // Set the line color with transparency
        Color transparentColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0.8f); // 50% transparency
        lineRenderer.startColor = transparentColor;
        lineRenderer.endColor = transparentColor;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.alignment = LineAlignment.View; // always faces the camera


        // Set the number of positions based on the path length
        lineRenderer.positionCount = path.Count;

        // Set the positions of the line renderer with a slight offset
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 offsetPosition = path[i].worldPosition + new Vector3(altPathOffset, 0, -0.1f); // Slightly offset in the x-axis and z-axis
            lineRenderer.SetPosition(i, offsetPosition);
        }
    }

    // Method for using the "default route only" option. Not for use by participants, but for debugging path planner
    //public void PathButtonClick()
    //{
    //    // Assume your current position is the rover's current position
    //    Vector3 currentPosition = R1rigidbody.position;
    //    //Debug.Log("Current position when clicked is " + currentPosition);


    //    // Start the coroutine and pass a callback to handle the result
    //    StartCoroutine(FindPathToAllGoalsCoroutine(currentPosition, defaultHazardCost, (fullPath) =>
    //    {
    //        if (fullPath != null)
    //        {
    //            // Handle the fullPath here, for example, display it
    //            roverPath = fullPath;
    //            DisplayPath(roverPath, mediumCostColor);
    //        }
    //        else
    //        {
    //            Debug.LogError("Pathfinding failed or no path found.");
    //        }
    //    }));

    //}

    // Method to display the path visually
    void DisplayPath(List<Node_mouse> roverPath, Color lineColor)
    {
        if (roverPath == null || roverPath.Count == 0) return;
        //Debug.Log("RoverPath count to render is " + roverPath.Count);

        // Create a new LineRenderer object from the prefab
        GameObject lineObject = Instantiate(linePrefab, buildMap.mouse2DWorldParent);
        SetLayerRecursively(lineObject, LayerMask.NameToLayer("MOUSELayer"));

        // Store the instantiated line object for later removal
        lineObjects.Add(lineObject);
        //Debug.Log("LineObjects count " + lineObjects.Count);

        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.alignment = LineAlignment.View; // always faces the camera

        // Set the number of points in the LineRenderer
        lineRenderer.positionCount = roverPath.Count;
        //Debug.Log("Points on line renderer " + lineRenderer.positionCount);
        // Assign each path node's position to the LineRenderer
        for (int i = 0; i < roverPath.Count; i++)
        {
            //Debug.Log("Line rendered world pos = " + roverPath[i].worldPosition);
            lineRenderer.SetPosition(i, roverPath[i].worldPosition);
        }
        float scorePrediction = pathfinder.FindPredictedRouteScore(roverPath);
        float hazardTimePrediction = pathfinder.FindPredictedHazardTime(roverPath);
        float goalTimePrediction = CalculateRouteTime(roverPath, speed);
        //OnRouteChosen(2, hazardTimePrediction, goalTimePrediction, scorePrediction, roverPath);
    }
    //IEnumerator MoveAlongPath()
    //{
    //    //Debug.Log("isMoving " + isMoving);
    //    //Debug.Log("MoveAlongPath1");
    //    Vector2[] goalLocations = buildMap.goalLocations;
    //    if (roverPath == null || roverPath.Count == 0)
    //    {
    //        Debug.LogError("roverPath is not assigned or empty.");
    //        yield break; // Stop the coroutine if no path exists
    //    }
    //    //Debug.Log("MoveAlongPath2");
    //    if (R1rigidbody == null)
    //    {
    //        Debug.LogError("R1rigidbody is not assigned.");
    //        yield break; // Stop the coroutine if no rigidbody is found
    //    }
    //    //Debug.Log("MoveAlongPath3");
    //    if (goalLocations == null || goalLocations.Length == 0)
    //    {
    //        Debug.LogError("goalLocations array is null or empty.");
    //        //yield break; // Stop the coroutine if no goals are set
    //    }
    //    //targetIndex = 0;
    //    //Debug.Log("goalLocations length now: " + goalLocations.Length);
    //    // Initialize the hazardLocations by referencing the hazard locations from BuildMap or any other source.
    //    hazardLocations = buildMap.GetHazardLocations();
    //    hazardScales = buildMap.GetHazardScales();
    //    //Debug.Log("MoveAlongPath4");
    //    float startTime = Time.time;
    //    while (targetIndex < roverPath.Count && isMoving == true)
    //    {
    //        //Debug.Log("IsMoving2 " + isMoving);
    //        Vector3 targetPosition = roverPath[targetIndex].worldPosition;
    //        //Debug.Log("targetPosition index is : " + targetIndex);

    //        // Move the object towards the target position
    //        while (!isPaused && Vector3.Distance(R1rigidbody.position, targetPosition) > 0.1f)
    //        {
    //            //Debug.Log("MoveAlongPath5");
    //            //Debug.Log("Current position is : " + R1rigidbody.position);
    //            //Vector2 newPosition = Vector2.MoveTowards(R1rigidbody.position, targetPosition, speed * Time.deltaTime);  // move the rover at constant speed independent of frame rate
    //            //R1rigidbody.MovePosition(newPosition);

    //            float step = speed * Time.deltaTime;
    //            Vector2 newPosition = Vector2.MoveTowards(R1rigidbody.position, targetPosition, step);
    //            Debug.Log($"Moving {step:F3} units this frame at speed {speed}, deltaTime: {Time.deltaTime:F4}");
    //            R1rigidbody.MovePosition(newPosition);

    //            // Check if the rover is within a hazard zone
    //            isInHazard = false;

    //            for (int i = 0; i < hazardLocations.Length; i++)
    //            {

    //                if (Vector3.Distance(R1rigidbody.position, hazardLocations[i]) <= hazardScales[i] * 0.5f)
    //                {
    //                    //Debug.Log("HazDist = " + Vector3.Distance(R1rigidbody.position, hazardLocations[i]));
    //                    isInHazard = true;
    //                    //Debug.Log("IsInHazard");
    //                    break; // If we find a hazard within distance, we can stop checking
    //                }
    //            }
    //            //Debug.Log("IsInHazard " + isInHazard);
    //            // Accumulate hazard time if in a hazard zone
    //            if (isInHazard)
    //            {
    //                //Debug.Log("Adding hazard time " + hazardTime);
    //                hazardTime += Time.deltaTime; // Add time since last frame to hazardTime
    //            }
    //            elapsedTime += Time.deltaTime;
    //            scoreDisplay.UpdateActualScore(goalCount, elapsedTime, hazardTime);
    //            yield return null; // Wait for the next frame
    //        }
    //        // If the game is paused, yield until it is unpaused
    //        while (isPaused)
    //        {
    //            //Debug.Log("Yielding to paused state");

    //            yield return null; // This keeps the coroutine running but does nothing while paused
    //        }

    //        // If there's a warning that should stop the rover from moving, yield until that's cleared
    //        while (textboxManager.roverDriveWarning)
    //        {
    //            //Debug.Log("Yielding to roverDriveWarning");

    //            yield return null;
    //        }

    //        // Once the target node is reached, move to the next node
    //        if (!isPaused)
    //        {
    //            // Check if within 7 units of any goal location
    //            for (int i = 0; i < goalLocations.Length; i++)
    //            {
    //                if (Vector3.Distance(R1rigidbody.position, goalLocations[i]) <= 10.0f)
    //                {

    //                    // If the goal has not been logged yet, log the time and goal index
    //                    if (!loggedGoals.Contains(goalNumbers[i]))
    //                    {
    //                        goalCount++;
    //                        float timeReached = Time.time; // Get the current time
    //                        float ATscore = goalCount * goalFactor - timeReached * timeFactor - hazardTime * hazardFactor;
    //                        goalTimeLog.Add(new Tuple<int, float>(i, Time.time)); // Log goal index and time
    //                        loggedGoals.Add(goalNumbers[i]); // Mark this goal as logged
    //                        goalList[goalNumbers[i]].IsVisited = true; // Mark the goal as visited

    //                        Debug.Log($"Reached goal {goalNumbers[i]} at time: {timeReached}");
    //                        Debug.Log("AT Score is " + ATscore);
    //                        float timeStamp = Time.time;
    //                        string logEntry = $"{timeStamp:F2}, Goal number reached: {goalNumbers[i]},AT Score: {ATscore}";
    //                        clickLog.Add(logEntry);
    //                        //Debug.Log("Goal count is " + goalCount);

    //                        // Check if it's the final goal
    //                        if (goalCount == totalGoals)
    //                        {
    //                            float elapsedTime = Time.time - startTime;
    //                            Debug.Log("Final goal reached after: " + elapsedTime);
    //                            //Update the actual score display once final goal reached
    //                            scoreDisplay.UpdateActualScore(goalCount, elapsedTime, hazardTime);
    //                            // Pause the game
    //                            //Debug.Log("Final goal reached.");
    //                            TogglePause();

    //                            // Remove the "Route confirmed" button temporarily
    //                            confirmRouteButton.gameObject.SetActive(false);
    //                            roverPause.gameObject.SetActive(false);

    //                            if (movementCoroutine != null)
    //                            {
    //                                StopCoroutine(movementCoroutine);
    //                                movementCoroutine = null;
    //                            }
    //                            //Time.timeScale = 0f; // Pause the game by setting the timescale to 0

    //                            // Clear any LineRenderer objects
    //                            ClearLineRenderers();
    //                            lowCostRoverPath.Clear();
    //                            defaultCostRoverPath.Clear();
    //                            highCostRoverPath.Clear();
    //                            manualRoute.ClearLineRenderers();
    //                            Debug.Log("Reached goal and resetting paths low default hi " + lowCostRoverPath.Count + "," + defaultCostRoverPath.Count + "," + highCostRoverPath.Count + ",");
    //                            routeChosen = 0;

    //                            // Remove old goals
    //                            buildMap.HideAllGoals();

    //                            // Generate new goals
    //                            int newGoals = 2;

    //                            Vector2[] newGoalArray = buildMap.SpawnGoals(newGoals);  // Just make 2 new goals once arrived at final initial goal
    //                            pathfinder.PrecomputePathsBetweenGoals(newGoalArray, goalCount);
    //                            totalGoals = totalGoals + newGoals;
    //                            //Debug.Log("New totalGoals is " + totalGoals);
    //                            goalNumbers.Clear();
    //                            for (int j = 0; j < newGoals; j++)
    //                            {
    //                                goalNumbers.Add(j + loggedGoals.Count);
    //                                //Debug.Log("new goalnumbers are " + (j + loggedGoals.Count));
    //                            }
    //                            //Debug.Log("Goalnumbers new count is "+  goalNumbers.Count);
    //                            buildMap.goalLocations = newGoalArray;
    //                            //Debug.Log("buildmap goallocations is " + buildMap.goalLocations.Length);

    //                            //Reset targetIndex = 0 for MoveAlongPath to restart
    //                            targetIndex = 0;

    //                            // Reset the readyForSAGAT to false when no route is selected and confirmed
    //                            readyForSAGAT = false;

    //                            // Set Gen New Paths button to active
    //                            if (selectedConfig != "Condition A")
    //                            {
    //                                generateAltsButton.gameObject.SetActive(true);

    //                            }

    //                            NoExplanationReplan = false;  // Condition B No Explanation reset for first click

    //                            // Return the goalTimeLog and exit the coroutine
    //                            yield break;
    //                        }
    //                        //}
    //                    }

    //                }
    //            }
    //            targetIndex++;

    //        }

    //    }
    //    // Movement complete, reset coroutine handle
    //    movementCoroutine = null;
    //}


    void FixedUpdate()
    {
        if (isMoving && isConfirmRouteSelected && Time.time >= routeConfirmedTime + Mathf.Round(UnityEngine.Random.Range(10, 20)))
        {
            readyForSAGAT = true;
        }
        //Debug.Log(Time.time);
        if (!hasRunSAGAT && Time.time >= SAGATStartTime && readyForSAGAT) // Check the flag and time
        {
            //Debug.Log(Time.time);
            //Time.timeScale = 0f; // Pause the game
            //storedTargetIndex = targetIndex; // Store the current target index
            Debug.Log("Running SAGAT at " + Time.time);
            if (isPaused == false) { TogglePause(); }
            Debug.Log("roverPath count is " + roverPath.Count + " targetIndex: " + targetIndex);
            sagatManager.runSAGAT(targetIndex - storedTargetIndex); // Call the SAGAT function. Nodes along path is targetIndex - storedTargetIndex
            hasRunSAGAT = true;
        }

        if (hasRunSAGAT && sagatManager.SAGATrunning)
        {
            // Debug.Log("SAGAT is running, pausing rover movement and stopping the clocks.");
            return;
        }

        // Collect elapsed time while not driving
        if (isPaused)
        {
            elapsedTimeForScore += Time.deltaTime;
            //Debug.Log("Elapsed time NOT MOVING: " + elapsedTime);
            scoreDisplay.UpdateActualScore(goalCount, elapsedTimeForScore, hazardTime);
        }

        if (batterySliderCtrl.batteryLevel < 10)
        {
            isMoving = false;
            roverPause.gameObject.SetActive(false);
            clearPathsButton.gameObject.SetActive(false);
            generateAltsButton.gameObject.SetActive(false);
        }

        if (!isMoving || isPaused || textboxManager.roverDriveWarning || roverPath == null || R1rigidbody == null || targetIndex - storedTargetIndex >= roverPath.Count)
        {
            return;
        }

        if (Vector3.Distance(R1rigidbody.position, targetPosition) <= 0.1f || targetIndex == storedTargetIndex)
        {
            AdvanceToNextNodeOrGoal();
            return;
        }

        stepCounter++;
        float distanceToTarget = Vector2.Distance(R1rigidbody.position, targetPosition);
        float step = Mathf.Min(speed * Time.fixedDeltaTime, distanceToTarget); // don’t overshoot
        Vector2 newPosition = Vector2.MoveTowards(R1rigidbody.position, targetPosition, step);

        // Calculate the direction of movement
        Vector2 direction = ((Vector2)targetPosition - R1rigidbody.position).normalized;

        // Calculate the target angle in degrees
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

        // Smoothly interpolate the current angle toward the target angle
        float smoothedAngle = Mathf.LerpAngle(R1rigidbody.rotation, targetAngle, 0.1f); // Adjust 0.1f for smoothness

        // Apply the smoothed rotation
        R1rigidbody.rotation = smoothedAngle;

        // Move the rover
        R1rigidbody.MovePosition(newPosition);

        // Hazard detection
        isInHazard = false;
        for (int i = 0; i < hazardLocations.Length; i++)
        {
            if (Vector3.Distance(R1rigidbody.position, hazardLocations[i]) <= hazardScales[i] * 0.5f)
            {
                isInHazard = true;
                break;
            }
        }

        if (isInHazard)
        {
            hazardTime += Time.fixedDeltaTime;
        }

        elapsedTime += Time.fixedDeltaTime;

        
        //Debug.Log("Elapsed time moving is " + elapsedTime);

        elapsedTimeForScore += Time.fixedDeltaTime;
        //Debug.Log("Elapsed time for score is " + elapsedTimeForScore);
        scoreDisplay.UpdateActualScore(goalCount, elapsedTimeForScore, hazardTime);
    }

    void AdvanceToNextNodeOrGoal()
    {
        // Check goal proximity
        foreach (var goal in buildMap.goalLocations.Select((pos, i) => new { pos, i }))
        {
            if (Vector3.Distance(R1rigidbody.position, goal.pos) <= 5.0f && !loggedGoals.Contains(goalNumbers[goal.i]))
            {
                goalCount++;
                float timeReached = Time.time;
                float ATscore = goalCount * goalFactor - (elapsedTimeForScore - cumulativeTimePenalties_NoExplanation) * timeFactor - hazardTime * hazardFactor;
                goalTimeLog.Add(new Tuple<int, float>(goal.i, elapsedTimeForScore));
                loggedGoals.Add(goalNumbers[goal.i]);
                goalList[goalNumbers[goal.i]].IsVisited = true;

                Debug.Log($"Reached goal {goalNumbers[goal.i]} at time: {elapsedTimeForScore} with idle time {elapsedTimeForScore-elapsedTime} and moving time: {elapsedTime}");
                Debug.Log("AT Score is " + ATscore);
                clickLog.Add($"{timeReached:F2}, Goal number reached: {goalNumbers[goal.i]},AT Score: {ATscore}");

                if (goalCount == totalGoals)
                {
                    float totalElapsed = Time.time - startTime;
                    //Debug.Log("Final goal reached after: " + elapsedTimeForScore + " with number of actual steps: " + stepCounter);
                    scoreDisplay.UpdateActualScore(goalCount, elapsedTimeForScore, hazardTime);
                    TogglePause();

                    confirmRouteButton.gameObject.SetActive(false);
                    roverPause.gameObject.SetActive(false);
                    ClearLineRenderers();
                    lowCostRoverPath.Clear();
                    defaultCostRoverPath.Clear();
                    highCostRoverPath.Clear();
                    manualRoute.ClearLineRenderers();
                    routeChosen = 0;

                    buildMap.HideAllGoals();
                    int newGoals = 3; // *****This is where you can change number of new goals once final goal is reached
                    Vector2[] newGoalArray = buildMap.SpawnGoals(newGoals);                   
                    pathfinder.PrecomputePathsBetweenGoals(newGoalArray, goalCount);
                    totalGoals += newGoals;

                    goalNumbers.Clear();
                    for (int j = 0; j < newGoals; j++)
                    {
                        goalNumbers.Add(j + loggedGoals.Count);
                    }
                        

                    buildMap.goalLocations = newGoalArray;
                    readyForSAGAT = false;
                    if (selectedConfig != "Condition A")
                    {
                        generateAltsButton.gameObject.SetActive(true);

                    }

                    NoExplanationReplan = false;
                    isMoving = false;
                    isRouteComplete = true;
                    stepCounter = 0;
                    finalBatteryLevel = batterySliderCtrl.batteryLevel;
                    return;
                }
            }
        }

        targetIndex++;
        if ((targetIndex - storedTargetIndex) < roverPath.Count)
        {
            targetPosition = roverPath[targetIndex - storedTargetIndex].worldPosition;
            //if (targetIndex > 0)
            //{
            //    totalStepSize = Vector3.Distance(roverPath[targetIndex - 1].worldPosition, roverPath[targetIndex].worldPosition);  // Distance from current node to next node
            //    //Debug.Log("Total step size is " + totalStepSize);
            //    stepIncrement = totalStepSize / stepsPerNode;  // how far each fixedupdate should be
            //}
            //else
            //{
            //    totalStepSize = Vector3.Distance(R1rigidbody.position, roverPath[targetIndex].worldPosition);  // Distance from current node to next node
            //    Debug.Log("Total step size is " + totalStepSize);
            //    stepIncrement = totalStepSize / stepsPerNode;  // how far each fixedupdate should be
            //}
        }
        else
        {
            isMoving = false;
        }
    }


    public float CalculateRouteTime(List<Node_mouse> path, float speed)
    {
        if (path == null || path.Count < 2)
        {
            Debug.LogError("Path is null or too short to calculate time.");
            return 0f;
        }
        float timeInSeconds = 0f;
        int predictedTotalSteps = 0;
        int steps = 0;
        for (int i=1; i< path.Count; i++)
        {
            steps = Mathf.CeilToInt(Vector3.Distance(path[i - 1].worldPosition, path[i].worldPosition)/(speed*Time.fixedDeltaTime));
            //Debug.Log("steps per node is: " + steps);
            predictedTotalSteps += steps;
        }

        timeInSeconds = predictedTotalSteps * Time.fixedDeltaTime;


        //Debug.Log("Predicted time is: " + timeInSeconds + " total nodes is " + path.Count + " total steps: " + predictedTotalSteps);
        return timeInSeconds;
    }

    public float CalculateRouteBattery(float timeInSeconds)
    {
        if (timeInSeconds <= 0)
        {
            Debug.LogError("Time is zero or negative.");
            return 0f;
        }
        float batteryUsed = timeInSeconds * batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier;
        return batteryUsed;
    }

    // Method to calculate route distance
    public float CalculateRouteDistance(List<Node_mouse> path)
    {
        if (path == null || path.Count < 2)
        {
            Debug.LogError("Path is null or too short to calculate distance.");
            return 0f;
        }
        float totalDistance = 0f;
        for (int i = 1; i < path.Count; i++)
        {
            totalDistance += Vector3.Distance(path[i - 1].worldPosition, path[i].worldPosition);
        }
        return totalDistance;
    }
    private void ClearLineRenderers()
    {
        //Debug.Log("Clearing line renderers. Total objects: " + lineObjects.Count);
        foreach (var lineObject in lineObjects)
        {
            Destroy(lineObject); // Destroy each LineRenderer GameObject
        }
        //Debug.Log("Destroyed line renderers. Total objects: " + lineObjects.Count);
        lineObjects.Clear(); // Clear the list of LineRenderer objects
    }

    // Method to find path from current location to each goal and fill in the DEFAULT cost array and paths dictionary
    //public IEnumerator FindPathToAllGoalsCoroutine(Vector3 currentPosition, float hazardCost, System.Action<List<Node>> onComplete)
    //{
    //    //Debug.Log("Coroutine started");
    //    // Step 1: Get the goal locations from BuildMap
    //    Vector2[] goalLocations = buildMap.GetGoalLocations();
    //    //Debug.Log("FindPath goal locations length is " + goalLocations.Length);


    //    if (goalLocations == null || goalLocations.Length == 0)
    //    {
    //        Debug.LogError("No goal locations available.");
    //        onComplete(null); // Notify the caller of failure
    //        yield break; // Exit the coroutine
    //    }

    //    List<Node> fullPath = new List<Node>();
    //    for (int i = 0; i < goalLocations.Length; i++)
    //    {
    //        pathfinder.UpdateRoutesAndPaths(currentPosition, i);    // Update the routes and paths for all 3 hazardCosts
    //        yield return null;
    //    }

    //    fullPath = pathfinder.FindOptimalPath();
    //    Debug.Log("Default cost fullPath node count is " + fullPath.Count);
    //    onComplete(fullPath);
    //}

    // Method to find path from current location to each goal and fill in the cost array and paths dictionary
    public IEnumerator FindPathToAllGoalsCoroutine3(Vector3 currentPosition, float hazardCost, System.Action<List<Node_mouse>> onComplete)
    {
        if (goalList == null || goalList.Count == 0)
        {
            Debug.LogError("No goals available.");
            onComplete(null); // Notify the caller of failure
            yield break; // Exit the coroutine
        }

        List<Node_mouse> fullPath = new List<Node_mouse>();
        //Debug.Log("goalList count is " + goalList.Count);
        for (int i = 0; i < goalList.Count; i++)
        {
            // Check if the goal is already visited
            //Debug.Log("GoalList[" + i + "].IsVisited is " + goalList[i].IsVisited);
            if (goalList[i].IsVisited)
            {
                continue; // Skip this goal if it is already visited
            }

            //Debug.Log("GoalList[i].GoalNumber is " + goalList[i].GoalNumber);
            //pathfinder.UpdateAltRoutesAndPaths2(currentPosition, goalList[i].GoalNumber, goalList); // Update the starter paths and cost matrix for all 3 hazardCosts
            pathfinder.UpdateAltRoutesAndPaths3(currentPosition, goalList[i].GoalNumber, goalList); // Update the starter paths and cost matrix for all 3 hazardCosts
            yield return null;

        }

        List<Goal> unvisitedGoals = goalList.FindAll(g => !g.IsVisited);

        fullPath = pathfinder.FindOptimalPath4(hazardCost, unvisitedGoals);
        onComplete(fullPath);
    }


    // This is the original method to find the path to all goals and fill in the paths dictionary...DON'T CHANGE THIS!!
    //public IEnumerator FindPathToAllGoalsCoroutine2(Vector3 currentPosition, float hazardCost, System.Action<List<Node_mouse>> onComplete)
    //{
    //    //Debug.Log("Coroutine started");
    //    // Step 1: Get the goal locations from BuildMap
    //    Vector2[] goalLocations = buildMap.GetGoalLocations();
    //    //Debug.Log("FindPath goal locations length is " + goalLocations.Length);


    //    if (goalLocations == null || goalLocations.Length == 0)
    //    {
    //        Debug.LogError("No goal locations available.");
    //        onComplete(null); // Notify the caller of failure
    //        yield break; // Exit the coroutine
    //    }

    //    List<Node_mouse> fullPath = new List<Node_mouse>();
    //    for (int i = 0; i < goalLocations.Length; i++)
    //    {
    //        pathfinder.UpdateAltRoutesAndPaths(currentPosition, i);    // Update the starter paths and cost matrix for all 3 hazardCosts
    //        yield return null;
    //    }

    //    fullPath = pathfinder.FindOptimalPath2(hazardCost);
    //    //Debug.Log("fullPath node count is " + fullPath.Count);
    //    onComplete(fullPath);
    //}

    public void SetManualRoute()
    {
        roverPath = manualRoute.SavePath();
        Debug.Log("Manual rover path finished with " + roverPath.Count + " nodes.");
        float scorePrediction = pathfinder.FindPredictedRouteScore(roverPath);
        float hazardTimePrediction = pathfinder.FindPredictedHazardTime(roverPath);
        float goalTimePrediction = CalculateRouteTime(roverPath, speed);
        float batteryRequired = CalculateRouteBattery(goalTimePrediction);
        routeChosen = 4;
        OnRouteChosen(routeChosen, hazardTimePrediction, goalTimePrediction, scorePrediction, roverPath, batteryRequired);
        DisplayConfirmRouteButton();

        // Add Ideal Route calculations here to compare against
        Vector3 currentPosition = R1rigidbody.position;
        defaultCostRoverPath = IdealGoalPathFinder(currentPosition, defaultHazardCost, goalNumbers); // Find the ideal path to the new goals
        idealATScore = AddBestPossibleATScore(pathfinder.FindPredictedRouteScore(defaultCostRoverPath));
        float IdealgoalTimePrediction = CalculateRouteTime(defaultCostRoverPath, speed);
        float IdealbatteryRequired = CalculateRouteBattery(IdealgoalTimePrediction);
        idealBattRemaining = AddBestPossibleBatteryUsage(IdealbatteryRequired);

        Debug.Log("Best possible AT score is " + idealATScore + " with battery remaining " + idealBattRemaining);
    }


    public void ExportLogToCSV(string filePath, List<string> listToWrite)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false)) // 'false' to overwrite the file
            {
                writer.WriteLine("Time (seconds), Event");
                foreach (string logEntry in listToWrite)
                {
                    writer.WriteLine(logEntry);
                }
            }
            Debug.Log("Click Log exported successfully to " + filePath);
        }
        catch (IOException ex)
        {
            Debug.LogError("Error writing to file: " + ex.Message);
        }
    }
    //void runSAGAT(int localtargetIndex)
    //{
    //    //float battPercent = batterySliderCtrl.batteryLevel;
    //    //float drainRate = batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier;
    //    //List<Node> remainingPath = roverPath.Skip(targetIndex).ToList();
    //    //Debug.Log("Remaining nodes on path " + remainingPath.Count);
    //    //float remainingTime = remainingPath.Count / (speed);
    //    //Debug.Log("Remaining time is " + remainingTime);
    //    //float batt2finish = battPercent - remainingTime * drainRate;
    //    //Debug.Log("Expected battery at finish is " + batt2finish);

    //    string[] questionsLvl1 = {
    //        "What is the current battery percentage?",
    //        "What are the current warnings?",
    //        "What are the current cautions?",
    //        "How many goals has MOUSE visited so far?",
    //        "What is the current AT Score?"
    //    };

    //    string[] questionsLvl2 = {

    //        "Is MOUSE currently at risk? (In Hazard OR WARN/CAUTION present)",
    //        "Is MOUSE able to collect scientific data?",
    //        "Is MOUSE able to move?",
    //        "Does MOUSE have enough battery to complete the route?",
    //        "How far along the route is MOUSE?"
    //    };

    //    string[] questionsLvl3 = {
    //        "What is MOUSE's predicted score at the end of the current route?",
    //        "What percent of remaining route will MOUSE spend in hazard?",
    //        "What time will MOUSE finish the current route?",
    //        "How much battery will MOUSE finish the route with?"
    //    };

    //    string[] battAnswers = FillBattAnswers();
    //    string[] warningAnswers = FillWarningAnswers();
    //    string[] cautionAnswers = FillCautionAnswers();
    //    string[] goalsVisitedAnswers = FillGoalsVisitedAnswers();
    //    string[] currentScoreAnswers = FillCurrentScoreAnswers();

    //    string[] hazardStatusAnswers = FillHazardStatusAnswers();
    //    string[] sciAbilityAnswers = FillSciAbilityAnswers();
    //    string[] driveAbilityAnswers = FillDriveAbilityAnswers();
    //    string[] batteryToFinishAnswers = FillBatt2FinishAnswers(localtargetIndex);
    //    string[] roverProgressAnswers = FillRoverProgAnswers(localtargetIndex);

    //    //string[] predscoreAnswers = FillPredScoreAnswers();
    //    //string[] predHazardTimeAnswers = FillPredHazardAnswers(localtargetIndex);
    //    //string[] predTimeFinishAnswers = FillTimeFinishAnswers(localtargetIndex);
    //    //string[] battFinishAnswers = FillBattFinishAnswers(localtargetIndex);

    //    //string[] battAnswers = { "1", "2", "3", "4" };
    //    //string[] warningAnswers = { "1", "2", "3", "4" };
    //    //string[] cautionAnswers = { "1", "2", "3", "4" };
    //    //string[] goalsVisitedAnswers = { "1", "2", "3", "4" };
    //    //string[] currentScoreAnswers = { "1", "2", "3", "4" };

    //    //string[] hazardStatusAnswers = { "1", "2", "3", "4" };
    //    //string[] sciAbilityAnswers = { "1", "2", "3", "4" };
    //    //string[] driveAbilityAnswers = { "1", "2", "3", "4" };
    //    //string[] batteryToFinishAnswers = { "1", "2", "3", "4" };
    //    //string[] roverProgressAnswers = { "1", "2", "3", "4" };

    //    string[] predscoreAnswers = { "1", "2", "3", "4" };
    //    string[] predHazardTimeAnswers = { "1", "2", "3", "4" };
    //    string[] predTimeFinishAnswers = { "1", "2", "3", "4" };
    //    string[] battFinishAnswers = { "1", "2", "3", "4" };


    //    string[][] answersLvl1 = {
    //        battAnswers,
    //        warningAnswers,
    //        cautionAnswers,
    //        goalsVisitedAnswers,
    //        currentScoreAnswers
    //    };
    //    string[][] answersLvl2 = {
    //        hazardStatusAnswers,
    //        sciAbilityAnswers,
    //        driveAbilityAnswers,
    //        batteryToFinishAnswers,
    //        roverProgressAnswers
    //    };
    //    string[][] answersLvl3 = {
    //        predscoreAnswers,
    //        predHazardTimeAnswers,
    //        predTimeFinishAnswers,
    //        battFinishAnswers
    //    };

    //    // Pick a random question and display the panel
    //    int randomIndex1 = UnityEngine.Random.Range(0, questionsLvl1.Length);
    //    int randomIndex2 = UnityEngine.Random.Range(0, questionsLvl2.Length);
    //    int randomIndex3 = UnityEngine.Random.Range(0, questionsLvl3.Length);

    //    Debug.Log("Question 1: " + randomIndex1 + " question2 " + randomIndex2 + " question 3 " + randomIndex3);
    //    sagatManager.ShowQuestionPanel(questionsLvl1[randomIndex1], answersLvl1[randomIndex1], questionsLvl2[randomIndex2], answersLvl2[randomIndex2], questionsLvl3[randomIndex3], answersLvl3[randomIndex3]);
    //    //Time.timeScale = 1f;  // Gets game going again after questionnaires complete
    //}

    //// Level 1 answers
    //string[] FillBattAnswers()
    //{
    //    string[] answers = new string[4];
    //    float battPercent = batterySliderCtrl.batteryLevel;
    //    // Ensure the correct answer is added first
    //    answers[0] = $"{battPercent:F0}%";

    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<float> uniqueAnswers = new HashSet<float> { battPercent };

    //    // Generate three random, unique numbers between 0 and 100
    //    float minValue=0f;

    //    // Calculate the quartile ranges
    //    float quartileSize = 25f;
    //    float[] quartileRanges = new float[4];
    //    for (int i = 0; i < 4; i++)
    //    {
    //        quartileRanges[i] = minValue + quartileSize * i;
    //    }

    //    // Determine the quartile of the actual score
    //    int actualScoreQuartile = (int)((battPercent - minValue) / quartileSize);
    //    HashSet<int> quartilesFilled = new HashSet<int> { actualScoreQuartile };
    //    while (quartilesFilled.Count < 4)
    //    {
    //        int randomQuartile;
    //        do
    //        {
    //            randomQuartile = UnityEngine.Random.Range(0, 4);
    //        } while (quartilesFilled.Contains(randomQuartile));

    //        quartilesFilled.Add(randomQuartile);
    //        float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

    //        if (!uniqueAnswers.Contains(randomValue))
    //        {
    //            uniqueAnswers.Add(randomValue);
    //            Debug.Log("BattAnswers random value is " + randomValue + "  real value: " + battPercent);

    //        }
    //    }

    //    // Convert the unique numbers into the answers array, skipping the first correct answer
    //    int index = 1;
    //    foreach (float value in uniqueAnswers)
    //    {
    //        if (value != battPercent)
    //        {
    //            answers[index] = $"{value:F0}%";
    //            index++;
    //        }
    //    }

    //    // Shuffle the answers array to randomize the position of the correct answer
    //    //ShuffleArray(answers);
    //    return answers;
    //}
    //string[] FillWarningAnswers()
    //{
    //    string[] answers = new string[4];
    //    int[] textboxStatuses = textboxManager.textboxesStatus;
    //    // Find all indices where the value equals 2
    //    int[] twosIndices = textboxStatuses
    //        .Select((value, index) => new { value, index })
    //        .Where(item => item.value == 2)
    //        .Select(item => item.index)
    //        .ToArray();

    //    int selectedIndex = 0;
    //    if (twosIndices.Length == 0)
    //    {
    //        //Debug.Log("No element with value 2 found in the array.");
    //        answers[0] = "None";
    //        //return null;
    //    }
    //    else
    //    {
    //        // Randomly select one instance where the value equals 1
    //        selectedIndex = twosIndices[UnityEngine.Random.Range(0, twosIndices.Length)];
    //        answers[0] = $"{textboxManager.textboxes[selectedIndex].GetText()}";  // Set correct answer to 0 position
    //    }

    //    // Find all indices where the value equals 0 and exclude the chosen index
    //    int[] zerosIndices = textboxStatuses
    //        .Select((value, index) => new { value, index })
    //        .Where(item => item.value == 0 && item.index != selectedIndex)
    //        .Select(item => item.index)
    //        .ToArray();

    //    if (zerosIndices.Length < 3)
    //    {
    //        //Debug.LogError("Not enough elements with value 0 found in the array.");
    //        return null;
    //    }

    //    // Randomly select three indices for value 0
    //    zerosIndices = zerosIndices.OrderBy(x => UnityEngine.Random.value).ToArray();
    //    for (int i = 1; i <= 3; i++)
    //    {
    //        answers[i] = textboxManager.textboxes[zerosIndices[i - 1]].GetText();
    //    }

    //    return answers;


    //}
    //string[] FillCautionAnswers()
    //{
    //    string[] answers = new string[4];
    //    int[] textboxStatuses = textboxManager.textboxesStatus;
    //    // Find all indices where the value equals 1
    //    int[] onesIndices = textboxStatuses
    //        .Select((value, index) => new { value, index })
    //        .Where(item => item.value == 1)
    //        .Select(item => item.index)
    //        .ToArray();
    //    int selectedIndex = 0;
    //    if (onesIndices.Length == 0)
    //    {
    //        //Debug.Log("No element with value 1 found in the array.");
    //        answers[0] = "None";
    //        //return null;
    //    }
    //    else
    //    {
    //        // Randomly select one instance where the value equals 1
    //        selectedIndex = onesIndices[UnityEngine.Random.Range(0, onesIndices.Length)];
    //        answers[0] = $"{textboxManager.textboxes[selectedIndex].GetText()}";  // Set correct answer to 0 position
    //    }

    //    // Find all indices where the value equals 0 and exclude the chosen index
    //    int[] zerosIndices = textboxStatuses
    //        .Select((value, index) => new { value, index })
    //        .Where(item => item.value == 0 && item.index != selectedIndex)
    //        .Select(item => item.index)
    //        .ToArray();

    //    if (zerosIndices.Length < 3)
    //    {
    //        //Debug.LogError("Not enough elements with value 0 found in the array.");
    //        return null;
    //    }

    //    // Randomly select three indices for value 0
    //    zerosIndices = zerosIndices.OrderBy(x => UnityEngine.Random.value).ToArray();
    //    for (int i = 1; i <= 3; i++)
    //    {
    //        answers[i] = textboxManager.textboxes[zerosIndices[i - 1]].GetText();
    //    }

    //    return answers;


    //}
    //string[] FillGoalsVisitedAnswers()
    //{
    //    string[] answers = new string[4];
    //    answers[0] = $"{goalCount:F0}";
    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<int> uniqueAnswers = new HashSet<int> { goalCount };

    //    // Generate three random, unique numbers between -1000 and 1000
    //    while (uniqueAnswers.Count < 4)
    //    {
    //        int randomValue = UnityEngine.Random.Range(1, totalGoals+1);
    //        uniqueAnswers.Add(randomValue);
    //    }
    //    int index = 1;
    //    foreach (int value in uniqueAnswers)
    //    {
    //        if (value != goalCount)
    //        {
    //            answers[index] = $"{value:F0}";
    //            index++;
    //        }
    //    }
    //    if (goalCount > 0)
    //    {
    //        answers[1] = "0";  //Always have a "0" answer
    //    }
    //    return answers;

    //}
    //string[] FillCurrentScoreAnswers()
    //{
    //    string[] answers = new string[4];
    //    float actualScore = scoreDisplay.actualScore;
    //    // Ensure the correct answer is added first
    //    answers[0] = $"{actualScore:F0}";

    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<float> uniqueAnswers = new HashSet<float> { actualScore };

    //    float minValue;
    //    float maxValue;
    //    float range;
    //    // Define the range for the random values
    //    if (actualScore > 50)
    //    {
    //        minValue = actualScore - actualScore * 1.5f;
    //        maxValue = actualScore + actualScore * 1.5f;
    //        range = maxValue - minValue;
    //    }
    //    else if (actualScore < -50)
    //    {
    //        minValue = actualScore + actualScore * 1.5f;
    //        maxValue = actualScore - actualScore * 1.5f;
    //        range = maxValue - minValue;
    //    }
    //    else
    //    {
    //        minValue = -50;
    //        maxValue = 50;
    //        range = 100;
    //    }

    //    Debug.Log("FillCurrentScoreAnswers range is: " + range + " minValue: " + minValue + " maxValue: " + maxValue);

    //    // Calculate the quartile ranges
    //    float quartileSize = range / 4;
    //    float[] quartileRanges = new float[4];
    //    for (int i = 0; i < 4; i++)
    //    {
    //        quartileRanges[i] = minValue + quartileSize * i;
    //    }

    //    // Determine the quartile of the actual score
    //    int actualScoreQuartile = (int)((actualScore - minValue) / quartileSize);

    //    HashSet<int> quartilesFilled = new HashSet<int> { actualScoreQuartile };
    //    while (quartilesFilled.Count < 4)
    //    {
    //        int randomQuartile;
    //        do
    //        {
    //            randomQuartile = UnityEngine.Random.Range(0, 4);
    //        } while (quartilesFilled.Contains(randomQuartile));

    //        quartilesFilled.Add(randomQuartile);
    //        float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

    //        if (!uniqueAnswers.Contains(randomValue))
    //        {
    //            uniqueAnswers.Add(randomValue);
    //            Debug.Log("CurrentScore random value is " + randomValue + "  real value: " + actualScore);

    //        }
    //    }

    //    //// Generate three random, unique numbers in different quartiles
    //    //while (uniqueAnswers.Count < 4)
    //    //{
    //    //    int randomQuartile;
    //    //    do
    //    //    {
    //    //        randomQuartile = UnityEngine.Random.Range(0, 4);
    //    //    } while (randomQuartile == actualScoreQuartile);

    //    //    float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));
    //    //    Debug.Log("CurrentScoreAnswers random value is " + randomValue + "  real value: " + actualScore);
    //    //    if (!uniqueAnswers.Contains(randomValue) && Mathf.Abs(randomValue-actualScore) > 10)
    //    //    {
    //    //        uniqueAnswers.Add(randomValue);
    //    //    }
    //    //}

    //    // Convert the unique numbers into the answers array, skipping the first correct answer
    //    int index = 1;
    //    foreach (float value in uniqueAnswers)
    //    {
    //        if (value != actualScore)
    //        {
    //            answers[index] = $"{value:F0}";
    //            index++;
    //        }
    //    }

    //    return answers;
    //}

    //// Level 2 answers
    //string[] FillHazardStatusAnswers()
    //{
    //    string[] answers = new string[4];
    //    bool isAtRisk = false;
    //    int[] textboxStatuses = textboxManager.textboxesStatus;
    //    for (int i = 0; i < textboxStatuses.Length; i++)
    //    {
    //        if (textboxStatuses[i] == 1 || textboxStatuses[i] == 2 || isInHazard)
    //        {
    //            isAtRisk = true;
    //        }
    //    }
    //    if (isAtRisk)
    //    {
    //        answers[0] = "Yes";
    //        answers[1] = "No";
    //    }
    //    else
    //    {
    //        answers[0] = "No";
    //        answers[1] = "Yes";
    //    }

    //    answers[2] = "Unknown";
    //    answers[3] = "Unknown";
    //    return answers;
    //}
    //string[] FillSciAbilityAnswers()
    //{
    //    string[] answers = new string[4];
    //    int[] textboxStatuses = textboxManager.textboxesStatus;
    //    if (textboxStatuses[11] == 2 || textboxStatuses[12] == 2 || textboxStatuses[13] == 2 || textboxStatuses[14] == 2)
    //    {
    //        answers[0] = "No";
    //        answers[1] = "Yes";

    //    }
    //    else
    //    {
    //        answers[0] = "Yes";
    //        answers[1] = "No";
    //    }
    //    answers[2] = "Unknown";
    //    answers[3] = "Unknown";
    //    return answers;
    //}
    //string[] FillDriveAbilityAnswers()
    //{
    //    string[] answers = new string[4];
    //    int[] textboxStatuses = textboxManager.textboxesStatus;
    //    for (int i = 0; i < 11; i++)
    //    {
    //        if (textboxStatuses[i] == 2)
    //        {
    //            answers[0] = "No";
    //            answers[1] = "Yes";

    //        }
    //        else
    //        {
    //            answers[0] = "Yes";
    //            answers[1] = "No";
    //        }
    //    }
    //    answers[2] = "Unknown";
    //    answers[3] = "Unknown";
    //    return answers;
    //}
    //string[] FillBatt2FinishAnswers(int targetIndex)
    //{
    //    string[] answers = new string[4];
    //    float battPercent = batterySliderCtrl.batteryLevel;
    //    float drainRate = batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier;
    //    float batt2finish = battPercent;

    //    // Add if statement incase route no route generated yet
    //    if ( roverPath.Count > 0 ) 
    //    {
    //        List<Node_mouse> remainingPath = roverPath.Skip(targetIndex).ToList();
    //        //Debug.Log("Remaining nodes on path " +  remainingPath.Count);
    //        float remainingTime = CalculateRouteTime(remainingPath, speed);
    //        //Debug.Log("Remaining time is " +  remainingTime);
    //        batt2finish = battPercent - remainingTime * drainRate;
    //        Debug.Log("Batt2Finish: Expected battery at finish is " +  batt2finish);
    //    }



    //    if (batt2finish > 10)
    //    {
    //        answers[0] = "Yes";
    //        answers[1] = "No";
    //    }
    //    else
    //    {
    //        answers[0] = "No";
    //        answers[1] = "Yes";
    //    }
    //    answers[2] = "Unknown";
    //    answers[3] = "Unknown";
    //    return answers;
    //}
    //string[] FillRoverProgAnswers(int targetIndex)
    //{
    //    string[] answers = new string[4];
    //    float completionPercent = 0f;
    //    if (roverPath.Count > 0)
    //    {
    //        List<Node_mouse> remainingPath = roverPath.Skip(targetIndex).ToList();
    //        Debug.Log("Remaining nodes on path " + remainingPath.Count + " total path count " + roverPath.Count + " targetIndex: " + targetIndex);
    //        completionPercent = (roverPath.Count - remainingPath.Count) * 100f / (roverPath.Count);
    //        Debug.Log("RoverProgress: Completion percent is " + completionPercent);
    //    }


    //    answers[0] = $"{completionPercent:F0}%";

    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<float> uniqueAnswers = new HashSet<float> { completionPercent };

    //    // Generate three random, unique numbers between 0 and 100
    //    while (uniqueAnswers.Count < 4)
    //    {
    //        // Define the range for the random values
    //        float minValue = 0f;
    //        float maxValue = 100f;
    //        float range = maxValue - minValue;

    //        // Calculate the quartile ranges
    //        float quartileSize = range / 4;
    //        float[] quartileRanges = new float[4];
    //        for (int i = 0; i < 4; i++)
    //        {
    //            quartileRanges[i] = minValue + quartileSize * i;
    //        }

    //        // Determine the quartile of the completionPercent
    //        int completionPercentQuartile = (int)((completionPercent - minValue) / quartileSize);


    //        HashSet<int> quartilesFilled = new HashSet<int> { completionPercentQuartile };
    //        while (quartilesFilled.Count < 4)
    //        {
    //            int randomQuartile;
    //            do
    //            {
    //                randomQuartile = UnityEngine.Random.Range(0, 4);
    //            } while (quartilesFilled.Contains(randomQuartile));

    //            quartilesFilled.Add(randomQuartile);
    //            float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

    //            if (!uniqueAnswers.Contains(randomValue))
    //            {
    //                uniqueAnswers.Add(randomValue);
    //                Debug.Log("CurrentScore random value is " + randomValue + "  real value: " + completionPercent);

    //            }
    //        }
    //        // Generate a random, unique number in a different quartile
    //        //int randomQuartile;
    //        //do
    //        //{
    //        //    randomQuartile = UnityEngine.Random.Range(0, 4);
    //        //} while (randomQuartile == completionPercentQuartile);

    //        //float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));
    //        //if (!uniqueAnswers.Contains(randomValue))
    //        //{
    //        //    uniqueAnswers.Add(randomValue);
    //        //}
    //    }

    //    // Convert the unique numbers into the answers array, skipping the first correct answer
    //    int index = 1;
    //    foreach (float value in uniqueAnswers)
    //    {
    //        if (value != completionPercent)
    //        {
    //            //Debug.Log("Rover progress index " + index + " + value " + value);
    //            answers[index] = $"{value:F0}%";
    //            index++;
    //        }
    //    }
    //    return answers;
    //}

    //// Level 3 answers
    //string[] FillPredScoreAnswers()
    //{
    //    string[] answers = new string[4];
    //    float predictedScore = predictedScoreAtEndOfRoute;
    //    Debug.Log("Predicted score " + predictedScore);
    //    // Ensure the correct answer is added first
    //    answers[0] = $"{predictedScore:F0}";

    //    // Define the quartile sizes
    //    float quartileSize = 100f;   //100 "points"
    //    float quartileCount = 4f;   //4 quartiles

    //    // Pick random quartile for correct answer to fall within
        
    //    int correctQuartile = UnityEngine.Random.Range(0, 4);
    //    float minValue = predictedScore - (quartileSize * correctQuartile + 1);
    //    float maxValue = predictedScore + (quartileSize * (quartileCount - correctQuartile));
        
    //    // Define the range for the random values
    //    float range = maxValue - minValue;

    //    Debug.Log("Predicted score range is: " + range + " minValue: " + minValue + " maxValue: " + maxValue);

    //    // Calculate the quartile ranges  
    //    float EndQuartileSize = range / 4;
    //    float[] quartileRanges = new float[4];
    //    for (int i = 0; i < 4; i++)
    //    {
    //        quartileRanges[i] = minValue + EndQuartileSize * i;
    //    }

    //    // Determine the quartile of the correct answer  
    //    int correctAnswerQuartile = (int)((predictedScore - minValue) / EndQuartileSize);
    //    Debug.Log("Predicted score quartile is: " + correctAnswerQuartile);

    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<float> uniqueAnswers = new HashSet<float> { predictedScore };

    //    while (uniqueAnswers.Count < 4)
    //    {

    //        // Generate a random, unique number in a different quartile  
    //        int randomQuartile;
    //        do
    //        {
    //            randomQuartile = UnityEngine.Random.Range(0, 4);
    //        } while (randomQuartile == correctAnswerQuartile);

    //        float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + EndQuartileSize));

    //        // Ensure the random value is at least 10 different from the correct answer  
    //        if (Mathf.Abs(randomValue - predictedScore) >= 10 && !uniqueAnswers.Contains(randomValue))
    //        {
    //            uniqueAnswers.Add(randomValue);
    //        }
    //    }

    //    // Convert the unique numbers into the answers array, skipping the first correct answer
    //    int index = 1;
    //    foreach (float value in uniqueAnswers)
    //    {
    //        if (value != predictedScore)
    //        {
    //            answers[index] = $"{value:F0}";
    //            index++;
    //        }
    //    }

    //    return answers;
    //}
    //string[] FillPredHazardAnswers(int targetIndex)
    //{
    //    string[] answers = new string[4];
    //    float hazardTimePrediction = 0.0f;
    //    float hazardPercentRemaining = 0.0f;
    //    if (roverPath.Count > 0)
    //    {
    //        List<Node_mouse> remainingPath = roverPath.Skip(targetIndex).ToList();
    //        float remainingPathTime = CalculateRouteTime(remainingPath, speed);
    //        if (remainingPathTime == 0.0f)
    //        { 
    //            remainingPathTime = 1.0f;
    //        }
    //        hazardTimePrediction = pathfinder.FindPredictedHazardTime(remainingPath);
    //        hazardPercentRemaining = hazardTimePrediction*100f / remainingPathTime;
    //        Debug.Log($"Total path is {roverPath.Count}, remaining path is {remainingPath.Count}, predicted hazard time is {hazardTimePrediction}, pred hazard percent is {hazardPercentRemaining}");
    //    }
    //    else
    //    {
    //        hazardPercentRemaining = 0.0f;
    //    }

    //    answers[0] = $"{hazardPercentRemaining:F0}";
    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<float> uniqueAnswers = new HashSet<float> { hazardPercentRemaining };

    //    // Generate three random, unique numbers between 0 and 100 in quartiles that are separate from the true value and separate from each other
    //    while (uniqueAnswers.Count < 4)
    //    {
    //        // Define the range for the random values
    //        float minValue = 0f;
    //        float maxValue = 100f;
    //        float range = maxValue - minValue;

    //        // Calculate the quartile ranges
    //        float quartileSize = range / 4;
    //        float[] quartileRanges = new float[4];
    //        for (int i = 0; i < 4; i++)
    //        {
    //            quartileRanges[i] = minValue + quartileSize * i;
    //        }

    //        // Determine the quartile of the true value
    //        int trueValueQuartile = (int)((hazardPercentRemaining - minValue) / quartileSize);

    //        // Generate a random, unique number in a different quartile
    //        int randomQuartile;
    //        do
    //        {
    //            randomQuartile = UnityEngine.Random.Range(0, 4);
    //        } while (randomQuartile == trueValueQuartile || uniqueAnswers.Any(value => (int)((value - minValue) / quartileSize) == randomQuartile));

    //        float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));
    //        if (!uniqueAnswers.Contains(randomValue))
    //        {
    //            uniqueAnswers.Add(randomValue);
    //        }
    //    }
    //    //// Generate three random, unique numbers between 0 and 100
    //    //while (uniqueAnswers.Count < 4)
    //    //{
    //    //    // 0 up to 2x above current
    //    //    float randomValue = Mathf.Round(UnityEngine.Random.Range(0f, hazardPercentRemaining*2f)); // Rounded to 1 decimal place
    //    //    Debug.Log("Hazard rand value 1 " + randomValue);
    //    //    // Make the difference at least 5% different to help make it not just random guesses close to real value
    //    //    if (Mathf.Abs(randomValue - hazardPercentRemaining) >= hazardPercentRemaining * 0.05f && !uniqueAnswers.Contains(randomValue) && randomValue > 0 && randomValue < 100)
    //    //    {
    //    //        uniqueAnswers.Add(randomValue);
    //    //    }
    //    //    if (hazardPercentRemaining == 0.0f)
    //    //    {
    //    //        randomValue = Mathf.Round(UnityEngine.Random.Range(0f, 100f));
    //    //        uniqueAnswers.Add(randomValue);
    //    //        Debug.Log("Hazard rand value 2 " + randomValue);
    //    //    }
    //    //}

    //    // Convert the unique numbers into the answers array, skipping the first correct answer
    //    int index = 1;
    //    foreach (float value in uniqueAnswers)
    //    {
    //        if (value != hazardPercentRemaining)
    //        {
    //            answers[index] = $"{value:F0}";
    //            index++;
    //        }
    //    }
    //    return answers;
    //}
    //string[] FillTimeFinishAnswers(int targetIndex)
    //{
    //    string[] answers = new string[4];
    //    float finishTimePrediction = 0.0f;
    //    if (roverPath.Count > 0)
    //    {
    //        List<Node_mouse> remainingPath = roverPath.Skip(targetIndex).ToList();
    //        finishTimePrediction = gameClock.elapsedTime + CalculateRouteTime(remainingPath, speed); ;
    //    }
    //    else
    //    {
    //        finishTimePrediction = gameClock.elapsedTime;
    //    }

    //    answers[0] = $"{finishTimePrediction:F0}";

    //    // Define the quartile sizes
    //    float quartileSize = 15f;   //15 seconds
    //    float quartileCount = 4f;   //4 quartiles

    //    // Pick random quartile for correct answer to fall within
    //    int correctQuartile = 0;
    //    float minValue = 0f;
    //    float maxValue = quartileSize * 4;
    //    if (finishTimePrediction < quartileSize)
    //    {
    //        correctQuartile = 0;
    //    }
    //    else if (finishTimePrediction < quartileSize * 2)
    //    {
    //        correctQuartile = UnityEngine.Random.Range(0, 2);
    //    }
    //    else if (finishTimePrediction < quartileSize * 3)
    //    {
    //        correctQuartile = UnityEngine.Random.Range(0, 3);
    //    }
    //    else
    //    {
    //        correctQuartile = UnityEngine.Random.Range(0, 4);
    //        minValue = finishTimePrediction - (quartileSize * correctQuartile + 1);
    //        maxValue = finishTimePrediction + (quartileSize * (correctQuartile - quartileCount + 2));
    //    }
    //    Debug.Log("Correct quartile is " + correctQuartile + " min value is " + minValue + " max value is " + maxValue);

    //    // Define the range for the random values
    //    float range = maxValue - minValue;

    //    // Calculate the quartile ranges  
    //    float EndQuartileSize = range / 4;
    //    float[] quartileRanges = new float[4];
    //    for (int i = 0; i < 4; i++)
    //    {
    //        quartileRanges[i] = minValue + EndQuartileSize * i;
    //    }

    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<float> uniqueAnswers = new HashSet<float> { finishTimePrediction };

    //    while (uniqueAnswers.Count < 4)
    //    {
            
    //        // Generate a random, unique number in a different quartile  
    //        int randomQuartile;
    //        do
    //        {
    //            randomQuartile = UnityEngine.Random.Range(0, 4);
    //        } while (randomQuartile == correctQuartile);

    //        float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + EndQuartileSize));

    //        // Ensure the random value is at least 10 different from the correct answer  
    //        if (Mathf.Abs(randomValue - finishTimePrediction) >= 10 && !uniqueAnswers.Contains(randomValue))
    //        {
    //            uniqueAnswers.Add(randomValue);
    //        }
    //    }

    //    //// Generate three random, unique numbers between 0 and 100
    //    //while (uniqueAnswers.Count < 4)
    //    //{
    //    //    //float randomValue = Mathf.Round(UnityEngine.Random.Range(finishTimePrediction- finishTimePrediction*1.5f, finishTimePrediction+ finishTimePrediction*1.5f)); // Rounded to 1 decimal place
    //    //    float randomValue = Mathf.Round(UnityEngine.Random.Range(gameClock.elapsedTime, finishTimePrediction + finishTimePrediction * 2f)); // Rounded to 1 decimal place

    //    //    // Make the difference at least 5 to help make it not just random guesses close to real value
    //    //    if (Mathf.Abs(randomValue - finishTimePrediction) >= 2 && !uniqueAnswers.Contains(randomValue))
    //    //    {
    //    //        uniqueAnswers.Add(randomValue);
    //    //    }
    //    //}

    //    // Convert the unique numbers into the answers array, skipping the first correct answer
    //    int index = 1;
    //    foreach (float value in uniqueAnswers)
    //    {
    //        if (value != finishTimePrediction)
    //        {
    //            answers[index] = $"{value:F0}";
    //            index++;
    //        }
    //    }
    //    return answers;
    //}
    //string[] FillBattFinishAnswers(int targetIndex)
    //{
    //    string[] answers = new string[4];
    //    float battPercent = batterySliderCtrl.batteryLevel;
    //    Debug.Log("Batt percent is " + battPercent);
    //    float drainRate = batterySliderCtrl.drainRate * batterySliderCtrl.movingDrainMultiplier;
    //    Debug.Log("Drain rate is " + drainRate);
    //    float batt2finish = battPercent;
    //    // Add if statement incase route no route generated yet
    //    if (roverPath.Count > 0)
    //    {
    //        List<Node_mouse> remainingPath = roverPath.Skip(targetIndex).ToList();
    //        Debug.Log("Remaining nodes on path " + remainingPath.Count);
    //        float remainingTime = CalculateRouteTime(remainingPath, speed) ; 
    //        Debug.Log("Remaining time is " + remainingTime);
    //        batt2finish = battPercent - remainingTime * drainRate;
    //        Debug.Log("Expected battery at finish is " + batt2finish);
    //    }

    //    answers[0] = $"{batt2finish:F0}%";

    //    // Create a HashSet to avoid duplicate random numbers
    //    HashSet<float> uniqueAnswers = new HashSet<float> { batt2finish };

    //    // Define the range for the random values  
    //    float minValue = 0f;
        

    //    // Calculate the quartile ranges  
    //    float quartileSize = 25f;
    //    float[] quartileRanges = new float[4];
    //    for (int i = 0; i < 4; i++)
    //    {
    //        quartileRanges[i] = minValue + quartileSize * i;
    //    }

    //    // Determine the quartile of the correct answer  
    //    int correctAnswerQuartile = (int)((batt2finish - minValue) / quartileSize);

    //    HashSet<int> quartilesFilled = new HashSet<int> { correctAnswerQuartile };
    //    while (quartilesFilled.Count < 4)
    //    {
    //        int randomQuartile;
    //        do
    //        {
    //            randomQuartile = UnityEngine.Random.Range(0, 4);
    //        } while (quartilesFilled.Contains(randomQuartile));

    //        quartilesFilled.Add(randomQuartile);
    //        float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

    //        if (!uniqueAnswers.Contains(randomValue) && Mathf.Abs(randomValue - batt2finish) > 10)
    //        {
    //            uniqueAnswers.Add(randomValue);
    //            Debug.Log("CurrentScore random value is " + randomValue + "  real value: " + batt2finish);

    //        }
    //    }
    //    //// Generate three random, unique numbers between 0 and 100
    //    //while (uniqueAnswers.Count < 4)
    //    //{
    //    //    // Generate a random, unique number in a different quartile  
    //    //    int randomQuartile;
    //    //    do
    //    //    {
    //    //        randomQuartile = UnityEngine.Random.Range(0, 4);
    //    //    } while (randomQuartile == correctAnswerQuartile);

    //    //    float randomValue = Mathf.Round(UnityEngine.Random.Range(quartileRanges[randomQuartile], quartileRanges[randomQuartile] + quartileSize));

    //    //    // Ensure the random value is at least 10 different from the correct answer  
    //    //    if (Mathf.Abs(randomValue - batt2finish) >= 10 && !uniqueAnswers.Contains(randomValue))
    //    //    {
    //    //        uniqueAnswers.Add(randomValue);
    //    //    }
    //    //}
    //    //Debug.Log("Correct quartile is " + correctAnswerQuartile + " min value is " + minValue + " max value is " + maxValue);

    //    // Convert the unique numbers into the answers array, skipping the first correct answer
    //    int index = 1;
    //    foreach (float value in uniqueAnswers)
    //    {
    //        if (value != batt2finish)
    //        {
    //            Debug.Log("Rover batt2finish index " + index + " + value " + value);
    //            answers[index] = $"{value:F0}%";
    //            index++;
    //        }
    //    }
    //    return answers;
    //}

    public void EndGameAndExportLogs()
    {
        Debug.Log("End Game Button Pressed");
        
        string baseFilePath = "C:\\Users\\markw\\OneDrive\\Documents\\GitHub\\SPACE1_Results\\Demos\\";
        string userFolderPath = Path.Combine(baseFilePath, userID);
        string trialFolderPath = Path.Combine(userFolderPath, trialNum);

        // Ensure the user directory exists
        if (!Directory.Exists(userFolderPath))
        {
            Directory.CreateDirectory(userFolderPath);
        }

        // Ensure the trial directory exists
        if (!Directory.Exists(trialFolderPath))
        {
            Directory.CreateDirectory(trialFolderPath);
        }

        float timeStamp = Time.time;
        string logEntry = $"{timeStamp:F2}, Goals reached: {goalCount},Final AT Score {scoreDisplay.actualScore:F2}, Final Battery level {batterySliderCtrl.batteryLevel}";
        clickLog.Add(logEntry);
        Debug.Log(logEntry);

        timeStamp = Time.time + 1;
        logEntry = $"{timeStamp:F2}, Idle time: {elapsedTimeForScore-elapsedTime},Moving Time {elapsedTime}";
        clickLog.Add(logEntry);
        Debug.Log(logEntry);


        PostRoundIdealScoreCalculator();
        
        // Add a scrub to the clickLog to remove duplicates
        List<string> filteredClickLog = new List<string>();
        float previousTime = -1f;

        foreach (var entry in clickLog)
        {
            // Extract the time part
            float currentTime = float.Parse(entry.Split(',')[0].Trim());


            // Check if the difference between the current and previous time is greater than or equal to 0.01 seconds
            if (previousTime < 0 || (currentTime - previousTime) >= 0.01f)
            {
                filteredClickLog.Add(entry);
                previousTime = currentTime;

            }
        }

        // Replace the original clickLog with the filtered one
        clickLog = filteredClickLog;

        // Add userID and selectedConfig to filenames
        Debug.Log($"userID: {userID}, selectedConfig: {selectedConfig}");
        string filenameSuffix = $"_User_{userID}_Trial_{trialNum}_{selectedConfig}.csv";

        // Add a filename to the filePath, such as 'log.csv'
        string clickLogfullFilePath = Path.Combine(trialFolderPath, "clickLog" + filenameSuffix);
        clickLog.AddRange(textboxManager.logEntries);
        ExportLogToCSV(clickLogfullFilePath, clickLog);
        UploadCSVtoGitHub(clickLogfullFilePath, clickLog, "clickLog" , filenameSuffix);

        // SAGAT file export
        string SAGATfullFilePath = Path.Combine(trialFolderPath, "SAGATResults" + filenameSuffix);
        sagatManager.ExportLogToCSV(SAGATfullFilePath);
        UploadCSVtoGitHub(SAGATfullFilePath, sagatManager.sagatLogEntries, "SAGATResults", filenameSuffix);
        string SAGATTrustfullFilePath = Path.Combine(trialFolderPath, "SAGAT_Trust_Results" + filenameSuffix);
        sagatManager.ExportTrustLogToCSV(SAGATTrustfullFilePath);
        UploadCSVtoGitHub(SAGATTrustfullFilePath, sagatManager.sagatTrustLogEntries, "SAGAT_Trust_Results", filenameSuffix);

        // Run post-Trial Surveys
        runPostTrialSurveys();
    }


    public void PostRoundIdealScoreCalculator()
    {
        Debug.Log("Actual battery remaining: " + batterySliderCtrl.batteryLevel + " Ideal Battery Remaining: " + idealBattRemaining);
        // Add the ideal AT Score to final log
        if (idealBattRemaining > 15f)  // Simulate running more routes to get best possible score
        {
            Debug.Log("Adding more goals for ideal AT score");
            Debug.Log("Goals visited so far " + goalCount);
            // Remove goals from goalList if not visited at this point.
            for (int i = 0; i < goalList.Count; i++)
            {
                //Debug.Log("GoalList " + i + " is " + goalList[i].IsVisited);
                if (goalList[i].IsVisited == false)
                {
                    //Debug.Log("Removing goalList number " + i);
                    goalList.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < goalList.Count; i++)
            {
                //Debug.Log("GoalList " + i + " is " + goalList[i].IsVisited);
            }
            int counter = 0;
            //Debug.Log("Counter: " + counter);

            Vector3 currentPosition = R1rigidbody.position;
            //Debug.Log("Starting position for final loop is " + currentPosition);
            while (idealBattRemaining > 10f)   // Rover can't drive below 10 battery level so stop it here
            {
                //Debug.Log("Counter2: " + counter);
                //Debug.Log("Goals visited so far including final loop " + goalCount);

                // Generate new goals
                int newGoals = 2;

                Vector2[] newGoalArray = buildMap.SpawnGoals(newGoals);  // Spawn new goals two at a time to match how it would appear to player
                pathfinder.PreComputeFinalPaths(newGoalArray, goalCount);
                totalGoals = goalCount + newGoals;
                //Debug.Log("New totalGoals is " + totalGoals);
                goalNumbers.Clear();
                int newGoalIndex = loggedGoals.Count;
                for (int j = 0; j < newGoals; j++)
                {
                    newGoalIndex += j;
                    goalNumbers.Add(newGoalIndex);
                    //Debug.Log("new goalnumbers are " + newGoalIndex);
                    loggedGoals.Add(newGoalIndex);
                }
                //Debug.Log("Goalnumbers new count is "+  goalNumbers.Count);
                buildMap.goalLocations = newGoalArray;

                defaultCostRoverPath = IdealGoalPathFinder(currentPosition, defaultHazardCost, goalNumbers); // Find the ideal path to the new goals

                // Best possible score calculations
                Debug.Log("POST-ROUND: Default cost rover path count is: " + defaultCostRoverPath.Count);

                float goalTimePrediction = CalculateRouteTime(defaultCostRoverPath, speed);
                float batteryRequired = CalculateRouteBattery(goalTimePrediction);
                Debug.Log("Ideal battery remaining is " + idealBattRemaining + " with battery required of " + batteryRequired);
                if (idealBattRemaining - batteryRequired > 10f)
                {

                    idealBattRemaining = AddBestPossibleBatteryUsage(batteryRequired);
                    idealATScore = AddBestPossibleATScore(pathfinder.FindPredictedRouteScore_PostRound(defaultCostRoverPath));
                    Debug.Log("Viable route found, adding to score. New ideal AT score is " + idealATScore + " with battery remaining " + idealBattRemaining);
                }
                else
                {
                    Debug.Log("No viable route found, breaking out of loop. Ideal batt remaining: " + idealBattRemaining + " batt required " + batteryRequired);
                    break;
                }

                goalCount += newGoals;

                for (int i = 0; i < goalList.Count; i++)
                {
                    goalList[i].IsVisited = true; //Set all goals as visited at this point
                }

                // Update currentlocation to end of path
                currentPosition = defaultCostRoverPath[defaultCostRoverPath.Count - 1].worldPosition;
                //Debug.Log("Current position is " + currentPosition);

                counter++;
                if (counter > 10)
                {
                    Debug.Log("Too many iterations to find ideal AT score, breaking out of loop");
                    break;
                }
            }
            Debug.Log("POST ROUND: Best possible AT score is " + idealATScore + " with battery remaining " + idealBattRemaining);


        }

        float timeStamp_ideals = Time.time + 1;
        string logEntry_ideals = $"{timeStamp_ideals:F2}, Best possible AT score: {idealATScore:F2}, Actual score: {scoreDisplay.actualScore}, Score ratio: {((scoreDisplay.actualScore) / idealATScore):F2}";
        clickLog.Add(logEntry_ideals);
        Debug.Log(logEntry_ideals);
    }
    // Method to find the ideal path to goals at the end of the round
    public List<Node_mouse> IdealGoalPathFinder(Vector2 currentPosition, float hazardCost, List<int> goalNumbers)
    {
        if (goalList == null || goalList.Count == 0)
        {
            Debug.LogError("No goals available.");
            return null; // error handling
        }

        List<Node_mouse> fullPath = new List<Node_mouse>();
        //Debug.Log("goalList count is " + goalList.Count);
        for (int i = 0; i < goalList.Count; i++)
        {
            // Check if the goal is already visited
            //Debug.Log("GoalList[" + i + "].IsVisited is " + goalList[i].IsVisited);
            if (goalList[i].IsVisited)
            {
                continue; // Skip this goal if it is already visited
            }

            pathfinder.UpdateAltRoutesAndPaths4(currentPosition, goalList[i].GoalNumber, goalList); // Update the starter paths and cost matrix for all 3 hazardCosts
            Debug.Log("Updated for goal " + goalList[i].GoalNumber);
        }

        List<Goal> unvisitedGoals = goalList.FindAll(g => !g.IsVisited);

        // Only search through the new goals generated after round is complete
        fullPath = pathfinder.FindOptimalPostRoundPath(goalNumbers);
        return fullPath;
    }
    public void runPostTrialSurveys()
    {
        postTrialSurveys.ShowQuestionPanel();
        Time.timeScale = 0f;
    }

    // Also make sure to update in runPostTrialSurveys for the post-trial surveys
    public async void UploadCSVtoGitHub(string filePath, List<string> content, string logDescriptor, string filenameSuffix)
    {
        if (saveData=="false")
        {
            Debug.Log("Data saving is disabled.");
            return;
        }


        
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found!");
            return;
        }

        string fileContent = string.Join("\n", content);
        string base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

        string apiUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/contents/Demos/{userID}/{trialNum}/{logDescriptor}_{filenameSuffix}.csv";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "token " + githubToken);
            client.DefaultRequestHeaders.Add("User-Agent", "UnityGame");

            // Fetch the existing file's sha
            HttpResponseMessage getResponse = await client.GetAsync(apiUrl);
            string sha = null;
            if (getResponse.IsSuccessStatusCode)
            {
                string getResponseContent = await getResponse.Content.ReadAsStringAsync();
                var getResponseData = JsonUtility.FromJson<Dictionary<string, object>>(getResponseContent);
                if (getResponseData.ContainsKey("sha"))
                {
                    sha = getResponseData["sha"].ToString();
                }
            }
            else if (getResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                Debug.LogError($"Failed to check file existence: {getResponse.StatusCode}");
                return;
            }

            var jsonData = new GitHubUploadData
            {
                message = "Updating CSV from Unity",
                content = base64Content,
                sha = sha
            };

            string json = JsonUtility.ToJson(jsonData);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync(apiUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("File successfully uploaded to GitHub!");
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to upload: {response.StatusCode}, {responseContent}");
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }



    [Serializable]
    public class GitHubUploadData
    {
        public string message;
        public string content;
        public string sha;
    }
}
