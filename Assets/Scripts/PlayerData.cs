using UnityEngine;

// A class used for sending/receiving data from the server
// Not used for actual in-game player data management
// In-game management is performed in SimData.cs
public class PlayerData
{
    //Player Data
    public string player_tag; //From player manager
    public int sessionNumber; //Subject session number
    public int environment; //Vr, Screen, or High-Bay
    public int paradigm; //Adaptive or non-adaptive

    public int[] difficulty_level = new int[3]; //Comes from server based on past performance
    //Manual Control Index = 0, Landing Site Index = 1, Descent Speed Index = 2
    public int[] performance = new int[3]; //Calcualted in Staircase script update function, will be an array
    //public double[] rawPerformance = new double[3]; //raw performance values from that trial
    public RawData rawPerformance = new RawData();

    public bool crashedVehicle; //Did player crash the rover during the navigation task?  ADDED 2/26 MWB

    //Survey Data
    public int bedford;

    public int perceived_difficulty;

    public int trial_no; //Changes with each run, ++ each download inn flight dynamics
    public int[ ] SART = new int [9];


    //May need to delete later, just for testing
    public int[] lockstep = new int[] {};
    public int[] next_level = new int[] {}; //The server node.js script wanted next_level as an array so I commented out the above declaration

    public string Stringify()
    {
        return JsonUtility.ToJson(this);
    }

    public static PlayerData Parse(string json)
    {

        return JsonUtility.FromJson<PlayerData>(json);
    }
}