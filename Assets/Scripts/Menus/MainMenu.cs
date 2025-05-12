using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.LSL4Unity.Scripts;
using UnityEngine.EventSystems;


public class MainMenu : MonoBehaviour
{
    GameObject WelcomeScreen;
    GameObject AlgoMenu;
    GameObject SandboxMenu;
    GameObject TrinityMenu;

    SimData playerData;

    int dropIDX;

    //LSL Markers
    private LSLMarkerStream triggers; 

    void Awake()
    {
        //Find LSL Stream
        triggers = FindObjectOfType<LSLMarkerStream>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public void Start()
    { 
        // Initialize screens
        WelcomeScreen = this.transform.GetChild(0).gameObject;
        AlgoMenu = this.transform.GetChild(2).gameObject;
        SandboxMenu = this.transform.GetChild(1).gameObject;
        TrinityMenu = this.transform.GetChild(3).gameObject;
        AlgoMenu.SetActive(false);
        SandboxMenu.SetActive(false);
        TrinityMenu.SetActive(false);

        playerData = GameObject.Find("Player").GetComponent<SimData>();
    }

    // Setup for a sandbox subject
    public void TrinitySandboxSetup(){
        // Temporary: grab difficulty from field and add to simdata

        string terrainText = SandboxMenu.transform.Find("DifficultyField").GetComponent<TMP_InputField>().text;
        int terrainDifficulty;

        // If terrainDifficulty is left empty, set it to 1 (easiest)
        try { terrainDifficulty = int.Parse(terrainText);}
        catch (System.Exception){terrainDifficulty = 1;}

        // Force the value to be between 1 and 24
        terrainDifficulty = Mathf.Clamp(terrainDifficulty,1,24);
        playerData.terrainDifficulty = terrainDifficulty;
        

        //int robotDifficulty;
        //string robotText = SandboxMenu.transform.Find("RobotField").GetComponent<TMP_InputField>().text;
        //try { robotDifficulty = int.Parse(robotText); }
        //catch (System.Exception) { robotDifficulty = 1; }
        //playerData.robotDifficulty = Mathf.Clamp(robotDifficulty, 1, 24);

        // Populate seed
        string seedText = SandboxMenu.transform.Find("SeedField").GetComponent<TMP_InputField>().text;
        int seed;
        // If left blank, choose a random seed
        try{ seed = int.Parse(seedText);}
        catch (System.Exception) {seed = 0;}

        if (seed == 0)
        {
            seed = Random.Range(1,12);
        }
        playerData.seed = seed;
        try {

            playerData.navTarget = SeedBank.getSandboxFuelEst(terrainDifficulty, seed);
        }
        catch (System.IndexOutOfRangeException e)
        {
            Debug.Log("Seed not found, using fake fuel estimation.");
            playerData.navTarget = 2.0f;
        }

        //string rockDiffTxt = SandboxMenu.transform.Find("ObservationField").GetComponent<TMP_InputField>().text;
        //int rockDiff;

        //try { rockDiff = int.Parse(rockDiffTxt); }
        //catch (System.Exception) {rockDiff = 1;}; 

        //int dropIDX = SandboxMenu.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().value;
        playerData.environment = dropIDX;
        //playerData.rockDifficulty = rockDiff;

        string subjectText = SandboxMenu.transform.Find("UserIDField").GetComponent<TMP_InputField>().text;
        Debug.Log("Subject Text: " + subjectText);
        playerData.subjectNumber = subjectText == "" ? 0 : int.Parse(subjectText);

        string sessionText = SandboxMenu.transform.Find("TrialNumberField").GetComponent<TMP_InputField>().text;
        Debug.Log("Session Text: " + sessionText);
        playerData.sessionNumber = sessionText == "" ? 0 : int.Parse(sessionText);
        string explCondition = SandboxMenu.transform.Find("ConfigDropdown").GetComponent<TMP_Dropdown>().options[
           SandboxMenu.transform.Find("ConfigDropdown").GetComponent<TMP_Dropdown>().value].text;
        Debug.Log("Expl Condition: " + explCondition);
        playerData.ExplanationCondition = explCondition;

        PlayerPrefs.SetString("UserID", subjectText);
        PlayerPrefs.SetString("SelectedConfig", explCondition);
        PlayerPrefs.SetString("TrialNumber", sessionText);
        Debug.Log("Stored UserID: " + PlayerPrefs.GetString("UserID"));
        Debug.Log("Stored Trial Number: " + PlayerPrefs.GetString("TrialNumber"));
        Debug.Log("Stored Config: " + PlayerPrefs.GetString("SelectedConfig"));

        
        SandboxMenu.gameObject.SetActive(false);
        SceneManager.LoadScene("Rover", LoadSceneMode.Additive);
        SceneManager.LoadScene("MOUSE GameScene", LoadSceneMode.Additive);
        StartCoroutine(SetMouseDisplay());
        StartCoroutine(TriggerMouseCameraAssignment());
        Debug.Log("Loaded Rover from TrinitySandboxSetup");

    }

    void RemoveExtraEventSystems()
    {
        EventSystem[] systems = GameObject.FindObjectsOfType<EventSystem>();
        if (systems.Length > 1)
        {
            // Keep the first one, destroy the rest
            for (int i = 1; i < systems.Length; i++)
            {
                Destroy(systems[i].gameObject);
            }
        }
    }

    void FixAudioListeners()
    {
        AudioListener[] listeners = GameObject.FindObjectsOfType<AudioListener>();
        if (listeners.Length > 1)
        {
            // Keep only the first enabled one
            for (int i = 1; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
            }
        }
    }
    private IEnumerator TriggerMouseCameraAssignment()
    {
        // Wait a couple of frames to ensure Mouse scene is loaded
        yield return null;
        yield return null;

        var mouseController = FindObjectOfType<RoverDriving>();
        if (mouseController != null)
        {
            mouseController.AssignCameraWhenAvailable();
            Debug.Log("Assigned camera to RoverDriving in MOUSE scene.");
        }
        else
        {
            Debug.LogWarning("MouseSceneController not found in scene.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(CleanupAfterSceneLoad());
    }

    private IEnumerator CleanupAfterSceneLoad()
    {
        yield return null; // Wait one frame so everything initializes
        RemoveExtraEventSystems();
        FixAudioListeners();
    }


    private IEnumerator SetMouseDisplay()
    {
        // Wait one frame for the scene to finish loading
        yield return null;

        // Find the camera in the MOUSE scene (change name as needed)
        Camera mouseCam = GameObject.Find("SPACE_mouse_cam")?.GetComponent<Camera>();
        if (mouseCam != null)
        {
            // Activate Display 5 (index 4)
            if (Display.displays.Length > 4)
            {
                Display.displays[4].Activate();
                Display.displays[4].SetRenderingResolution(Display.displays[4].systemWidth,
                                                           Display.displays[4].systemHeight);
            }

            mouseCam.targetDisplay = 4; // 0-based index -> Display 5
            mouseCam.enabled = true;

            // Fix: Iterate over the array of RoverDriving objects and call InitializeRoverDrivingScene on each
            var mouseRoverDriving = GameObject.FindObjectsOfType<RoverDriving>();
            if (mouseRoverDriving != null)
            {
                foreach (var rover in mouseRoverDriving)
                {
                    rover.InitializeRoverDrivingScene();
                }
                Debug.Log("Initialized RoverDriving scene for MOUSE.");
            }
        }
        else
        {
            Debug.LogWarning("Could not find 'mouse_display_cam' after loading Mouse GameScene.");
        }
    }

    // Setup for actual subject
    public void TrinitySetup(){
        playerData.subjectNumber = int.Parse(TrinityMenu.transform.Find("SubjectField").GetComponent<TMP_InputField>().text);
        playerData.sessionNumber = int.Parse(TrinityMenu.transform.Find("SessionField").GetComponent<TMP_InputField>().text);
        playerData.paradigm = TrinityMenu.transform.Find("ParadigmDropdown").GetComponent<TMP_Dropdown>().value;

        //Perform the first upload function to check for player existance
        StartCoroutine(ServerCommunication.FirstUpload(playerData.subjectNumber.ToString(), result => {
            Debug.Log("First Upload Results: " + result);
            //Then, download from the server and start the trial
            DownloadAndStart();
 
        }));
    }

    public void DownloadAndStart()
    {   
        StartCoroutine(ServerCommunication.Download(playerData.GetComponent<SimData>().subjectNumber.ToString(), result => {
        Debug.Log("Download Results: " + result.Stringify());
        // TODO -> determine terrain level by dividing terrain difficulty by 3
        playerData.terrainDifficulty = result.difficulty_level[0];
        playerData.robotDifficulty = result.difficulty_level[1];
        playerData.rockDifficulty = result.difficulty_level[2];
        playerData.environment = dropIDX;
        SeedBank.getNavSeed(playerData);

        playerData.next_level = result.next_level;
        playerData.difficulty_level = new int[3];

        for (int x = 0; x < result.difficulty_level.Length; x++) {
            playerData.difficulty_level[x] = Mathf.Clamp(result.difficulty_level[x] + result.next_level[x], 0, 25); //Don't let difficulty level go negative

            /*if (playerData.environment == 0) //if in HIGH BAY mockup TODO enable later
                {
                    playerData.difficulty_level[x] = 18; //set difficulty to 18
                }*/

            //Check to make sure its not a skip level
            if (playerData.difficulty_level[x] == 18 && playerData.environment > 0)
            {
                //If the next level is the skip level (18), make sure to skip it
                if(result.next_level[x] <=0 ){
                    //if we were stepping DOWN to 18, skip down to 17
                    playerData.difficulty_level[x] = 17;
                }
                else {
                    playerData.difficulty_level[x] = 19; //otherwise, skip UP to 19
                }
            }
                
        } 

        playerData.next_level = new int[] {0,0,0}; //clear the next level info once everything is set
        playerData.lockstep = result.lockstep; //determine if anything is in lockstep

        // Update variables locally
        playerData.terrainDifficulty = playerData.difficulty_level[0];
        playerData.robotDifficulty = playerData.difficulty_level[1];
        playerData.rockDifficulty = playerData.difficulty_level[2];
        playerData.trial = 1;

        SceneManager.LoadScene("Rover");
            Debug.Log("Loaded Rover from DownloadandStart");
        triggers.Write("Navigation Task Started"); 
        }));
    }

    // Move from the start screen to the algo selection screen (choose between sandbox and server algo)
    public void StartContinue()
    {
        dropIDX = WelcomeScreen.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().value;

        //AlgoMenu.SetActive(true);
        WelcomeScreen.SetActive(false);
        SandboxMenu.SetActive(true);    // skip right to sandbox menu since using sandbox every time
    }

    // Go back to a previous screen.  Which screen to go to is determined by the number
    public void GoBack(int screenNum)
    {
        if (screenNum == 1) // Go from algorithm back to home screen
        {
            WelcomeScreen.SetActive(true);
            AlgoMenu.SetActive(false);
        }
        else if (screenNum == 2) // Go from TRINITY back to algo
        {
            TrinityMenu.SetActive(false);
            AlgoMenu.SetActive(true);
        }
        else if (screenNum == 3) // Go from Sandbox back to algo
        {
            SandboxMenu.SetActive(false);
            AlgoMenu.SetActive(true);
        }
    }

    public void AlgoType()
    {
        int algo = AlgoMenu.transform.Find("Dropdown").GetComponent<TMP_Dropdown>().value;
        if (algo == 0)
        {
            TrinityMenu.SetActive(true);
        }
        else
        {
            SandboxMenu.SetActive(true);
        }
        AlgoMenu.SetActive(false);
    }

    public void QuitGame(){
        Application.Quit();
    }

}
