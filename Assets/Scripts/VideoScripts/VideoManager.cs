using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.LSL4Unity.Scripts;

public class VideoManager : MonoBehaviour
{
    // This scripts builds on work found at: https://gamedev.stackexchange.com/questions/149338/unity-how-to-make-an-array-of-video-clips-and-play-them

    public VideoPlayer videoPlayer;
    
    GameObject Player; //Player
    public Text waitText; //Text asking subjects to wait for video to end.

    public int[] taskCheck; //will store which tasks need training videos

    public VideoClip[] clips = new VideoClip[3]; //Store 3 video clips, one for each subtask

    //Array of bools to check which videos have been watched (if any need to be)
    public bool[] watched; 

    //LSL Markers
    private LSLMarkerStream triggers; //For 

    void Awake()
    {
        //Find LSL Stream
        triggers = FindObjectOfType<LSLMarkerStream>();
    }


    void Start()
    {
        videoPlayer = this.GetComponent<VideoPlayer>();
        taskCheck= new int[3]; //Instantiates the array for which task videos will run
        watched = new bool[] {false, false, false}; //Instantiates the array that will check if training videos have been or need to be watched. 

        Player = GameObject.Find("Player"); //Find the player object, which we will access to determine which videos to play
        
        //Iterate through Sim Data difficulty array and find 
         for (int x = 0; x < Player.GetComponent<SimData>().difficulty_level.Length; x++) 
         { //Iterate through each difficulty value in SimData

            if (Player.GetComponent<SimData>().difficulty_level[x] == 0)
            {
                //This task is in level 0, and we need to watch the training video.
                taskCheck[x] = 1;
                //Debug.Log("The video for task " + x+ " will play");
            }
            else
            {
                //Otherwise, we do NOT need to watch the training video for that task
                taskCheck[x] = 0;
                //Since we do not need to watch this video, we'll set it to "watched" in our check array
                watched[x] = true;
            }
        }

        //Now lets see what video should be watched first
        for ( int id = 0; id < taskCheck.Length; id++)
        {
            if (taskCheck[id] == 1)
            {
                //We need to watch a training video
                PlayVideo(id); //runs the PlayVideo method for this ID
                triggers.Write("Training video played");
            }
        }
    }
   
    public void PlayVideo(int id)
    {
        //This method will play the video desired

        // To be safe, let's bounds-check the ID 
        // and throw a descriptive error to catch bugs.
        if(id < 0 || id >= clips.Length) {
            Debug.Log("Cannot play video #" + id + " . The video clip array contains " + clips.Length + " videos");
            return;
        }

        // If we get here, we have a video clip at this index
        //Assign that clip to be played by the video player:
        videoPlayer.clip = clips[id];
        //And play it:
        videoPlayer.Play();
        //And record that it has been played
        watched[id] = true;
    }

    public void CheckVideos()
    {
        if(videoPlayer.isPlaying)
        {
            //Debug.Log("Video is still playing!!");
            waitText.color = new Color32(255, 255, 255, 255);
        }
        
        else
        {
            //Debug.Log("Video Button Pressed");
            //This method checks to see if any other training videos should be shown. This will be called when the subject tries to "move on" 
            for (int x = 0; x < watched.Length; x++)
            {
                //Debug.Log("check video index = " + x);

                //Debug.Log("The value at this index is " + watched[x]);

                if(!watched[x])
                {
                    //Then we stil need to watch the video for this subtask
                    Debug.Log("The video for subtask " + x + " will start.");
                    waitText.color = new Color32(255, 255, 255, 0); //get rid of waitText 
                    PlayVideo(x); 
                    return;
                }

                else
                {
                    //Debug.Log("We do not need to watch video "+x);
                }
            }


            //We are all done watching videos, and we can move on to the next trial
            //Debug.Log("VIDEOS ARE DONE");

            for (int z = 0; z < Player.GetComponent<SimData>().difficulty_level.Length; z++ ) //Pushes all the level 0's up to level 1
            {
                if(Player.GetComponent<SimData>().difficulty_level[z] == 0)
                {
                    Player.GetComponent<SimData>().difficulty_level[z] = 1;
                }
            }
            
            // We do not want the trial counter to increase from videos, so we just go ahead and continue by loading the Scene
            SceneManager.LoadScene("Rover");
            triggers.Write("Rover Task Started"); 
        }

    }

}
