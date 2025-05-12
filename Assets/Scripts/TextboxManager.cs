using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
//using TMPro.Examples;
//using System.Diagnostics;

public class TextboxManager : MonoBehaviour
{
    public CautionTextbox[] textboxes; // Assign the 15 textboxes in the Inspector
    public List<string> logEntries = new List<string>();   // List to log activations/deactivations
    public int[] textboxesStatus;
    public SAGATPopupManager popupManager;
    public RoverDriving roverDriving;

    // Probability list for how cautions and warnings will be activated/deactivated
    // None -> caution = 0.2
    // None -> warning = 0.02
    // Caution -> warning = 0.5
    // Caution -> none = 0.2
    // Warning -> none = 0.1
    // Warning -> caution = 0.2
    public float pk_none2caut = 0.2f;
    public float pk_none2warn = 0.2f;
    public float pk_caut2warn = 0.5f;
    public float pk_caut2none = 0.2f;
    public float pk_warn2none = 0.01f;
    public float pk_warn2caut = 0.02f;

    public float activateStartTime = 10f;
    public float activateInterval = 15f;
    public float deactivateStartTime = 12f;
    public float deactivateInterval = 18f;

    public bool isRed = false;
    public bool roverDriveWarning = false;
    public bool roverSciKitWarning = false;
    float presetGoalFactor = 100f;
    private void Start()
    {
        // Call this method on an interval or when needed to randomly activate
        InvokeRepeating("ActivateRandomTextbox", activateStartTime, activateInterval); // Adjust time as needed
        InvokeRepeating("DeactivateRandomTextbox", deactivateStartTime, deactivateInterval);  // Adjust times as desired
        textboxesStatus = new int[textboxes.Length];  // initialize status array
        for (int i = 0; i < textboxesStatus.Length; i++)
        {
            textboxesStatus[i] = 0;
        }

        // Loop through each textbox and add a click listener
        foreach (CautionTextbox textbox in textboxes)
        {
            // Make sure each textbox has a Button component or equivalent click listener
            Button button = textbox.GetComponent<Button>();
            if (button != null)
            {
                // Use a lambda to capture the textbox in the loop
                button.onClick.AddListener(() => StartCoroutine(ClickAndDeactivate(textbox)));
            }
        }
        presetGoalFactor = roverDriving.goalFactor;  //in case any changes made

    }

    //void Update()
    //{
    //    //if (roverDriveWarning) { roverDriving.isMoving = false; }
    //    //if (roverSciKitWarning) { roverDriving.goalFactor = 0; }
    //}

    // Method to pause the InvokeRepeating calls
    public void PauseForSagatPopup()
    {
        if (popupManager.SAGATrunning)
        {
            CancelInvoke("ActivateRandomTextbox");
            CancelInvoke("DeactivateRandomTextbox");

        }
    }

    // Method to resume the InvokeRepeating calls
    public void ResumeAfterSagatPopup()
    {
        if (!popupManager.SAGATrunning)
        {
            InvokeRepeating("ActivateRandomTextbox", activateStartTime, activateInterval);
            InvokeRepeating("DeactivateRandomTextbox", deactivateStartTime, deactivateInterval);

        }
    }

    private void ActivateRandomTextbox()
    {
        int randomIndex = Random.Range(0, textboxes.Length);
        //Debug.Log("Caution panel number " +  randomIndex);
        //Debug.Log("Activate Random index = " + randomIndex + " status is " + textboxesStatus[randomIndex]);

        if (textboxesStatus[randomIndex] == 0) // textbox currently in NONE status
        {
            bool activateWarning = Random.value < pk_none2warn;
            if (activateWarning)
            {
                isRed = true;
                textboxes[randomIndex].ActivateTextbox(isRed);   // Set textbox to red WARNING status
                textboxesStatus[randomIndex] = 2;  // 2 = red warning
                if (randomIndex < 11)
                {
                    roverDriveWarning = true;
                }
                else
                {
                    roverSciKitWarning = true;
                    roverDriving.goalFactor = 0;
                }
            }

            if (!activateWarning && Random.value < pk_none2caut)
            {
                isRed = false;
                textboxes[randomIndex].ActivateTextbox(false);  // set textbox to yellow CAUTION status
                textboxesStatus[randomIndex] = 1;  // 1 = yellow caution, 0 = default (none)
            }




            // Log the activation event
            string logEntry = $"{Time.time}, {(isRed ? "Red" : "Yellow")}, '{textboxes[randomIndex].GetText()}'";
            logEntries.Add(logEntry);
            //Debug.Log(logEntry); // Optional: print to console for testing
        }
        if (textboxesStatus[randomIndex] == 1)
        {
            //Debug.Log("Random index in caution already is = " + randomIndex + " status is " +  textboxesStatus[randomIndex]);
            if (Random.value < pk_caut2warn)
            {
                isRed = true;
                textboxes[randomIndex].ActivateTextbox(isRed);   // Set textbox to red WARNING status
                textboxesStatus[randomIndex] = 2;  // 2 = red warning
                if (randomIndex < 11)
                {
                    roverDriveWarning = true;

                }
                else
                {
                    roverSciKitWarning = true;
                    roverDriving.goalFactor = 0;

                }
            }
            // Log the activation event
            string logEntry = $"{Time.time}, {(isRed ? "Red" : "Yellow")}, '{textboxes[randomIndex].GetText()}'";
            logEntries.Add(logEntry);
        }

    }

    // Coroutine to change color to green, wait 5 seconds, then run the deactivation process
    private IEnumerator ClickAndDeactivate(CautionTextbox textbox)
    {
        // Check if the current color is red
        int tbindex = System.Array.IndexOf(textboxes, textbox);
        //Debug.Log("Index of clicked box is " + tbindex);
        if (textboxesStatus[tbindex] != 2)
        {
            //Debug.Log("clicked on box that wasn't red");
            yield break; // Exit the coroutine if the color is not red
        }

        // Set the color to green
        //textboxes[tbindex].GetComponentInChildren<Text>().color = Color.green;
        //textboxes[tbindex].GetComponentInChildren<Image>().color = Color.gray;

        // Wait for 5 seconds
        //yield return new WaitForSeconds(5);

        roverDriving.LogButtonClick(textboxes[tbindex].GetText());

        // Deactivate the textbox according to its current status
        DeactivateTextboxAfterDelay(tbindex);
    }

    // Method to deactivate a specific textbox based on its status and random probability
    private void DeactivateTextboxAfterDelay(int tbindex)
    {
        if (tbindex < 0) return; // Safety check: make sure the textbox is found in the array
        textboxes[tbindex].DeactivateTextbox();
        textboxesStatus[tbindex] = 0;
        string logEntry = $"{Time.time}, Deactivated warning, '{textboxes[tbindex].GetText()}'";
        //Debug.Log(logEntry);
        logEntries.Add(logEntry);
        ClearWarnings();
    }

    private void DeactivateRandomTextbox()
    {
        //Debug.Log("Deactivating random textbox");

        if (textboxes.Length != textboxesStatus.Length)
        {
            Debug.LogError("Mismatched array lengths: textboxes and textboxesStatus");
            return;
        }

        int statusCheckSum = 0;
        for (int i = 0; i < textboxes.Length; i++)
        {
            statusCheckSum += textboxesStatus[i];
            if (textboxesStatus[i] == 1) // deactivating cautions
            {
                //Debug.Log("Found caution textbox number " + i);
                if (Random.value < pk_caut2none)
                {
                    // Log the deactivation event before deactivating
                    string logEntry = $"{Time.time}, Deactivated caution, '{textboxes[i].GetText()}'";
                    logEntries.Add(logEntry);
                    textboxes[i].DeactivateTextbox();
                    textboxesStatus[i] = 0;
                    //Debug.Log("Deactivating caution textbox number " + i);
                }

            }
            if (textboxesStatus[i] == 2)
            {
                //Debug.Log("Found warning textbox number " + i);
                if (Random.value < pk_warn2none)
                {
                    // Log the deactivation event before deactivating
                    string logEntry = $"{Time.time}, Deactivated warning, '{textboxes[i].GetText()}'";
                    logEntries.Add(logEntry);
                    textboxes[i].DeactivateTextbox();
                    textboxesStatus[i] = 0;
                }
                if (Random.value < pk_warn2caut)
                {
                    // Log the deactivation event before deactivating
                    string logEntry = $"{Time.time}, Deactivated warning to caution, '{textboxes[i].GetText()}'";
                    logEntries.Add(logEntry);
                    textboxes[i].DeactivateTextboxToCaution();
                    textboxesStatus[i] = 1;
                    //Debug.Log("Deactivating warning textbox number " + i);
                }
            }
        }
        //Debug.Log("Check sum of statuses: " + statusCheckSum);
        ClearWarnings();

    }

    private void ClearWarnings()
    {
        
        bool driveWarning = false;
        bool sciWarning = false;
        for (int i = 0; i < textboxesStatus.Length; i++)
        {
            if (textboxesStatus[i] == 2)  // checks to see if any warnings still remain
            {
                if (i < 11)  // first 11 are considered "rover driving warnings" and will stop the rover
                {
                    driveWarning = true;
                    roverDriveWarning = true;
                    //roverDriving.isMoving = false;
                }
                else if (i < 15) // last 4 are considered "science kit warnings" and will give zero reward at goals
                {
                    sciWarning = true;
                    //roverDriving.goalFactor = 0;
                }
            }
        }
        if (!driveWarning)
        {
            roverDriveWarning = false;
            //roverDriving.isMoving = true;
            //Debug.Log("isMoving should be set to move again "+roverDriving.isMoving);
        }
        if (!sciWarning)
        {
            roverDriving.goalFactor = presetGoalFactor;
            //Debug.Log("Goal factor reset to " + roverDriving.goalFactor);
        }
        //Debug.Log($"driveWarning: {driveWarning}, sciWarning: {sciWarning}, roverDriveWarning: {roverDriveWarning}, goalFactor: {roverDriving.goalFactor}");

    }
    public void ExportLogToCSV(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Time, Status, ButtonText"); // CSV header
                foreach (string entry in logEntries)
                {
                    writer.WriteLine(entry);
                }
            }
            Debug.Log($"Caution Log exported to {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to write log to CSV: {ex.Message}");
        }
    }
}
