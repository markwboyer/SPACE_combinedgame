using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Assets.LSL4Unity.Scripts;

public class EndRobot : MonoBehaviour
{
    GameObject roverCam;
    GameObject rover;
    GameObject ObservationCam;
    GameObject navMapPFD;
    GameObject mainDisplayPFD;
    GameObject canvas;
    PandaController.PandaController robot_controller;
    SimData playerData;
    bool observation_started = false;
    PFDScript pfd;

    public Material navMat;

    //LSL Markers
    private LSLMarkerStream triggers; //For 

    void Awake()
    {
        //Find LSL Stream
        triggers = FindObjectOfType<LSLMarkerStream>();
    }

    private void Start()
    {
        roverCam = GameObject.Find("Terrain Generation/Topo Camera");
        rover = GameObject.Find("rover");
        ObservationCam = GameObject.Find("ObservationMap/ObservationCam");
        navMapPFD = GameObject.Find("rover/CenterControlPanel/NavigationMapPFD");
        mainDisplayPFD = GameObject.Find("rover/CenterControlPanel/Display");
        robot_controller = GameObject.Find("panda").GetComponent<PandaController.PandaController>();
        playerData = GameObject.Find("Player").GetComponent<SimData>();
        canvas = GameObject.Find("FadeBlackImg");
        pfd = GameObject.Find("PFD_Upper").GetComponent<PFDScript>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        // this may be called twice depending on collision checking. Check if we've already started
        if (collision.gameObject.name == "Construction" && !observation_started)
        {
            //StartObservationTask();
            //triggers.Write("Observation Task Started");

            Debug.Log("reached end of map");
        }
    }

    public void StartObservationTask()
    {
        Debug.Log("Start of observation task");
        roverCam.SetActive(false);
        ObservationCam.SetActive(true);
        GameObject observation_map = GameObject.Find("ObservationMap");
        observation_map.GetComponent<ObservationController>().enabled = true;
        observation_map.GetComponent<PlayerInput>().enabled = true;
        navMapPFD.SetActive(false);
        mainDisplayPFD.GetComponent<MeshRenderer>().material = navMat;

        ObsTimer obs_timer = GameObject.Find("ObservationTimer").GetComponent<ObsTimer>();
        obs_timer.StartTimer();

        // Disable PFD script to save on system resources
        GameObject.Find("PFD_Upper").GetComponent<PFDScript>().enabled = false;
        observation_started = true;
        pfd.mp_text.GetComponent<Text>().text = "Observation";
        pfd.phase_stopwatch.Reset();
        pfd.phase_stopwatch.Start();
    }

    // allow the user to drive to the observation point
    public void EndRobotTask()
    {
        Rigidbody rb = rover.GetComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.None;
        triggers.Write("Robotic Arm Task Ended");
        StartCoroutine(canvas.GetComponent<Fade>().FadeOutIn(TransportRover, 2.0f));
    }

    private void TransportRover()
    {
        rover.transform.position = new Vector3(-345.92f, 12.5f, -5.25f);
        rover.transform.rotation = Quaternion.Euler(0,-90.0f,0);
    }
}
