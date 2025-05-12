using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.LSL4Unity.Scripts;

public class ObservationController : MonoBehaviour
{
    public Texture2D cursor_tex;
    public Texture2D ROSI_tex;
    public Texture2D rock_tex;

    GameObject cursor;
    GameObject terrain;
    SimData playerData;

    float transform_adjust = Mathf.Sqrt(615)/2;
    int rosiCtr = 0;
    int numRosi = 0;
    int score = 0; // stores the score (# ROSIs selected - # incorrectly selected)
    int selectedRosiCtr = 0;

    Vector2 contextUpdate;

    List<GameObject> rockSprites = new List<GameObject>();
    public Color32 rockChangeColor;

    //LSL Markers
    private LSLMarkerStream triggers; //For 

    void Awake()
    {
        //Find LSL Stream
        triggers = FindObjectOfType<LSLMarkerStream>();
    }

    void Start()
    {
        // Not sure how code worked before line below was present
        // Either way, playerData needs to be populated before anything else happens
        playerData = GameObject.Find("Player").GetComponent<SimData>();

        cursor =  new GameObject(string.Format("ObservationCursor"));
        Sprite cursor_sprite = Sprite.Create(cursor_tex, new Rect(0.0f,0.0f, cursor_tex.width,cursor_tex.height), Vector2.zero);
        cursor.AddComponent<Image>();
        cursor.GetComponent<Image>().sprite = cursor_sprite;
        cursor.transform.SetParent(this.transform, false);
        cursor.GetComponent<RectTransform>().sizeDelta = new Vector2(cursor_tex.width, cursor_tex.height);
        cursor.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        cursor.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        cursor.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        cursor.transform.localPosition = new Vector2(0, 0);

        // this.enabled = false;
    }

    public void cursorInput(InputAction.CallbackContext context)
    {
        contextUpdate =  context.ReadValue<Vector2>();
    }

    public void populateMap(List<Rock> rocks)
    {
        int ctr = 0;
        foreach (Rock rockToAdd in rocks)
        {
             // Debug.Log("Adding rock with x z:" + rockToAdd.position.x + " " + rockToAdd.position.z);
            GameObject modRock = addRock((rockToAdd.position.z+10)*2, -(rockToAdd.position.x+525)*2, ctr);
            if (rockToAdd.isRosi)
            {
                modRock.GetComponent<RockProperties>().isROSI = true;
                numRosi++;
            }
            else
            {
                modRock.GetComponent<RockProperties>().isROSI = false;
            }
            ctr++;
        }
    }

    GameObject addRock(float x, float y, int ctr)
    {
        GameObject rock =  new GameObject(string.Format("Mapped rock {0}", ctr));
        Sprite rock_sprite = Sprite.Create(rock_tex, new Rect(0.0f,0.0f, rock_tex.width, rock_tex.height), Vector2.zero);
        rock.AddComponent<Image>();
        rock.GetComponent<Image>().sprite = rock_sprite;
        rock.transform.SetParent(this.transform, false);
        rock.GetComponent<RectTransform>().sizeDelta = new Vector2(rock_tex.width, rock_tex.height);
        rock.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        rock.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        rock.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        rock.AddComponent<RockProperties>();

        rock.transform.localPosition = new Vector2(x, y);
        rockSprites.Add(rock);

        return rock;
    }

    public void addROSI(InputAction.CallbackContext context)
    {
        // Debug.Log(context.ReadValue<float>());
        if (context.ReadValue<float>() == 1 && rosiCtr < numRosi)
        {
            // Find closest landing site
            float dist = 99999;
            GameObject bestRock = null;            
            foreach (GameObject rockObject in rockSprites)
            {
                float compDist = Vector3.Distance(cursor.transform.position, rockObject.transform.position);
                if (compDist < dist)
                {
                    dist = compDist;
                    bestRock = rockObject;
                }
            }
            // Debug.Log("Best rock is: " + bestRock.name);
            // Rock found and clicked on
            if (dist < 25 && !bestRock.GetComponent<RockProperties>().isSelected)
            {
                triggers.Write("Rock Selected");
                bestRock.GetComponent<Image>().color = rockChangeColor; //new Color32(0,150,150,255);
                cursor.transform.localPosition = bestRock.transform.localPosition;
                bestRock.GetComponent<RockProperties>().isSelected = true;
                if (bestRock.GetComponent<RockProperties>().isROSI)
                {
                    selectedRosiCtr++;
                    score += 1;
                    Debug.Log("Correct!  ROSI's selected: " + selectedRosiCtr + "/" + numRosi + " score: " + score);
                    // Update player variables in case trial ends early (same in else clause below)
                    playerData.rockScore = score;
                    playerData.rockTarget = numRosi;
                }
                else
                {
                    Debug.Log("Incorrect selection!");
                    score -= 1;
                    playerData.rockScore = score;
                    playerData.rockTarget = numRosi;
                }
                rosiCtr++;
            }
        }
        else if (rosiCtr >= numRosi) // Finished the observation task (TODO add logic to fade screen to black if this happens)
        {
            playerData = GameObject.Find("Player").GetComponent<SimData>();
            playerData.rockScore = score;
            playerData.rockTarget = numRosi;
            Debug.Log("Out of selections!  Final score: " + selectedRosiCtr + "/" + numRosi);
            triggers.Write("Observation Task Ended");
            SceneManager.LoadScene("Surveys");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Clamp the x and y position such that it never goes off screen
        float newX = Mathf.Clamp((cursor.transform.localPosition.x + contextUpdate.x),-159,178);
        float newY = Mathf.Clamp((cursor.transform.localPosition.y + contextUpdate.y),-327,235);
        cursor.transform.localPosition = new Vector2(newX, newY);

        //cursor.transform.localPosition = new Vector2(cursor.transform.localPosition.x + contextUpdate.x, cursor.transform.localPosition.y + contextUpdate.y);
        //Debug.Log("x y:" + cursor.transform.localPosition.x + contextUpdate.x + " " + cursor.transform.localPosition.y + contextUpdate.y);
    }
}
