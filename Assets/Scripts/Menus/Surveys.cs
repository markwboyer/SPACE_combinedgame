using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Surveys : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] buttons;
    public GameObject surveyText;

    GameObject surveyMenu;
    GameObject difficultyMenu;

    int screen = 0;
    int[] prevScreen = { -1, 0, 0, 2, 2, 4, 4 };

    // Used to know which return val does what
    int[] returnVals = { 0, 0, 0, 0 };

    void Start()
    {
        updateScreen(0);
        surveyMenu = GameObject.Find("MenuInterface/Surveys");
        surveyMenu.SetActive(false);
        difficultyMenu = GameObject.Find("MenuInterface/PerceivedDifficulty");
        difficultyMenu.SetActive(true);

    }

    public void getDifficulty(int difficulty)
    {
        GameObject.Find("Player").GetComponent<SimData>().perceivedDifficulty = difficulty;
        surveyMenu.SetActive(true);
        difficultyMenu.SetActive(false);

        int trial = GameObject.Find("Player").GetComponent<SimData>().trial;
        if (trial == 1 || trial == 5 || trial == 10)
        {
            GameObject.Find("Player").GetComponent<SimData>().perceivedDifficulty = difficulty;
            surveyMenu.SetActive(true);
            difficultyMenu.SetActive(false);
        }
        else
        {
            GameObject.Find("Player").GetComponent<SimData>().perceivedDifficulty = difficulty;
            GameObject.Find("Player").GetComponent<SimData>().bedford = -1;
            SceneManager.LoadScene("Feedback");
        }        

    }

    public void buttonAction(int action)
    {
        if (screen == 0)
        {
            switch (action)
            {
                case 0:
                    updateScreen(2);
                    break;
                case 1:
                    updateScreen(1);
                    break;
                default:
                    Debug.Log("Invalid selection!");
                    break;
            }

        }
        else if (screen == 2)
        {
            switch (action)
            {
                case 0:
                    updateScreen(4);
                    break;
                case 1:
                    updateScreen(3);
                    break;
                default:
                    Debug.Log("Invalid selection!");
                    break;
            }
        }
        else if (screen == 4)
        {
            switch (action)
            {
                case 0:
                    updateScreen(6);
                    break;
                case 1:
                    updateScreen(5);
                    break;
                default:
                    Debug.Log("Invalid selection!");
                    break;
            }
        }
        else
        {
            Debug.Log("Return value:" + returnVals[action]);
            GameObject.Find("Player").GetComponent<SimData>().bedford = returnVals[action];
            SceneManager.LoadScene("Feedback");
        }
    }

    public void goBack()
    {
        //Debug.Log("Screen #:" + screen + " Going back to:" + prevScreen[screen]);
        if(prevScreen[screen] == -1)
        {
            surveyMenu.SetActive(false);
            difficultyMenu.SetActive(true);
        }
        else
        {
            updateScreen(prevScreen[screen]);
        }

    }

    public void ContinueToFeedback()
    {
        SceneManager.LoadScene("Feedback");
    }

    // Sorry for the poor implementation :/
    void updateScreen(int newScreen)
    {
        screen = newScreen;
        int[] temp;
        switch(newScreen)
        {
            case 0:
                surveyText.transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Was it possible to complete the task?";
                buttons[0].SetActive(true);
                buttons[1].SetActive(true);
                buttons[0].GetComponentInChildren<Text>().text = "Yes";
                buttons[1].GetComponentInChildren<Text>().text = "No";
                buttons[2].SetActive(false);
                buttons[3].SetActive(false);
                break;
            case 1:
                surveyText.transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Please select the most applicable choice:";
                buttons[0].GetComponentInChildren<Text>().text = "Tasks abandoned. Crew member unable to apply sufficient effort.";
                temp = new int[] { 10, -1, -1, -1 };
                returnVals = temp;
                buttons[1].SetActive(false);
                buttons[3].SetActive(false);
                break;
            case 2:
                surveyText.transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Was workload tolerable for the task?";
                buttons[0].SetActive(true);
                buttons[1].SetActive(true);
                buttons[0].GetComponentInChildren<Text>().text = "Yes";
                buttons[1].GetComponentInChildren<Text>().text = "No";
                buttons[2].SetActive(false);
                buttons[3].SetActive(false);
                break;
            case 3:
                surveyText.transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Please select the most applicable choice:";
                buttons[0].SetActive(true);
                buttons[1].SetActive(true);
                buttons[0].GetComponentInChildren<Text>().text = "Very high workload with almost no spare capacity. Difficulty in maintaining level of effort.";
                buttons[1].GetComponentInChildren<Text>().text = "Extremely high workload. No spare capacity. Serious doubts as to ability to maintain level of effort.";
                temp = new int[] { 8, 9, -1, -1 };
                returnVals = temp;
                buttons[2].SetActive(false);
                buttons[3].SetActive(false);
                break;
            case 4:
                surveyText.transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Was workload satisfactory without reduction?";
                buttons[0].SetActive(true);
                buttons[1].SetActive(true);
                buttons[0].GetComponentInChildren<Text>().text = "Yes";
                buttons[1].GetComponentInChildren<Text>().text = "No";
                buttons[2].SetActive(false);
                buttons[3].SetActive(false);
                break;
            case 5:
                surveyText.transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Please select the most applicable choice:";
                buttons[0].SetActive(true);
                buttons[1].SetActive(true);
                buttons[2].SetActive(true);
                buttons[3].SetActive(true);
                buttons[0].GetComponentInChildren<Text>().text = "Insufficient spare capacity for easy attention to additonal tasks.";
                buttons[1].GetComponentInChildren<Text>().text = "Reduced spare capacity. Additional tasks cannot be given the desired amount of attention.";
                buttons[2].GetComponentInChildren<Text>().text = "Little spare capacity. Level of effort allows little attention to additional tasks.";
                buttons[3].GetComponentInChildren<Text>().text = "Very little spare capacity, but maintenance of effort in the primary task not in question.";
                temp = new int[] { 4, 5, 6, 7 };
                returnVals = temp;
                break;
            case 6:
                surveyText.transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Please select the most applicable choice:";
                buttons[0].SetActive(true);
                buttons[1].SetActive(true);
                buttons[2].SetActive(true);
                buttons[0].GetComponentInChildren<Text>().text = "Workload insignificant.";
                buttons[1].GetComponentInChildren<Text>().text = "Workload low.";
                buttons[2].GetComponentInChildren<Text>().text = "Enough spare capacity for all desirable tasks.";
                temp = new int[] { 1, 2, 3, -1 };
                returnVals = temp;
                buttons[3].SetActive(false);
                break;

            default:
                Debug.Log("Invalid screen!");
                break;
        }
    }
}