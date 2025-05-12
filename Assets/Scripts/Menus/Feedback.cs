using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Assets.LSL4Unity.Scripts;
using System.IO;
using System.Text;

public class Feedback : MonoBehaviour
{
    GameObject Player;
    GameObject LockMenu;
    PlayerData pd;

    SimData playerData;
    List<PlayerData> pd_list;

    bool recievedResult = false;

    public int[] performance; //value to input into player data --> array of [MC, LS, DT] ranked (-1, 0, 1) based on poor, adequate, excellent  
    //public double[] rawPerformance; //Raw performance metrics array to send to server --> array of [MC, LS Selected Distance, LS Best Distance, DT, Fuel Remaining]
    RawData rawPerformance;
    float[] terrainData;

    public bool prof_complete = false; // bool to check if we are completely proficient

    public List<Button> Poor; //poor buttons
    public List<Button> Adequate; //adequate buttons
    public List<Button> Excellent; //excellent buttons

    public Button NextTrial_Button;

    public GameObject gameOver; //popup screen when proficiency is reached and game is going to end

    //LSL Markers
    private LSLMarkerStream triggers;

    void Awake()
    {
        //Find LSL Stream
        triggers = FindObjectOfType<LSLMarkerStream>();
    }


    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Player");
        LockMenu = GameObject.Find("MenuInterface/LockMenu");
        playerData = Player.GetComponent<SimData>();
        gameOver.SetActive(false);

        performance = new int[3];
        getPerformance();
    }

    // Gets the performance data from the trial and fills it into the values in the scene
    void getPerformance()
    {
        // Calculate navigation task performance
        float navDiff = playerData.navPerformance - playerData.navTarget;
        // Less than first value means excellent, between two values is adequate
        // Less than second value is poor performance
        float[] navMetric = { 0.1f, 0.2f };
        if (navDiff < navMetric[0]) { performance[0] = 1; } // Good performance
        else if (navDiff < navMetric[1]) { performance[0] = 0; } // Adequate performance
        else { performance[0] = -1; } // Poor performance

        // Calculate robotic arm task performance
        performance[1] = 1; // Always score excellent until task exists
        if (playerData.robotDuration < 15.0f)
        {
            performance[1] = 1;
        }
        else if (playerData.robotDuration < 20.0f)
        {
            performance[1] = 0;
        }
        else if (playerData.robotDuration > 60.0f)
        {
            performance[1] = -1;
        }
        else
        {
            performance[1] = -1;
        }

        /**** Calculate observation task performance ****/
        float[] rockMetric = { 5.0f, 4.0f };
        float rockPercent = (float)playerData.rockScore / playerData.rockTarget;
        // if 6+ rocks, excellent; if 4-5, adequate; if <4, poor

        // Less than first value means excellent, between two values is adequate
        // Less than second value is poor performance
        Debug.Log("Rock score calculated: " + playerData.rockScore + "/" + playerData.rockTarget + " evaluated to: " + rockPercent);
        if (playerData.rockScore > rockMetric[0]) { performance[2] = 1; } // Good performance
        else if (playerData.rockScore > rockMetric[1]) { performance[2] = 0; } // Adequate performance
        else { performance[2] = -1; } // Poor performance

        // Override conditions for binary failure
        if (playerData.flippedVehicle)
        {
            performance[0] = -1;
            GameObject.Find("Canvas/Crash/Nav_Crash").SetActive(true);
            triggers.Write("Rover Crashed");
        }
        if (playerData.tornCable)
        {
            performance[1] = -1;
            GameObject.Find("Canvas/Crash/Rob_Crash_Torn").SetActive(true);
            triggers.Write("Cable Torn");
        }
        if (playerData.singularity_halt)
        {
            performance[1] = -1;
            GameObject.Find("Canvas/Crash/Rob_Crash_Singularity").SetActive(true);
            triggers.Write("Robot Arm Reached Singularity");
        }
        if (playerData.crashedVehicle)
        {
            performance[0] = -1;
            GameObject.Find("Canvas/Crash/Nav_Crash").SetActive(true);
            triggers.Write("Rover Crashed");
        }

        // Check if this should be their last trial
        // First, trial control for session 0 (training)
        if (Player.GetComponent<SimData>().sessionNumber == 0) // training
        {
            // We max out at 40 trials
            if (playerData.trial >= 40)
            {
                // Disable the next trial button
                NextTrial_Button.gameObject.SetActive(false);
                triggers.Write("Player reached 40 trials");
            }

            // Also stop a subject if they don't have enough trials left to get proficient
            // Find the lowest difficulty level subtask
            int lowest_level = 13; // arbitrarily high number to be replaced in loop
            for (int x = 0; x < Player.GetComponent<SimData>().difficulty_level.Length; x++)
            {
                if (playerData.difficulty_level[x] < lowest_level)
                {
                    lowest_level = Player.GetComponent<SimData>().difficulty_level[x];
                }
            }

            // See how many trials are needed to become proficient from this level
            int min_trials_to_prof = ((12 - lowest_level) * 2) + 1; // we have a margin of 1 to account for being halfway to a step on a given level

            // Do we have that many trials remaining?
            if ((40 - Player.GetComponent<SimData>().trial) < min_trials_to_prof)
            {
                // Stop the game
                Debug.Log("STOPPING. There are not enough trials left to reach proficiency. You need atleast " + min_trials_to_prof + " trials.");
                triggers.Write("Player did not reach proficiency");
                //Disable the "Next Trial" Button 
                NextTrial_Button.gameObject.SetActive(false);
            }
        }

        // For experimental trials, max out at trial 10
        if (Player.GetComponent<SimData>().sessionNumber == 1)
        {
            if (Player.GetComponent<SimData>().trial >= 10)
            {
                //Disable the next trial button
                NextTrial_Button.gameObject.SetActive(false);
            }
        }

        updateScreen();
        SendDataToServer();
    }

    void updateScreen()
    {
        for (int i = 0; i < 3; i++)
        {
            if (performance[i] == 1)
            {
                // Excellent performance
                Poor[i].image.color = new Color32(255, 10, 0, 75); //Make poor opaque
                Poor[i].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 40);
                Adequate[i].image.color = new Color32(255, 228, 0, 75); //Make adequate opaque
                Adequate[i].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 40);
            }
            else if (performance[i] == 0)
            {
                // Adequate performance
                Poor[i].image.color = new Color32(255, 10, 0, 75); //Make poor opaque
                Poor[i].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 40);
                Excellent[i].image.color = new Color32(0, 255, 41, 75); //Make excellent opaque
                Excellent[i].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 40);
            }
            else
            {
                // Poor performance
                Adequate[i].image.color = new Color32(255, 228, 0, 75); //Make adequate opaque
                Adequate[i].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 40);
                Excellent[i].image.color = new Color32(0, 255, 41, 75); //Make excellent opaque
                Excellent[i].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 40);
            }
        }
    }

    public void SendDataToServer()
    {
        Debug.Log("SendDataToServer method called");
        // transform.Find("SeedField").GetComponent<TMP_InputField>().text;
        // Temporary variables to allow server communication
        //string navStep = LockMenu.transform.Find("NavLock").GetComponent<TMP_InputField>().text;
        //string robStep = LockMenu.transform.Find("RoboticLock").GetComponent<TMP_InputField>().text;
        //string obsStep = LockMenu.transform.Find("ObservationLock").GetComponent<TMP_InputField>().text;

        int[] tempDiffLevel = { playerData.terrainDifficulty, playerData.robotDifficulty, playerData.rockDifficulty };
        int[] tempPerf = performance; //{int.Parse(navStep), int.Parse(robStep), int.Parse(obsStep)};
        //double[] tempRaw = { playerData.navTarget-playerData.navPerformance, 0, (double) playerData.rockScore / playerData.rockTarget };

        Player = GameObject.Find("Player");
        updatePlayerData();
        List<PlayerData> datalist = Player.GetComponent<SimData>().playerdatalist;
        pd = datalist[datalist.Count - 1];

        // subject data to send
        pd.environment = Player.GetComponent<SimData>().environment;
        pd.paradigm = Player.GetComponent<SimData>().paradigm;
        pd.player_tag = Player.GetComponent<SimData>().subjectNumber.ToString(); //Get player id
        pd.trial_no = Player.GetComponent<SimData>().trial; //Get trial number
        pd.sessionNumber = Player.GetComponent<SimData>().sessionNumber; //Get session number
        pd.next_level = new int[] { 0, 0, 0 };  // Formerly: Player.GetComponent<SimData>().next_level;
        pd.lockstep = Player.GetComponent<SimData>().lockstep; //Player.GetComponent<SimData>().lockstep;
        pd.bedford = Player.GetComponent<SimData>().bedford;
        pd.perceived_difficulty = Player.GetComponent<SimData>().perceivedDifficulty;
        pd.difficulty_level = tempDiffLevel; //Player.GetComponent<SimData>().difficulty_level;

        pd.crashedVehicle = Player.GetComponent<SimData>().crashedVehicle;

        //Performance data to send
        pd.performance = tempPerf; //performance;
        RawData rp = new RawData();
        rp.navPerformance = playerData.navPerformance;
        rp.navTarget = playerData.navTarget;
        rp.obsRocks = playerData.rockTarget;
        rp.obsScore = playerData.rockScore;
        rp.robotDuration = playerData.robotDuration;
        pd.rawPerformance = rp; //tempRaw; //rawPerformance;

        //Debug.Log(pd.Stringify());
        Debug.Log("Sending data to server...");
        string sendString = pd.Stringify();
        string fixedObj = JsonUtility.ToJson(pd.rawPerformance);
        sendString = sendString.Replace("{\"instanceID\":0}", fixedObj);
        Debug.Log(fixedObj);
        Debug.Log(sendString);
        //StartCoroutine(ServerCommunication.Upload(sendString, pd.player_tag, result =>
        //{
        //    Debug.Log(result);
        //    //StartNextTrial();
        //}));

        // Save CSV to local folder
        string path = $@"C:\Users\markw\OneDrive\Documents\GitHub\SPACE1_Results\Demos\{pd.player_tag}\{pd.sessionNumber}\Player {pd.player_tag}_Trial {pd.sessionNumber}.csv";
        StringBuilder csvContent = new StringBuilder();
        csvContent.AppendLine("player_tag,sessionNumber,environment,paradigm,difficulty_level,performance,crashedVehicle,bedford,perceived_difficulty,trial_no,SART,lockstep,next_level,navPerformance,navTarget,obsRocks,obsScore,robotDuration");

        // Add player data to CSV
        csvContent.AppendLine($"{pd.player_tag},{pd.sessionNumber},{pd.environment},{pd.paradigm},{string.Join(";", pd.difficulty_level)},{string.Join(";", pd.performance)},{pd.crashedVehicle},{pd.bedford},{pd.perceived_difficulty},{pd.trial_no},{string.Join(";", pd.SART)},{string.Join(";", pd.lockstep)},{string.Join(";", pd.next_level)},{pd.rawPerformance.navPerformance},{pd.rawPerformance.navTarget},{pd.rawPerformance.obsRocks},{pd.rawPerformance.obsScore},{pd.rawPerformance.robotDuration}");

        // Ensure the directory exists before writing the file
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        File.WriteAllText(path, csvContent.ToString());

        Debug.Log("Rover Data saved to " + path);

        // Find the GameObject containing the RoverDriving script
        GameObject roverDrivingObject = GameObject.Find("RoverNavigation");
        if (roverDrivingObject != null)
        {
            RoverDriving roverDriving = roverDrivingObject.GetComponent<RoverDriving>();
            if (roverDriving != null)
            {
                // Call the EndGameAndExportLogs method
                roverDriving.EndGameAndExportLogs();
            }
            else
            {
                Debug.LogError("RoverDriving component not found on the GameObject.");
            }
        }
        else
        {
            Debug.LogError("GameObject containing RoverDriving script not found in the scene.");
        }

        //EndSession();

        //Debug.Log("Player Data: " + pd.Stringify());
    }

    // TODO temporary function to allow playerData list to be created and updated
    void updatePlayerData()
    {
        pd_list = playerData.playerdatalist;
        PlayerData sendData = new PlayerData();

        sendData.player_tag = Player.GetComponent<SimData>().subjectNumber.ToString(); //Get player id
        pd_list.Add(sendData);



    }

    public void StartNextTrial()
    {
        //First, we want to download the next trial information from the server
        StartCoroutine(ServerCommunication.Download(Player.GetComponent<SimData>().subjectNumber.ToString(), result =>
        {
            Debug.Log("Download Results: " + result.Stringify());

            SeedBank.getNavSeed(playerData);
            Player.GetComponent<SimData>().flippedVehicle = false;
            Player.GetComponent<SimData>().tornCable = false;



            Player.GetComponent<SimData>().next_level = result.next_level;
            Player.GetComponent<SimData>().difficulty_level = new int[3];
            Player.GetComponent<SimData>().lockstep = result.lockstep; //determine if anything is in lockstep

            if (Player.GetComponent<SimData>().sessionNumber == 0 && Player.GetComponent<SimData>().trial > 1) //if in training, need to check for proficiency
            {
                Debug.Log("START NEXT TRIAL: Checking for proficiency before loading the next trial...");
                for (int x = 0; x < result.difficulty_level.Length; x++)
                {
                    if (result.difficulty_level[x] >= 12)
                    {
                        Debug.Log("Subtask " + x + " = at level 12");
                        // Check if the level 12 subtask is about to step to proficiency
                        if (Player.GetComponent<SimData>().next_level[x] == 1)
                        {
                            // Then we have reached proficiency
                            Debug.Log("Proficiency reached in subtask : " + x);
                            // Trip that task proficiency tool
                            Player.GetComponent<SimData>().proficiency[x] = true;
                            Debug.Log("Placing a 0 in the next level array at subtask " + x);
                            Player.GetComponent<SimData>().next_level[x] = 0; //and should not step up past 12
                        }
                        else
                        {
                            Debug.Log("Subtask " + x + " is at level 12 but is not ready to step up");
                        }
                    }
                }

                //If all 3 trip to proficient and we've done 10 trials, then we are done
                //So lets check all 3 
                prof_complete = true;
                foreach (bool check in Player.GetComponent<SimData>().proficiency)
                {
                    if (!check)
                    {
                        //Proficiency not reached!
                        prof_complete = false;
                        Debug.Log("Not proficient yet! I think the prof_complete variable = " + prof_complete);
                        break;
                    }
                }
                //otherwise, prof_complete stays true
                if (prof_complete)
                {
                    if (Player.GetComponent<SimData>().trial >= 10)
                    {
                        Debug.Log("Ending Session due to proficiency!");
                        StartCoroutine(Prof_Coroutine());
                    }
                    else
                    {
                        Debug.Log("Proficient, but not at 10 trials yet");
                        Player.GetComponent<SimData>().paradigm = 0; //set static difficulty
                    }

                }
                else
                {
                    Debug.Log("We get to do another trial, because all 3 tasks are not proficient. I think prof_complete = " + prof_complete);
                }
            }

            try
            {
                for (int x = 0; x < result.difficulty_level.Length; x++)
                {

                    Player.GetComponent<SimData>().difficulty_level[x] = Mathf.Clamp(result.difficulty_level[x] + result.next_level[x], 0, 25);
                }
            }
            catch
            {
                Debug.Log("Next level array not found on server. Continuing with previous levels...");
                for (int x = 0; x < result.difficulty_level.Length; x++)
                {

                    Player.GetComponent<SimData>().difficulty_level[x] = Mathf.Clamp(result.difficulty_level[x], 0, 25);
                }

            }
            for (int i = 0; i < result.difficulty_level.Length; i++) // if level 0 for any subtask, play video
            {
                if (Player.GetComponent<SimData>().difficulty_level[i] == 0)
                {
                    SceneManager.LoadScene("Videos");
                    triggers.Write("Player reached level 0 and was shown training video");
                }
            }

            Player.GetComponent<SimData>().next_level = new int[] { 0, 0, 0 }; //clear the next level info once everything is set

            Debug.LogFormat("PD Diff: {0}, {1}, {2}", playerData.difficulty_level[0], playerData.difficulty_level[1], playerData.difficulty_level[2]);
            // terrainData = DifficultyData.getTerrainDifficulty(result.difficulty_level[0]);
            playerData.terrainDifficulty = playerData.difficulty_level[0];
            playerData.robotDifficulty = playerData.difficulty_level[1];
            playerData.rockDifficulty = playerData.difficulty_level[2];

        }));

        playerData.trial += 1;
        SceneManager.LoadScene("Rover");
        triggers.Write("Rover scene begins");

    }

    IEnumerator Prof_Coroutine()
    {
        //Show subject why we are ending the game
        gameOver.SetActive(true);
        //Wait for 3 seconds
        yield return new WaitForSeconds(3);
        //Then end the session
        //EndSession();
    }

    public void EndSession()
    {
        //This function is mapped to the "End Session" button, which allows a user to quit the game after they reach 10 trials.
        //Since the game won't build correctly with an editor command, we use Preprocessor directives

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
        triggers.Write("End of Sesssion");
    }
}
