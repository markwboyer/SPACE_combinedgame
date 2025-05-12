using System.Collections.Generic;
using UnityEngine;

public class BuildMap : MonoBehaviour
{
    public RoverDriving roverDriving;
    public Vector3 CtrDispSize = new Vector3(2f, 2f, 2f);   // size the map display
    public Vector3 RoverStatusDispSize = new Vector3(1f, 1f, 1f);   // size the rover status display
    public float roverDispMoveX = 10.0f;    // Rover display right of center display right edge

    public int NumBoulders = 10;  // number of boulders in the game
    public int NumHazards = 10;    // number of hazards in the game
    public float maxScale = 1.0f;  // max scaling factor for hazards
    public float minScale = 1.0f;  // min scaling size for hazards
    private float hazardscale;       // final scale of hazard

    public int NumGoals = 4;        // number of goals in the game to begin

    public int minX = -100;
    public int maxX = 100;          // max size of grid in X direction from zero
    public int minY = -100;
    public int maxY = 100;          // max size of grid in Y direction
    public int boulderBuffer = 5;   // buffer from edge of map for boulders
    public float cellSize = 1.0f;   // size of grid cells
    private bool isBoulder = false; // Initialize bool variables for boulder and hazard detection
    private bool isHazard = false;
    public float avoidDist = 7.0f;  // Distance to avoid boulders and hazards
    //public float hazardCost = 2.0f;
    float hazardWeight = 0.0f;

    public GameObject boulder;
    public GameObject hazard;
    public GameObject goal;
    public List<GameObject> spawnedGoals = new List<GameObject>();
    
    // Trying out some new goal logging methods
    public List<Goal> goalList = new List<Goal>();

    public GameObject CenterDisplay;
    public GameObject InstCtrDisplay;

    public GameObject RoverStatusDisplay;
    public GameObject InstRoverStatusDisplay;

    // Transform to display properly
    public Transform mouse2DWorldParent;

    public Vector2[] boulderLocations;
    public Vector2[] hazardLocations;
    public float[] hazardScaleArray;
    public Vector2[] goalLocations;
    //public Vector3[] goalLocations3D;

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    public Transform player;
    public float nodeRadius;
    public Node_mouse[,] roverGrid;
    public List<Node_mouse> path;  // The list to store the A* path


    // Start is called before the first frame update
    void Start()
    {
        // Instantiate CtrDisplay
        //InstCtrDisplay = Instantiate(CenterDisplay, new Vector3(0, 0, 0), Quaternion.identity);
        //ResizeCtrDisp(InstCtrDisplay, CtrDispSize);

        // Instantiate CtrDisplay
        //InstRoverStatusDisplay = Instantiate(RoverStatusDisplay, new Vector3(0, 0, 0), Quaternion.identity);
        //ResizeRoverDisp(InstRoverStatusDisplay, RoverStatusDispSize);

        // Set the parent of the instantiated object to the mouse2DWorldParent
        GameObject container = GameObject.Find("Mouse2DWorld");
        if (container != null)
        {
            mouse2DWorldParent = container.transform;
            Debug.Log("Mouse2DWorld container found in scene!");
        }
        else
        {
            Debug.LogError("Mouse2DWorld container not found in scene!");
        }

        hazardLocations = new Vector2[NumHazards];  // Initialize the array to hold locations of hazards
        hazardScaleArray = new float[NumHazards];  // Initialize the array to hold scale of hazards
        SpawnHazards();
        boulderLocations = new Vector2[NumBoulders];  // Initialize the array to hold locations
        SpawnBoulders();
        //Debug.Log("Size boulderLocations is " + boulderLocations.GetLength(1));

        //Debug.Log("goalLocations length is " + goalLocations.Length);
        SpawnGoals(NumGoals);

        //Initialize the grid array
        nodeDiameter = nodeRadius * 2;
        gridSizeX = maxX - minX;
        gridSizeY = maxY - minY;
        roverGrid = new Node_mouse[gridSizeX, gridSizeY];
        CreateGrid();

    }



    //public float[,] CreateGrid(Vector2[] boulderLocations, Vector2[] hazardLocations, float cellSize)
    void CreateGrid()
    {
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                // Get world position of grid point
                Vector3 worldPosition = new Vector3(x * cellSize, y * cellSize, 0);
                //Debug.Log("World position is " + x * cellSize + " by " + y * cellSize);

                // Check if there's an boulder at this point
                bool isBoulder = CheckForBoulder(worldPosition, boulderLocations, avoidDist);

                // Check if boulder at grid point
                if (isBoulder == true)
                {
                    //Debug.Log("Boulder at grid location x,y: " + (x - maxX) + "," + (y - maxY));
                }


                // Check if there's a hazard at this point
                bool isHazard = CheckForHazard(worldPosition, hazardLocations, maxScale);

                // Set z-value based on if there's a boulder
                //roverGrid[x, y] = isHazard ? hazardCost : 0.0f;
                if (isHazard == true)
                {
                    hazardWeight = 1f;// roverDriving.defaultHazardCost;
                    //Debug.Log("Hazard at roverGrid location x,y: " + (x - maxX) + "," + (y - maxY) + " z " + grid[x, y]);
                    //Debug.Log("is boulder" + isBoulder +"worldpos" + worldPosition + " x " + x + " y " + y + " Hazard weight is: " + hazardWeight);
                }
                else { hazardWeight = 0; }
                //Debug.Log("is boulder" + isBoulder +"worldpos" + worldPosition + " x " + x + " y " + y + " Hazard weight is: " + hazardWeight);

                //Debug.Log("Grid position is " + (x - minX) + " by " + (y - minY));
                roverGrid[x - minX, y - minY] = new Node_mouse(isBoulder, worldPosition, x - minX, y - minY, hazardWeight);
                isBoulder = false;
                isHazard = false;
            }
        }
        //return roverGrid;
        //Debug.Log("Final roverGrid is " + roverGrid.GetLength(0) + " by " + roverGrid.GetLength(1));
    }
    public List<Node_mouse> GetNeighbors(Node_mouse node, int extraSteps)
    {
        List<Node_mouse> neighbors = new List<Node_mouse>();

        for (int x = -extraSteps; x <= extraSteps; x++)
        {
            for (int y = -extraSteps; y <= extraSteps; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(roverGrid[checkX, checkY]);
                    //Debug.Log("Found neighbor at " + checkX + "" + checkY );
                }
            }
        }

        return neighbors;
    }
    public Node_mouse GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - minX) / (maxX - minX);
        float percentY = (worldPosition.y - minY) / (maxY - minY);
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        //Debug.Log("pos is " + x + ", " + y);
        return roverGrid[x, y];
    }

    bool CheckForBoulder(Vector3 position, Vector2[] boulderLocations, float avoidDist)
    {
        //Vector2 pos2D = position;
        //Debug.Log("pos2D is "+ pos2D);
        //Debug.Log("boulderLocations exist " + boulderLocations[1]);
        for (int i = 0; i < NumBoulders; i++)
        {
            //Debug.Log("Boulder position is " + boulderLocations[i]);
            float bolddist = Vector2.Distance(position, boulderLocations[i]);
            //Debug.Log("Boulder distance is " + bolddist);
            if (bolddist <= avoidDist)
            {
                //Debug.Log("Factor Boulder exists at location " + position);
                return isBoulder = true;
            }
            else
            {
                isBoulder = false;
            }
        }
        return isBoulder;
    }
    bool CheckForHazard(Vector3 position, Vector2[] hazardLocations, float maxScale)
    {
        //Vector2 pos2D = position;

        for (int i = 0; i < NumHazards; i++)
        {
            float hazarddist = Vector2.Distance(position, hazardLocations[i]);
            float hazardscaleLocal = hazardScaleArray[i];
            //Debug.Log("Hazard distance is " + hazarddist + " scale is " + hazardscaleLocal);
            if (hazarddist <= hazardscaleLocal * 0.5f)
            {
                //Debug.Log("Hazard distance " + hazarddist + " scale is " + hazardscaleLocal);
                //Debug.Log("Factor Hazard at location " + position + " dist " + hazarddist + " scale " + hazardscaleLocal);
                return isHazard = true;
            }
            else { isHazard = false; }
        }
        return isHazard;
    }

    
    public Vector2[] SpawnGoals(int NumGoals)
    {
        List<float> yPositions = GenerateSpreadOutYPositions(NumGoals, minY, maxY);
        ShuffleList(yPositions);

        goalLocations = new Vector2[NumGoals];  // Initialize the array to hold locations
        for (int i = 0; i < NumGoals; i++)
        {
            // Generate goals in pseudorandom position within the specified 2D area (spread evenly on x axis)
            float xmin = minX + ((i * (maxX - minX)) / NumGoals);
            float xmax = minX + (((i + 1) * (maxX - minX)) / NumGoals);
            float randomX = Random.Range(xmin + 5, xmax - 5);
            //float randomY = Random.Range(minY, maxY);
            float randomY = yPositions[i];
            Vector2 randomPosition = new Vector2(randomX, randomY);
            bool isBoulder = CheckForBoulder(randomPosition, boulderLocations, avoidDist);
            //Debug.Log("Is boulder check 1 "+ isBoulder + " original pos " + randomX + " , " + randomY);
            // Ensure goal is not directly over a boulder
            while (isBoulder == true)
            {
                randomX = randomX + 5;
                randomY = randomY + 5;
                randomPosition.x = randomX;
                randomPosition.y = randomY;
                isBoulder = CheckForBoulder(randomPosition, boulderLocations, avoidDist);
                //Debug.Log("Goal moved to new pos x, y" + randomX + "," + randomY);
            }
            //Debug.Log("Goal x y are " + randomX + ", " + randomY);
            // Spawn the object at the random position
            GameObject newGoal = Instantiate(goal, new Vector3(randomX, randomY, 0f), Quaternion.identity);//, mouse2DWorldParent);
            SetLayerRecursively(newGoal, LayerMask.NameToLayer("MOUSELayer"));

            // Store new spawned goal
            spawnedGoals.Add(newGoal);

            // Get the GoalNumberText component and set the number
            GoalNumberText display = newGoal.GetComponent<GoalNumberText>();
            if (display != null)
            {
                display.SetGoalNumber(i); // Pass the current goal number
            }

            // Store the object's position in the array
            goalLocations[i] = newGoal.transform.position;
            //Debug.Log("Goal locations are " + goalLocations[i].ToString());

            // Add the goal to the list of goals
            goalList.Add(new Goal(i + roverDriving.goalCount, randomPosition));
        }
        
        //// Print object locations to console (optional)
        //foreach (Vector2 location in goalLocations)
        //{
        //    //Debug.Log("Goal Position: " + location);
        //}

        return goalLocations;
    }

    public List<Goal> GetGoalList()
    {
        return goalList;
    }

    List<float> GenerateSpreadOutYPositions(int count, float minY, float maxY)
    {
        List<float> positions = new List<float>();
        float interval = (maxY - minY) / count;

        for (int i = 0; i < count; i++)
        {
            float start = minY + (i * interval);
            float end = start + interval;
            float randomY = Random.Range(start, end); // Pick a random value within this interval
            positions.Add(randomY);
        }

        return positions;
    }

    void ShuffleList(List<float> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count);
            float temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void HideAllGoals()
    {
        // Deactivate each goal in the spawnedGoals list
        foreach (GameObject goal in spawnedGoals)
        {
            if (goal != null)
            {
                goal.SetActive(false);
            }
        }

        // Clear the list for the next set of goals
        spawnedGoals.Clear();
    }

    // Method to return the array of object locations
    public Vector2[] GetGoalLocations()
    {
        //Debug.Log("goalLocations returned");
        return goalLocations;
    }

    public Vector2[] SpawnHazards()
    {
        for (int i = 0; i < NumHazards; i++)
        {
            // Generate random position within the specified 2D area
            float randomX = Random.Range(minX * 0.9f, maxX * 0.9f);
            float randomY = Random.Range(minY * 0.9f, maxY * 0.9f);
            Vector2 randomPosition = new Vector2(randomX, randomY);
            //Debug.Log("Hazard x y are "+ randomX +", "+ randomY);

            // Spawn the object at the random position
            GameObject newHazard = Instantiate(hazard, new Vector3(randomX, randomY, 0f), Quaternion.identity);//, mouse2DWorldParent);
            SetLayerRecursively(newHazard, LayerMask.NameToLayer("MOUSELayer"));

            // Change scale of hazards
            float hazardscale = Random.Range(minScale, maxScale);
            //Debug.Log("Hazardscale is " + hazardscale);
            newHazard.transform.localScale = new Vector3(hazardscale, hazardscale, 1);
            //Debug.Log("Hazard Scale" + scale);

            // Store the object's position in the array
            hazardLocations[i] = newHazard.transform.position;
            hazardScaleArray[i] = hazardscale;
        }

        // Print object locations to console (optional)
        foreach (Vector2 location in hazardLocations)
        {
            //Debug.Log("Hazard Position: " + location);
        }
        return hazardLocations;
    }

    // Method to return the array of object locations
    public Vector2[] GetHazardLocations()
    {
        return hazardLocations;
    }

    // Method to return array of hazard scales
    public float[] GetHazardScales()
    {
        return hazardScaleArray;
    }

    public Vector2[] SpawnBoulders()
    {
        for (int i = 0; i < NumBoulders; i++)
        {
            // Generate random position within the specified 2D area
            int randomX = Random.Range(minX + boulderBuffer, maxX - boulderBuffer);
            int randomY = Random.Range(minY + boulderBuffer, maxY - boulderBuffer);
            Vector2 randomPosition = new Vector3(randomX, randomY);

            // Spawn the object at the random position
            GameObject newBoulder = Instantiate(boulder, new Vector3( randomX, randomY,0f), Quaternion.identity);//, mouse2DWorldParent);
            SetLayerRecursively(newBoulder, LayerMask.NameToLayer("MOUSELayer"));
            //Debug.Log("Boulder rotation: " + newBoulder.transform.rotation.eulerAngles);

            // Store the object's position in the array
            boulderLocations[i] = newBoulder.transform.position;
        }

        // Print object locations to console (optional)
        foreach (Vector2 location in boulderLocations)
        {
            //Debug.Log("Boulder Position: " + location);
        }

        return boulderLocations;
    }

    // Method to return the array of object locations
    public Vector2[] GetBoulderLocations()
    {
        return boulderLocations;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

}
