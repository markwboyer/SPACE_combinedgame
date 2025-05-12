using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Assets.LSL4Unity.Scripts;

public class EndNav : MonoBehaviour
{
    GameObject roverCam;
    GameObject rover;
    GameObject ObservationCam;
    GameObject navMapPFD;
    GameObject mainDisplayPFD;
    PandaController.PandaController robot_controller;
    GameObject observation_map;
    GameObject canvas;
    PFDScript pfd;
    SimData playerData;

    public Material navMat;

    //LSL Markers
    private LSLMarkerStream triggers;

    public Feedback feedback;

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
        ObservationCam.SetActive(false);
        observation_map = GameObject.Find("ObservationMap");
        navMapPFD = GameObject.Find("rover/CenterControlPanel/NavigationMapPFD");
        mainDisplayPFD = GameObject.Find("rover/CenterControlPanel/Display");
        robot_controller = GameObject.Find("panda").GetComponent<PandaController.PandaController>();
        playerData = GameObject.Find("Player").GetComponent<SimData>();
        canvas = GameObject.Find("FadeBlackImg");
        pfd = GameObject.Find("PFD_Upper").GetComponent<PFDScript>();

        // Initialize the Feedback instance
        feedback = FindObjectOfType<Feedback>();
        if (feedback == null)
        {
            Debug.LogError("Feedback component not found in the scene.");
        }
    }

    public void EndGameAndSendData()
    {
        triggers.Write("Game Ended");
        Debug.Log("End of game");
        observation_map.GetComponent<ObservationController>().enabled = false;
        observation_map.GetComponent<PlayerInput>().enabled = false;
        playerData.navPerformance = rover.GetComponent<PowerController>().kwh;
        Debug.LogFormat("Fuel consumed: {0}", playerData.navPerformance);
        rover.GetComponent<PowerController>().enabled = false;
        rover.transform.position = new Vector3(-328.1956f, 12.24437f, -27.69985f);
        rover.transform.rotation = Quaternion.Euler(0.0f, -90.031f, 0.0f);
        Rigidbody rb = rover.GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (feedback != null)
        {
            feedback.SendDataToServer();
        }
        else
        {
            Debug.LogError("Feedback instance is not initialized.");
        }
    }

    private void Fade()
    {
        StartCoroutine(canvas.GetComponent<Fade>().FadeOutIn(EndGameAndSendData));
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.name == "Construction")
        {
            Fade();
        }
    }

    public void StartRobotTask()
    {
        triggers.Write("Robotic Arm Task Started");
        Debug.Log("Start of robot task");
        observation_map.GetComponent<ObservationController>().enabled = false;
        observation_map.GetComponent<PlayerInput>().enabled = false;
        playerData.navPerformance = rover.GetComponent<PowerController>().kwh;
        Debug.LogFormat("Fuel consumed: {0}", playerData.navPerformance);
        rover.GetComponent<PowerController>().enabled = false;
        rover.transform.position = new Vector3(-328.1956f, 12.24437f, -27.69985f);
        rover.transform.rotation = Quaternion.Euler(0.0f, -90.031f, 0.0f);
        Rigidbody rb = rover.GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        robot_controller.should_start = true;
        pfd.mp_text.GetComponent<Text>().text = "Robotics";
        pfd.phase_stopwatch.Reset();
        pfd.phase_stopwatch.Start();
    }
}
