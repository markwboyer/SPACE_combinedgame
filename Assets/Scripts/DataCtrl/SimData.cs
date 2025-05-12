using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimData : MonoBehaviour
{
    public int subjectNumber;
    public int sessionNumber;
    public int trial;
    public string ExplanationCondition; // Condition A-I for which explanations are displayed

    public int paradigm; // 0 is adaptive, 1 is non-adaptive

    // Check for proficiency
    public bool[] proficiency = new bool[3];

    // Performance metrics
    public int terrainDifficulty; // should be a number 1-24
    public int robotDifficulty; // WIP, but temporary variable for later
    public int rockDifficulty; // should be 1-12, eventually 1-24
    public int seed;
    public int environment; // 0 for vr, 1 for screen, 2 for mockup
    public float navPerformance; // Stores the player's performance on the navigation task
    public float navTarget; // Stores the ideal player performance
    public float robotDuration; // stores the time taken for the robotic arm task
    public int rockScore; // The score of the player (+1 for ROSIs, -1 for incorrect, 0 for no selection)
    public int rockTarget; // The number of ROSIs generated on the map

    public int bedford; // Return value for bedford scale

    public int perceivedDifficulty;

    // Binary failure flags
    public bool flippedVehicle = false; // Did player crash the rover during the navigation task?
    public bool tornCable = false; // Did the player tear the cable during the robot task?
    public bool singularity_halt = false;

    // Added 2/26/2025 - more crash fidelity
    public bool crashedVehicle = false; // Did player crash the rover during the navigation task?

    // Some of the variables below are WIP, and may go unused
    // This is to make sure that the framework remains compatable with the EDL server setup

    //Difficulty data
    public int[] next_level; // What difficulty step should we load EDL with?
    public int[] difficulty_level; //Comes from server based on past performance
    public int[] lockstep; //Are we in lockstep?

    public List<PlayerData> playerdatalist = new List<PlayerData>(); //list of player data for sending to server
    
    private static SimData instance;

    void Awake()
    {
        // Instance mangement code from:
        // https://gamedev.stackexchange.com/questions/182018/how-do-i-check-if-there-is-already-an-object-in-this-scene-with-the-same-name-up
        // If there's already a copy of me, self-destruct and abort!
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
