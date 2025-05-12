using System.Collections.Generic;
using System.Linq;
//using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
//using TMPro.Examples;
//using TMPro.Examples;

public class AStarPathfinder : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    //public Node_mouse startNode;
    //public Node_mouse endNode;
    public BuildMap buildMap;   // Get roverGrid from building the map

    private List<Node_mouse> path;  // Final path to be output by AStar algorithm
    private List<Node_mouse> newpath;
    private List<Node_mouse> returnedpath;
    private List<Node_mouse> currentPath;
    private List<Node_mouse> starterpath;
    public RoverDriving RoverDriving;

    public Vector2[] goalLocations;  // A list of all goal locations
    public Dictionary<(int, int), List<Node_mouse>> pathsBetweenGoals;  // Store paths as pairs of goal indices
    private float[,] routeCosts; // Matrix to store the cost of routes
    public Dictionary<(int, int), List<Node_mouse>> lowCostPathsBetweenGoals;  // Store paths as pairs of goal indices
    private float[,] lowCostRouteCosts; // Matrix to store the cost of routes
    public Dictionary<(int, int), List<Node_mouse>> highCostPathsBetweenGoals;  // Store paths as pairs of goal indices
    private float[,] highCostRouteCosts; // Matrix to store the cost of routes

    //private Dictionary<(int, int), List<Node_mouse>> noExplanationPathsBetweenGoals;  // Store paths as pairs of goal indices

    // Trying out using dictionary for the route costs rather than matrices
    private Dictionary<(int, int), float> routeCostsDict_default; // Dictionary to store the cost of routes
    private Dictionary<(int, int), float> routeCostsDict_low; // Dictionary to store the cost of routes
    private Dictionary<(int, int), float> routeCostsDict_high; // Dictionary to store the cost of routes
    //private Dictionary<(int, int), float> routeCostsDict_noExplanation; // Dictionary to store the cost of routes

    //public float noExplanationHazardWeight;

    void Start()
    {
        if (buildMap == null)
        {
            Debug.LogError("buildMap reference is null. Please assign it in the Inspector.");
            return;
        }

        //goalLocations = buildMap.GetGoalLocations();

        //noExplanationHazardWeight = RoverDriving.defaultHazardCost * Random.Range(0.5f, 100000f); // Randomize the weight for no explanation paths
        //Debug.Log($"Hazard cost is: {noExplanationHazardWeight}");

        // Precompute paths between all goals
        PrecomputePathsBetweenGoals(buildMap.goalLocations, 0);
    }
    public void PrecomputePathsBetweenGoals(Vector2[] goalLocations, int goalsVisited)
    {

        int numGoals = goalLocations.Length;
        //Debug.Log("NumGoals now is " + numGoals);
        routeCosts = new float[numGoals + 1, numGoals + 1];
        pathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();

        // Also do low and high cost paths for alternates
        lowCostRouteCosts = new float[numGoals + 1, numGoals + 1];
        lowCostPathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();
        highCostRouteCosts = new float[numGoals + 1, numGoals + 1];
        highCostPathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();

        // Initialize dictionaries for route costs
        routeCostsDict_default = new Dictionary<(int, int), float>();
        routeCostsDict_low = new Dictionary<(int, int), float>();
        routeCostsDict_high = new Dictionary<(int, int), float>();

        // Initialize dictionaries for no explanation paths
        //noExplanationPathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();
        //routeCostsDict_noExplanation = new Dictionary<(int, int), float>();


        for (int i = 0; i < numGoals; i++)
        {
            for (int j = i + 1; j < numGoals; j++)
            {
                int startIdx = i + goalsVisited;
                int endIdx = j + goalsVisited;

                //Debug.Log("Goal location " + i + " " + goalLocations[i] + " goal location  " + j + " " + goalLocations[j]);
                newpath = FindPath2(goalLocations[i], goalLocations[j], RoverDriving.defaultHazardCost);
                float cost = FindPathCost(newpath, RoverDriving.defaultHazardCost);
                pathsBetweenGoals[(startIdx, endIdx)] = newpath;
                List<Node_mouse> revpath = FindPath2(goalLocations[j], goalLocations[i], RoverDriving.defaultHazardCost);
                float revcost = FindPathCost(revpath, RoverDriving.defaultHazardCost);
                pathsBetweenGoals[(endIdx, startIdx)] = revpath;
                routeCosts[i, j] = cost;
                routeCosts[j, i] = revcost; // Symmetric

                


                List<Node_mouse> lowCostPath = FindPath2(goalLocations[i], goalLocations[j], 0.0f);

                float lowCostCost = FindPathCost(lowCostPath, 0f);
                lowCostPathsBetweenGoals[(startIdx, endIdx)] = lowCostPath;
                List<Node_mouse> lowCostPathRev = FindPath2(goalLocations[j], goalLocations[i], 0.0f);
                float lowCostCostRev = FindPathCost(lowCostPathRev, 0f);
                lowCostPathsBetweenGoals[(endIdx, startIdx)] = lowCostPathRev;
                lowCostRouteCosts[i, j] = lowCostCost;
                lowCostRouteCosts[j, i] = lowCostCostRev;

                
                List<Node_mouse> highCostPath = FindPath2(goalLocations[i], goalLocations[j], RoverDriving.highHazardCost);
                float highCostCost = FindPathCost(highCostPath, RoverDriving.highHazardCost);
                highCostPathsBetweenGoals[(startIdx, endIdx)] = highCostPath;
                List<Node_mouse> highCostPathRev = FindPath2(goalLocations[j], goalLocations[i], RoverDriving.highHazardCost);
                float highCostCostRev = FindPathCost(highCostPathRev, RoverDriving.highHazardCost);
                highCostPathsBetweenGoals[(endIdx, startIdx)] = highCostPathRev;
                highCostRouteCosts[i, j] = highCostCost;
                highCostRouteCosts[j, i] = highCostCostRev; // Symmetric

                // Dictionary for route costs
                routeCostsDict_default[(startIdx, endIdx)] = cost;
                routeCostsDict_default[(endIdx, startIdx)] = revcost;
                // Dictionary for low cost route costs
                routeCostsDict_low[(startIdx, endIdx)] = lowCostCost;
                routeCostsDict_low[(endIdx, startIdx)] = lowCostCostRev;
                // Dictionary for high cost route costs
                routeCostsDict_high[(startIdx, endIdx)] = highCostCost;
                routeCostsDict_high[(endIdx, startIdx)] = highCostCostRev;

                //Debug.Log($"From point {i} to {j} low cost: {lowCostCost} default cost {cost} high cost {highCostCost}");

                // Calculate for no explanation paths - modify the AT score weights slightly to make route similar to optimal but slightly different
                //if (RoverDriving.selectedConfig == "Condition B")
                //{
                //    //Debug.Log("Condition B main paths");
                //    List<Node_mouse> newpath_noExplanation = FindPath3(goalLocations[i], goalLocations[j], noExplanationHazardWeight);  // Modify the hazard cost
                //    float cost_noExplanation = FindPathCost(newpath_noExplanation, noExplanationHazardWeight);
                //    noExplanationPathsBetweenGoals[(startIdx, endIdx)] = newpath_noExplanation;
                //    List<Node_mouse> revpath_noExplanation = FindPath3(goalLocations[j], goalLocations[i], noExplanationHazardWeight);
                //    float revcost_noExplanation = FindPathCost(revpath_noExplanation, noExplanationHazardWeight);
                //    noExplanationPathsBetweenGoals[(endIdx, startIdx)] = revpath_noExplanation;
                //    routeCostsDict_noExplanation[(startIdx, endIdx)] = cost_noExplanation;
                //    routeCostsDict_noExplanation[(endIdx, startIdx)] = revcost_noExplanation;
                //}
                


                //Debug.Log("Low costs");
                //DisplayRouteCostsMatrix(lowCostRouteCosts);
                //Debug.Log("default costs");
                //DisplayRouteCostsMatrix(routeCosts);
                //Debug.Log("high costs");
                //DisplayRouteCostsMatrix(highCostRouteCosts);

            }
        }
    }

    public void PreComputeFinalPaths(Vector2[] goalLocations, int goalsVisited)
    {
        int numGoals = goalLocations.Length;
        Debug.Log("NumGoals now is " + numGoals);
        routeCosts = new float[numGoals + 1, numGoals + 1];
        //pathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();
        for (int i = 0; i < numGoals; i++)
        {
            for (int j = i + 1; j < numGoals; j++)
            {
                int startIdx = i + goalsVisited;
                int endIdx = j + goalsVisited;

                

                //Debug.Log("Goal location " + i + " " + goalLocations[i] + " goal location  " + j + " " + goalLocations[j]);
                newpath = FindPath2(goalLocations[i], goalLocations[j], RoverDriving.defaultHazardCost);
                float cost = FindPathCost(newpath, RoverDriving.defaultHazardCost);
                pathsBetweenGoals[(startIdx, endIdx)] = newpath;
                List<Node_mouse> revpath = FindPath2(goalLocations[j], goalLocations[i], RoverDriving.defaultHazardCost);
                float revcost = FindPathCost(revpath, RoverDriving.defaultHazardCost);
                pathsBetweenGoals[(endIdx, startIdx)] = revpath;
                routeCosts[i, j] = cost;
                routeCosts[j, i] = revcost; // Symmetric
                // Dictionary for route costs
                routeCostsDict_default[(startIdx, endIdx)] = cost;
                routeCostsDict_default[(endIdx, startIdx)] = revcost;

                //// Calculate for no explanation paths - modify the AT score weights slightly (using FindPath3) to make route similar to optimal but slightly different
                //if (RoverDriving.selectedConfig == "Condition B")
                //{
                //    //Debug.Log("Condition B main paths");
                //    List<Node_mouse> newpath_noExplanation = FindPath3(goalLocations[i], goalLocations[j], noExplanationHazardWeight);
                //    float cost_noExplanation = FindPathCost(newpath_noExplanation, noExplanationHazardWeight);
                //    noExplanationPathsBetweenGoals[(startIdx, endIdx)] = newpath_noExplanation;
                //    List<Node_mouse> revpath_noExplanation = FindPath3(goalLocations[j], goalLocations[i], noExplanationHazardWeight);
                //    float revcost_noExplanation = FindPathCost(revpath_noExplanation, noExplanationHazardWeight);
                //    noExplanationPathsBetweenGoals[(endIdx, startIdx)] = revpath_noExplanation;
                //    routeCostsDict_noExplanation[(startIdx, endIdx)] = cost_noExplanation;
                //    routeCostsDict_noExplanation[(endIdx, startIdx)] = revcost_noExplanation;
                //}

                //Debug.Log("start index: " + startIdx + " end index: " + endIdx + " cost " + cost);

            }
        }
    }
    //public void PrecomputePathsBetweenGoals2(Vector2[] goalLocations)
    //{

    //    int numGoals = goalLocations.Length;
    //    //Debug.Log("NumGoals now is " + numGoals);
    //    routeCosts = new float[numGoals + 1, numGoals + 1];
    //    pathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();

    //    // Also do low and high cost paths for alternates
    //    lowCostRouteCosts = new float[numGoals + 1, numGoals + 1];
    //    lowCostPathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();
    //    highCostRouteCosts = new float[numGoals + 1, numGoals + 1];
    //    highCostPathsBetweenGoals = new Dictionary<(int, int), List<Node_mouse>>();
    //    for (int i = 0; i < numGoals; i++)
    //    {
    //        for (int j = i + 1; j < numGoals; j++)
    //        {
    //            //Debug.Log("Goal location " + i + " " + goalLocations[i] + " goal location  " + j + " " + goalLocations[j]);
    //            newpath = FindPath2(goalLocations[i], goalLocations[j], RoverDriving.defaultHazardCost);
    //            float cost = FindPathCost(newpath, RoverDriving.defaultHazardCost);
    //            pathsBetweenGoals[(i, j)] = newpath;
    //            List<Node_mouse> revpath = FindPath2(goalLocations[j], goalLocations[i], RoverDriving.defaultHazardCost);
    //            float revcost = FindPathCost(revpath, RoverDriving.defaultHazardCost);
    //            pathsBetweenGoals[(j, i)] = revpath;
    //            routeCosts[i, j] = cost;
    //            routeCosts[j, i] = revcost; // Symmetric




    //            List<Node_mouse> lowCostPath = FindPath2(goalLocations[i], goalLocations[j], 0.0f);

    //            float lowCostCost = FindPathCost(lowCostPath, 0f);
    //            lowCostPathsBetweenGoals[(i, j)] = lowCostPath;
    //            List<Node_mouse> lowCostPathRev = FindPath2(goalLocations[j], goalLocations[i], 0.0f);


    //            float lowCostCostRev = FindPathCost(lowCostPathRev, 0f);
    //            lowCostPathsBetweenGoals[(j, i)] = lowCostPathRev;
    //            lowCostRouteCosts[i, j] = lowCostCost;
    //            lowCostRouteCosts[j, i] = lowCostCostRev;

    //            List<Node_mouse> highCostPath = FindPath2(goalLocations[i], goalLocations[j], RoverDriving.highHazardCost);
    //            float highCostCost = FindPathCost(highCostPath, RoverDriving.highHazardCost);
    //            highCostPathsBetweenGoals[(i, j)] = highCostPath;
    //            List<Node_mouse> highCostPathRev = FindPath2(goalLocations[j], goalLocations[i], RoverDriving.highHazardCost);
    //            float highCostCostRev = FindPathCost(highCostPathRev, RoverDriving.highHazardCost);
    //            highCostPathsBetweenGoals[(j, i)] = highCostPathRev;
    //            highCostRouteCosts[i, j] = highCostCost;
    //            highCostRouteCosts[j, i] = highCostCostRev; // Symmetric

    //            // Dictionary for route costs
    //            routeCostsDict_default[(i, j)] = cost;
    //            routeCostsDict_default[(j, i)] = revcost;
    //            // Dictionary for low cost route costs
    //            routeCostsDict_low[(i, j)] = lowCostCost;
    //            routeCostsDict_low[(j, i)] = lowCostCostRev;
    //            // Dictionary for high cost route costs
    //            routeCostsDict_high[(i, j)] = highCostCost;
    //            routeCostsDict_high[(j, i)] = highCostCostRev;

    //            //Debug.Log($"From point {i} to {j} low cost: {lowCostCost} default cost {cost} high cost {highCostCost}");


    //            //Debug.Log("Low costs");
    //            //DisplayRouteCostsMatrix(lowCostRouteCosts);
    //            //Debug.Log("default costs");
    //            //DisplayRouteCostsMatrix(routeCosts);
    //            //Debug.Log("high costs");
    //            //DisplayRouteCostsMatrix(highCostRouteCosts);

    //        }
    //    }
    //}
    // Method to update route costs and pathsBetweenGoals when just the "NEW ROUTE" button is clicked
    //public void UpdateRoutesAndPaths(Vector3 currentPosition, int goalNumberIndex)
    //{

    //    int numGoals = buildMap.goalLocations.Length;
    //    float hazardCost = RoverDriving.defaultHazardCost;
    //    Vector2[] goalLocations = buildMap.goalLocations;

    //    starterpath = FindPath2(currentPosition, goalLocations[goalNumberIndex], hazardCost);
    //    float startercost = FindPathCost(starterpath, hazardCost);

    //    pathsBetweenGoals[(-1, goalNumberIndex)] = starterpath; // Use -1 as an index for current position

    //    routeCosts[numGoals, goalNumberIndex] = startercost; // Last row/column in the matrix for current position
    //    routeCosts[goalNumberIndex, numGoals] = startercost; // Symmetric

    //}
    // Method to display the full routeCosts matrix
    public void DisplayRouteCostsMatrix(float[,] routeCosts)
    {
        int rows = routeCosts.GetLength(0);
        int cols = routeCosts.GetLength(1);
        //Debug.Log(rows);
        //Debug.Log(cols);
        string matrixOutput = "Route Costs Matrix:\n";

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrixOutput += routeCosts[i, j].ToString("F2") + "\t";
            }
            matrixOutput += "\n";
        }
        Debug.Log(matrixOutput);
    }

    // Method to update all 3 options for starter paths to each goal when "GENERATE ROUTES" button is clicked
    //public void UpdateAltRoutesAndPaths(Vector3 currentPosition, int goalNumberIndex)
    //{
    //    Vector2[] goalLocations = buildMap.goalLocations;
    //    int numGoals = goalLocations.Length;

    //    // Default starter path and costs
    //    List<Node_mouse> defaultstarterpath = FindPath2(currentPosition, goalLocations[goalNumberIndex], RoverDriving.defaultHazardCost);
    //    float defaultstartercost = FindPathCost(defaultstarterpath, RoverDriving.defaultHazardCost);
    //    pathsBetweenGoals[(-1, goalNumberIndex)] = defaultstarterpath; // Use -1 as an index for current position
    //    routeCosts[numGoals, goalNumberIndex] = defaultstartercost; // Last row/column in the matrix for current position
    //    routeCosts[goalNumberIndex, numGoals] = defaultstartercost; // Symmetric

    //    // Repeat for low cost (i.e. 0 cost hazards - fastest route)
    //    List<Node_mouse> lowcoststarterpath = FindPath2(currentPosition, goalLocations[goalNumberIndex], 0.0f);
    //    float lowcoststartercost = FindPathCost(lowcoststarterpath, 0f);
    //    lowCostPathsBetweenGoals[(-1, goalNumberIndex)] = lowcoststarterpath; // Use -1 as an index for current position
    //    lowCostRouteCosts[numGoals, goalNumberIndex] = lowcoststartercost; // Last row/column in the matrix for current position
    //    lowCostRouteCosts[goalNumberIndex, numGoals] = lowcoststartercost; // Symmetric

    //    // Repeat for high cost (i.e. high cost hazards - avoid hazards)
    //    List<Node_mouse> highcoststarterpath = FindPath2(currentPosition, goalLocations[goalNumberIndex], RoverDriving.highHazardCost);
    //    float highcoststartercost = FindPathCost(highcoststarterpath, RoverDriving.highHazardCost);
    //    highCostPathsBetweenGoals[(-1, goalNumberIndex)] = highcoststarterpath; // Use -1 as an index for current position
    //    highCostRouteCosts[numGoals, goalNumberIndex] = highcoststartercost; // Last row/column in the matrix for current position
    //    highCostRouteCosts[goalNumberIndex, numGoals] = highcoststartercost; // Symmetric

    //    //Debug.Log("Low costs");
    //    //DisplayRouteCostsMatrix(lowCostRouteCosts);
    //    //Debug.Log("default costs");
    //    //DisplayRouteCostsMatrix(routeCosts);
    //    //Debug.Log("high costs");
    //    //DisplayRouteCostsMatrix(highCostRouteCosts);
    //}

    // Method to update all 3 options for starter paths to each goal when "GENERATE ROUTES" button is clicked
    //public void UpdateAltRoutesAndPaths2(Vector3 currentPosition, int goalNumberIndex, List<Goal> goalList)
    //{
    //    Vector2[] goalLocations = buildMap.goalLocations;
    //    int numGoals = goalLocations.Length;
    //    Debug.Log("numGoals is " + numGoals + " goals Visited is " + RoverDriving.goalCount);
    //    int localGoalIndex;
    //    if (RoverDriving.goalCount == 0)  // initial build
    //    {
    //        localGoalIndex = goalNumberIndex;
    //    }
    //    else if (RoverDriving.goalCount == RoverDriving.totalGoals - buildMap.goalLocations.Length) // all goals visited
    //    {
    //        localGoalIndex = goalNumberIndex - RoverDriving.goalCount;
    //    }
    //    else  // some goals visited
    //    {
    //        localGoalIndex = goalNumberIndex - RoverDriving.goalCount; // NOT CORRECT!!!!
    //    }
    //    //Debug.Log("Local goal index is " + localGoalIndex);

    //    // Default starter path and costs
    //    Debug.Log("Current position is " + currentPosition + " goal number index is " + goalNumberIndex + " goal location is " + goalList[goalNumberIndex].Position);
    //    List<Node_mouse> defaultstarterpath = FindPath2(currentPosition, goalList[goalNumberIndex].Position, RoverDriving.defaultHazardCost);
    //    float defaultstartercost = FindPathCost(defaultstarterpath, RoverDriving.defaultHazardCost);
    //    pathsBetweenGoals[(-1, localGoalIndex)] = defaultstarterpath; // Use -1 as an index for current position
    //    routeCosts[numGoals, localGoalIndex] = defaultstartercost; // Last row/column in the matrix for current position
    //    routeCosts[localGoalIndex, numGoals] = defaultstartercost; // Symmetric

    //    // Repeat for low cost (i.e. 0 cost hazards - fastest route)
    //    List<Node_mouse> lowcoststarterpath = FindPath2(currentPosition, goalList[goalNumberIndex].Position, 0.0f);
    //    float lowcoststartercost = FindPathCost(lowcoststarterpath, 0f);
    //    lowCostPathsBetweenGoals[(-1, localGoalIndex)] = lowcoststarterpath; // Use -1 as an index for current position
    //    lowCostRouteCosts[numGoals, localGoalIndex] = lowcoststartercost; // Last row/column in the matrix for current position
    //    lowCostRouteCosts[localGoalIndex, numGoals] = lowcoststartercost; // Symmetric

    //    // Repeat for high cost (i.e. high cost hazards - avoid hazards)
    //    List<Node_mouse> highcoststarterpath = FindPath2(currentPosition, goalList[goalNumberIndex].Position, RoverDriving.highHazardCost);
    //    float highcoststartercost = FindPathCost(highcoststarterpath, RoverDriving.highHazardCost);
    //    highCostPathsBetweenGoals[(-1, localGoalIndex)] = highcoststarterpath; // Use -1 as an index for current position
    //    highCostRouteCosts[numGoals, localGoalIndex] = highcoststartercost; // Last row/column in the matrix for current position
    //    highCostRouteCosts[localGoalIndex, numGoals] = highcoststartercost; // Symmetric

    //    Debug.Log("Low costs");
    //    DisplayRouteCostsMatrix(lowCostRouteCosts);
    //    Debug.Log("default costs");
    //    DisplayRouteCostsMatrix(routeCosts);
    //    Debug.Log("high costs");
    //    DisplayRouteCostsMatrix(highCostRouteCosts);
    //}

    public void UpdateAltRoutesAndPaths3(Vector3 currentPosition, int goalNumberIndex, List<Goal> goalList)
    {
        //Vector2[] goalLocations = buildMap.goalLocations;
        //int numGoals = goalLocations.Length;
        //Debug.Log("numGoals is " + numGoals + " goals Visited is " + RoverDriving.goalCount);
        

        // Default starter path and costs
        //Debug.Log("Current position is " + currentPosition + " goal number index is " + goalNumberIndex + " goal location is " + goalList[goalNumberIndex].Position);
        List<Node_mouse> defaultstarterpath = FindPath2(currentPosition, goalList[goalNumberIndex].Position, RoverDriving.defaultHazardCost);
        float defaultstartercost = FindPathCost(defaultstarterpath, RoverDriving.defaultHazardCost);
        pathsBetweenGoals[(-1, goalNumberIndex)] = defaultstarterpath; // Use -1 as an index for current position
        //routeCosts[numGoals, localGoalIndex] = defaultstartercost; // Last row/column in the matrix for current position
        //routeCosts[localGoalIndex, numGoals] = defaultstartercost; // Symmetric

        // Repeat for low cost (i.e. 0 cost hazards - fastest route)
        List<Node_mouse> lowcoststarterpath = FindPath2(currentPosition, goalList[goalNumberIndex].Position, 0.0f);
        float lowcoststartercost = FindPathCost(lowcoststarterpath, 0f);
        lowCostPathsBetweenGoals[(-1, goalNumberIndex)] = lowcoststarterpath; // Use -1 as an index for current position
        //lowCostRouteCosts[numGoals, localGoalIndex] = lowcoststartercost; // Last row/column in the matrix for current position
        //lowCostRouteCosts[localGoalIndex, numGoals] = lowcoststartercost; // Symmetric

        // Repeat for high cost (i.e. high cost hazards - avoid hazards)
        List<Node_mouse> highcoststarterpath = FindPath2(currentPosition, goalList[goalNumberIndex].Position, RoverDriving.highHazardCost);
        float highcoststartercost = FindPathCost(highcoststarterpath, RoverDriving.highHazardCost);
        highCostPathsBetweenGoals[(-1, goalNumberIndex)] = highcoststarterpath; // Use -1 as an index for current position
        //highCostRouteCosts[numGoals, localGoalIndex] = highcoststartercost; // Last row/column in the matrix for current position
        //highCostRouteCosts[localGoalIndex, numGoals] = highcoststartercost; // Symmetric

        //if (RoverDriving.selectedConfig == "Condition B")
        //{
        //    Debug.Log("Condition B starter path");
        //    //Debug.Log("Current position is " + currentPosition + " goal number index is " + goalNumberIndex + " goal location is " + goalList[goalNumberIndex].Position);
        //    List<Node_mouse> starterpath_NoExpl = FindPath3(currentPosition, goalList[goalNumberIndex].Position, noExplanationHazardWeight);   // Change in hazard cost is in FindPath3
        //    float starterCost_NoExpl = FindPathCost(starterpath_NoExpl, noExplanationHazardWeight);
        //    noExplanationPathsBetweenGoals[(-1, goalNumberIndex)] = starterpath_NoExpl; // Use -1 as an index for current position
        //    routeCostsDict_noExplanation[(-1, goalNumberIndex)] = starterCost_NoExpl; // Last row/column in the matrix for current position
        //}

        // Dictionary for route costs
        routeCostsDict_default[(-1, goalNumberIndex)] = defaultstartercost;
        // Dictionary for low cost route costs
        routeCostsDict_low[(-1, goalNumberIndex)] = lowcoststartercost;
        // Dictionary for high cost route costs
        routeCostsDict_high[(-1, goalNumberIndex)] = highcoststartercost;
        
        //Debug.Log("Low costs");
        //DisplayRouteCostsMatrix(lowCostRouteCosts);
        //Debug.Log("default costs");
        //DisplayRouteCostsMatrix(routeCosts);
        //Debug.Log("high costs");
        //DisplayRouteCostsMatrix(highCostRouteCosts);
    }

    // Method to update ONLY THE DEFAULT COST starter path for the final "ideal AT score" calculations
    public void UpdateAltRoutesAndPaths4(Vector3 currentPosition, int goalNumberIndex, List<Goal> goalList)
    {
        // Default starter path and costs
        //Debug.Log("Current position is " + currentPosition + " goal number index is " + goalNumberIndex + " goal location is " + goalList[goalNumberIndex].Position);
        List<Node_mouse> defaultstarterpath = FindPath2(currentPosition, goalList[goalNumberIndex].Position, RoverDriving.defaultHazardCost);
        float defaultstartercost = FindPathCost(defaultstarterpath, RoverDriving.defaultHazardCost);
        pathsBetweenGoals[(-1, goalNumberIndex)] = defaultstarterpath; // Use -1 as an index for current position
        
        //Debug.Log("Final default starter path count is: " + defaultstarterpath.Count);

        //if (RoverDriving.selectedConfig == "Condition B")
        //{
        //    Debug.Log("Condition B starter path");
        //    //Debug.Log("Current position is " + currentPosition + " goal number index is " + goalNumberIndex + " goal location is " + goalList[goalNumberIndex].Position);
        //    List<Node_mouse> starterpath_NoExpl = FindPath3(currentPosition, goalList[goalNumberIndex].Position, noExplanationHazardWeight);   // Change in hazard cost is in FindPath3
        //    float starterCost_NoExpl = FindPathCost(starterpath_NoExpl, noExplanationHazardWeight);
        //    noExplanationPathsBetweenGoals[(-1, goalNumberIndex)] = starterpath_NoExpl; // Use -1 as an index for current position
        //    routeCostsDict_noExplanation[(-1, goalNumberIndex)] = starterCost_NoExpl; // Last row/column in the matrix for current position
        //}

        // Dictionary for route costs
        routeCostsDict_default[(-1, goalNumberIndex)] = defaultstartercost;
        //Debug.Log("Default cost for goal " + goalNumberIndex + " is " + defaultstartercost);
        //Debug.Log("Default starter cost in dictionary is: " + routeCostsDict_default[(-1, goalNumberIndex)]);
    }


    // Method to compute paths from current position to all goals and find optimal path for default hazardCost only
    //public List<Node_mouse> FindOptimalPath()
    //{
    //    int numGoals = buildMap.goalLocations.Length;

    //    // Find the optimal path using a brute-force search
    //    List<int> goalIndices = Enumerable.Range(0, numGoals).ToList();

    //    (float minCost, List<int> bestPath) = FindLowestCostPath(numGoals, goalIndices);


    //    // Convert indices to Node_mouses for the final path
    //    List<Node_mouse> optimalPath = new List<Node_mouse>();
    //    if (pathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
    //    {
    //        optimalPath.AddRange(startPath); // Start path from current position
    //                                         //Debug.Log("StartPath added " + startPath.Count);
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
    //    }

    //    for (int i = 1; i < bestPath.Count - 1; i++)
    //    {
    //        int from = bestPath[i];
    //        int to = bestPath[i + 1];

    //        // Check if the path segment exists and add it
    //        if (pathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
    //        {
    //            int initialCount = optimalPath.Count;
    //            optimalPath.AddRange(segmentPath);

    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
    //        }
    //    }

    //    //Debug.Log($"Optimal Path Cost: {minCost} for hazard cost {hazardCost}");
    //    buildMap.path = optimalPath; // Set the final path to display or use
    //    return optimalPath;
    //}

    // Method to find the optimal path for each of the three hazardCosts
    //public List<Node_mouse> FindOptimalPath2(float hazardCost)
    //{
    //    int numGoals = buildMap.goalLocations.Length;
    //    // Find the optimal path using a brute-force search
    //    List<int> goalIndices = Enumerable.Range(0, numGoals).ToList();

    //    if (hazardCost == 0f)
    //    {

    //        (float minCost, List<int> bestPath) = FindLowestCostPath_Zero(numGoals, goalIndices);

    //        // Convert indices to Node_mouses for the final path
    //        List<Node_mouse> optimalPath = new List<Node_mouse>();
    //        if (lowCostPathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
    //        {
    //            optimalPath.AddRange(startPath); // Start path from current position
    //                                             //Debug.Log("StartPath added " + startPath.Count);
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
    //        }
    //        //Debug.Log("best path order is starting " + string.Join(", ", bestPath));
    //        for (int i = 1; i < bestPath.Count - 1; i++)
    //        {
    //            int from = bestPath[i];
    //            int to = bestPath[i + 1];


    //            // Check if the path segment exists and add it
    //            if (lowCostPathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
    //            {
    //                int initialCount = optimalPath.Count;
    //                optimalPath.AddRange(segmentPath);
    //            }
    //            else
    //            {
    //                Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
    //            }
    //        }

    //        //Debug.Log($"Optimal Path Cost: {minCost} for hazard cost {hazardCost}");
    //        buildMap.path = optimalPath; // Set the final path to display or use
    //        return optimalPath;
    //    }
    //    else if (hazardCost == RoverDriving.highHazardCost)
    //    {
    //        //Debug.Log("Finding optimal path for high cost");
    //        //(float minCost, List<int> bestPath) = FindLowestCostPath_High(numGoals, goalIndices);
    //        (float minCost, List<int> bestPath) = FindLowestCostPath_High2(goalIndices);


    //        // Convert indices to Node_mouses for the final path
    //        List<Node_mouse> optimalPath = new List<Node_mouse>();
    //        if (highCostPathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
    //        {
    //            optimalPath.AddRange(startPath); // Start path from current position
    //                                             //Debug.Log("StartPath added " + startPath.Count);
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
    //        }
    //        //Debug.Log("best path order is starting " + string.Join(", ", bestPath));
    //        for (int i = 1; i < bestPath.Count - 1; i++)
    //        {
    //            int from = bestPath[i];
    //            int to = bestPath[i + 1];


    //            // Check if the path segment exists and add it
    //            if (highCostPathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
    //            {
    //                int initialCount = optimalPath.Count;
    //                optimalPath.AddRange(segmentPath);
    //            }
    //            else
    //            {
    //                Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
    //            }
    //        }

    //        //Debug.Log($"Optimal Path Cost: {minCost} for hazard cost {hazardCost}");
    //        buildMap.path = optimalPath; // Set the final path to display or use
    //        return optimalPath;
    //    }
    //    else
    //    {
    //        //Debug.Log("Finding optimal path for default cost");
    //        (float minCost, List<int> bestPath) = FindLowestCostPath(numGoals, goalIndices);

    //        // Convert indices to Node_mouses for the final path
    //        List<Node_mouse> optimalPath = new List<Node_mouse>();
    //        if (pathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
    //        {
    //            optimalPath.AddRange(startPath); // Start path from current position
    //                                             //Debug.Log("StartPath added " + startPath.Count);
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
    //        }
    //        //Debug.Log("best path order is starting " + string.Join(", ", bestPath));
    //        for (int i = 1; i < bestPath.Count - 1; i++)
    //        {
    //            int from = bestPath[i];
    //            int to = bestPath[i + 1];


    //            // Check if the path segment exists and add it
    //            if (pathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
    //            {
    //                int initialCount = optimalPath.Count;
    //                optimalPath.AddRange(segmentPath);
    //            }
    //            else
    //            {
    //                Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
    //            }
    //        }

    //        //Debug.Log($"Optimal Path Cost: {minCost} for hazard cost {hazardCost}");
    //        buildMap.path = optimalPath; // Set the final path to display or use
    //        return optimalPath;
    //    }


    //}

    //public List<Node_mouse> FindOptimalPath3(float hazardCost, List<Goal> unvisitedGoals)
    //{
    //    int numGoals = buildMap.goalLocations.Length;
    //    Debug.Log("NumGoals in FindOptPath3 is " + numGoals);
    //    List<int> goalIndices = unvisitedGoals.Select(goal => goal.GoalNumber).ToList();
    //    Debug.Log("Goal Indices count is " + goalIndices.Count);

    //    if (hazardCost == 0f)
    //    {
    //        //(float minCost, List<int> bestPath) = FindLowestCostPath_Zero(numGoals, goalIndices);
    //        (float minCost, List<int> bestPath) = FindLowestCostPath_Zero2(goalIndices);


    //        List<Node_mouse> optimalPath = new List<Node_mouse>();
    //        if (lowCostPathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
    //        {
    //            optimalPath.AddRange(startPath);
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
    //        }

    //        for (int i = 1; i < bestPath.Count - 1; i++)
    //        {
    //            int from = bestPath[i];
    //            int to = bestPath[i + 1];

    //            if (lowCostPathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
    //            {
    //                optimalPath.AddRange(segmentPath);
    //            }
    //            else
    //            {
    //                Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
    //            }
    //        }

    //        buildMap.path = optimalPath;
    //        return optimalPath;
    //    }
    //    else if (hazardCost == RoverDriving.highHazardCost)
    //    {
    //        //(float minCost, List<int> bestPath) = FindLowestCostPath_High(numGoals, goalIndices);
    //        (float minCost, List<int> bestPath) = FindLowestCostPath_High2(goalIndices);


    //        List<Node_mouse> optimalPath = new List<Node_mouse>();
    //        if (highCostPathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
    //        {
    //            optimalPath.AddRange(startPath);
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
    //        }

    //        for (int i = 1; i < bestPath.Count - 1; i++)
    //        {
    //            int from = bestPath[i];
    //            int to = bestPath[i + 1];

    //            if (highCostPathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
    //            {
    //                optimalPath.AddRange(segmentPath);
    //            }
    //            else
    //            {
    //                Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
    //            }
    //        }

    //        buildMap.path = optimalPath;
    //        return optimalPath;
    //    }
    //    else
    //    {
    //        //(float minCost, List<int> bestPath) = FindLowestCostPath(numGoals, goalIndices);
    //        (float minCost, List<int> bestPath) = FindLowestCostPath_Default2(goalIndices);


    //        List<Node_mouse> optimalPath = new List<Node_mouse>();
    //        if (pathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
    //        {
    //            optimalPath.AddRange(startPath);
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
    //        }

    //        for (int i = 1; i < bestPath.Count - 1; i++)
    //        {
    //            int from = bestPath[i];
    //            int to = bestPath[i + 1];

    //            if (pathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
    //            {
    //                optimalPath.AddRange(segmentPath);
    //            }
    //            else
    //            {
    //                Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
    //            }
    //        }

    //        buildMap.path = optimalPath;
    //        return optimalPath;
    //    }
    //}

    public List<Node_mouse> FindOptimalPath4(float hazardCost, List<Goal> unvisitedGoals)
    {
        int numGoals = buildMap.goalLocations.Length;
        //Debug.Log("NumGoals in FindOptPath4 is " + numGoals);
        List<int> goalIndices = unvisitedGoals.Select(goal => goal.GoalNumber).ToList();
        //Debug.Log("Goal Indices count is " + goalIndices.Count);

        if (hazardCost == 0f)
        {
            //(float minCost, List<int> bestPath) = FindLowestCostPath_Zero(numGoals, goalIndices);
            (float minCost, List<int> bestPath) = FindLowestCostPath_Zero2(goalIndices);


            List<Node_mouse> optimalPath = new List<Node_mouse>();
            if (lowCostPathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
            {
                optimalPath.AddRange(startPath);
            }
            else
            {
                Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
            }

            for (int i = 1; i < bestPath.Count - 1; i++)
            {
                int from = bestPath[i];
                int to = bestPath[i + 1];

                if (lowCostPathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
                {
                    optimalPath.AddRange(segmentPath);
                }
                else
                {
                    Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
                }
            }

            buildMap.path = optimalPath;
            return optimalPath;
        }
        else if (hazardCost == RoverDriving.highHazardCost)
        {
            //(float minCost, List<int> bestPath) = FindLowestCostPath_High(numGoals, goalIndices);
            (float minCost, List<int> bestPath) = FindLowestCostPath_High2(goalIndices);


            List<Node_mouse> optimalPath = new List<Node_mouse>();
            if (highCostPathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
            {
                optimalPath.AddRange(startPath);
            }
            else
            {
                Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
            }

            for (int i = 1; i < bestPath.Count - 1; i++)
            {
                int from = bestPath[i];
                int to = bestPath[i + 1];

                if (highCostPathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
                {
                    optimalPath.AddRange(segmentPath);
                }
                else
                {
                    Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
                }
            }

            buildMap.path = optimalPath;
            return optimalPath;
        }
        else
        {
            List<int> bestPath = new List<int>();
            float minCost = float.MaxValue;
            (minCost, bestPath) = FindLowestCostPath_Default2(goalIndices);

            List<Node_mouse> optimalPath = new List<Node_mouse>();
            if (pathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
            {
                optimalPath.AddRange(startPath);
            }
            else
            {
                Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
            }

            for (int i = 1; i < bestPath.Count - 1; i++)
            {
                int from = bestPath[i];
                int to = bestPath[i + 1];

                if (pathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
                {
                    optimalPath.AddRange(segmentPath);
                }
                else
                {
                    Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
                }
            }

            buildMap.path = optimalPath;
            return optimalPath;
        }
    }

    public List<Node_mouse> FindOptimalPostRoundPath(List<int> goalIndicesToVisit)
    {
        List<int> bestPath = new List<int>();
        float minCost = float.MaxValue;
        (minCost, bestPath) = FindLowestCostPath_Default2(goalIndicesToVisit);
        


        List<Node_mouse> optimalPath = new List<Node_mouse>();
        if (pathsBetweenGoals.TryGetValue((-1, bestPath[1]), out List<Node_mouse> startPath) && startPath != null)
        {
            optimalPath.AddRange(startPath);
        }
        else
        {
            Debug.LogWarning($"Path segment from start position to goal {bestPath[0]} is missing or null.");
        }

        for (int i = 1; i < bestPath.Count - 1; i++)
        {
            int from = bestPath[i];
            int to = bestPath[i + 1];

            if (pathsBetweenGoals.TryGetValue((from, to), out List<Node_mouse> segmentPath) && segmentPath != null)
            {
                optimalPath.AddRange(segmentPath);
            }
            else
            {
                Debug.LogWarning($"Path segment from goal {from} to goal {to} is missing or null.");
            }
        }

        buildMap.path = optimalPath;
        return optimalPath;
    }

    //private (float, List<int>) FindLowestCostPath(int startIdx, List<int> goalIndices)
    //{
    //    float minCost = float.MaxValue;
    //    List<int> bestPath = null;
    //    List<int> adjustedGoalIndices;

    //    // Create a copy of goalIndices and subtract RoverDriving.goalCount from each index if all goals this round have been visited
    //    if (RoverDriving.goalCount == RoverDriving.totalGoals - buildMap.goalLocations.Length)
    //    {
    //        adjustedGoalIndices = goalIndices.Select(index => index - RoverDriving.goalCount).ToList();

    //    }
    //    else
    //    {
    //        adjustedGoalIndices = goalIndices;
    //    }

    //    // Get all permutations of goal indices
    //    foreach (var perm in GetPermutations(adjustedGoalIndices, adjustedGoalIndices.Count))
    //    {
    //        //Debug.Log("Perm " + perm.ToString());
    //        float cost = routeCosts[startIdx, perm[0]];
    //        string pathString = startIdx + " -> " + perm[0]; // Start building path display

    //        for (int i = 0; i < perm.Count - 1; i++)
    //        {
    //            cost += routeCosts[perm[i], perm[i + 1]];
    //            pathString += " -> " + perm[i + 1];
    //        }

    //        pathString += " | Total Cost: " + cost.ToString("F2");
    //        //Debug.Log("Path: " + pathString); // Log each path and its cost


    //        if (cost < minCost)
    //        {
    //            minCost = cost;
    //            //Debug.Log("Found lower cost");

    //            bestPath = new List<int>() { startIdx };
    //            bestPath.AddRange(perm);

    //            //Debug.Log("Updated bestPath count: " + bestPath.Count);
    //        }
    //    }

    //    // Ensure bestPath is valid
    //    if (bestPath == null)
    //    {
    //        bestPath = new List<int> { startIdx }; // Default to just startIdx if no valid path found
    //        Debug.LogWarning("No valid paths found. Returning startIdx only.");
    //    }

    //    //Debug.Log("Final bestpath " + bestPath.Count);
    //    return (minCost, bestPath);
    //}

    //private (float, List<int>) FindLowestCostPath_Zero(int startIdx, List<int> goalIndices)
    //{
    //    float minCost = float.MaxValue;
    //    List<int> bestPath = null;
    //    List<int> adjustedGoalIndices;

    //    // Create a copy of goalIndices and subtract RoverDriving.goalCount from each index
    //    if (RoverDriving.goalCount == RoverDriving.totalGoals-buildMap.goalLocations.Length)
    //    {
    //        adjustedGoalIndices = goalIndices.Select(index => index - RoverDriving.goalCount).ToList();

    //    }
    //    else
    //    {
    //        adjustedGoalIndices = goalIndices;
    //    }

    //        // Get all permutations of adjusted goal indices
    //        foreach (var perm in GetPermutations(adjustedGoalIndices, adjustedGoalIndices.Count))
    //        {
    //            //Debug.Log("Perm 0 " + perm[0].ToString() + " perm1 " + perm[1].ToString());
    //            float cost = lowCostRouteCosts[startIdx, perm[0]];
    //            string pathString = startIdx + " -> " + perm[0]; // Start building path display

    //            for (int i = 0; i < perm.Count - 1; i++)
    //            {
    //                cost += lowCostRouteCosts[perm[i], perm[i + 1]];
    //                pathString += " -> " + perm[i + 1];
    //            }

    //            pathString += " | Total Cost: " + cost.ToString("F2");
    //            //Debug.Log("Path: " + pathString); // Log each path and its cost

    //            if (cost < minCost)
    //            {
    //                minCost = cost;
    //                //Debug.Log("Found lower cost");

    //                bestPath = new List<int>() { startIdx };
    //                bestPath.AddRange(perm);

    //                //Debug.Log("Updated bestPath count: " + bestPath.Count);
    //            }
    //        }

    //    // Ensure bestPath is valid
    //    if (bestPath == null)
    //    {
    //        bestPath = new List<int> { startIdx }; // Default to just startIdx if no valid path found
    //        Debug.LogWarning("No valid paths found. Returning startIdx only.");
    //    }

    //    //Debug.Log("Final bestpath " + bestPath.Count);
    //    return (minCost, bestPath);
    //}

    private (float, List<int>) FindLowestCostPath_Zero2(List<int> goalIndices)
    {
        float minCost = float.MaxValue;
        List<int> bestPath = null;

        // Get all permutations of adjusted goal indices
        foreach (var perm in GetPermutations(goalIndices, goalIndices.Count))
        {
            //Debug.Log("Perm 0 " + perm[0].ToString() + " perm1 " + perm[1].ToString());
            //float cost = lowCostRouteCosts[startIdx, perm[0]];
            float cost = routeCostsDict_low[(-1, perm[0])];
            string pathString = -1 + " -> " + perm[0]; // Start building path display

            for (int i = 0; i < perm.Count - 1; i++)
            {
                //cost += lowCostRouteCosts[perm[i], perm[i + 1]];
                cost += routeCostsDict_low[(perm[i], perm[i + 1])];
                pathString += " -> " + perm[i + 1];
            }

            pathString += " | Total Cost: " + cost.ToString("F2");
            //Debug.Log("Path: " + pathString); // Log each path and its cost

            if (cost < minCost)
            {
                minCost = cost;
                //Debug.Log("Found lower cost");

                bestPath = new List<int>() { -1 };
                bestPath.AddRange(perm);

                //Debug.Log("Updated bestPath count: " + bestPath.Count);
            }
        }

        // Ensure bestPath is valid
        if (bestPath == null)
        {
            bestPath = new List<int> { -1 }; // Default to just starting index, which is -1
            Debug.LogWarning("No valid paths found. Returning startIdx only.");
        }

        //Debug.Log("Final bestpath " + bestPath.Count);
        return (minCost, bestPath);
    }

    private (float, List<int>) FindLowestCostPath_High2(List<int> goalIndices)
    {
        float minCost = float.MaxValue;
        List<int> bestPath = null;

        // Get all permutations of adjusted goal indices
        foreach (var perm in GetPermutations(goalIndices, goalIndices.Count))
        {
            //Debug.Log("Perm 0 " + perm[0].ToString() + " perm1 " + perm[1].ToString());
            //float cost = lowCostRouteCosts[startIdx, perm[0]];
            float cost = routeCostsDict_high[(-1, perm[0])];
            string pathString = -1 + " -> " + perm[0]; // Start building path display

            for (int i = 0; i < perm.Count - 1; i++)
            {
                //cost += lowCostRouteCosts[perm[i], perm[i + 1]];
                cost += routeCostsDict_high[(perm[i], perm[i + 1])];
                pathString += " -> " + perm[i + 1];
            }

            pathString += " | Total Cost: " + cost.ToString("F2");
            //Debug.Log("Path: " + pathString); // Log each path and its cost

            if (cost < minCost)
            {
                minCost = cost;
                //Debug.Log("Found lower cost");

                bestPath = new List<int>() { -1 };
                bestPath.AddRange(perm);

                //Debug.Log("Updated bestPath count: " + bestPath.Count);
            }
        }

        // Ensure bestPath is valid
        if (bestPath == null)
        {
            bestPath = new List<int> { -1 }; // Default to just starting index, which is -1
            Debug.LogWarning("No valid paths found. Returning startIdx only.");
        }

        //Debug.Log("Final bestpath " + bestPath.Count);
        return (minCost, bestPath);
    }

    private (float, List<int>) FindLowestCostPath_Default2(List<int> goalIndices)
    {
        float minCost = float.MaxValue;
        List<int> bestPath = null;

        // Get all permutations of adjusted goal indices
        foreach (var perm in GetPermutations(goalIndices, goalIndices.Count))
        {
            //Debug.Log("Perm 0 " + perm[0].ToString() + " perm1 " + perm[1].ToString());
            //float cost = lowCostRouteCosts[startIdx, perm[0]];
            float cost = routeCostsDict_default[(-1, perm[0])];
            string pathString = -1 + " -> " + perm[0]; // Start building path display

            for (int i = 0; i < perm.Count - 1; i++)
            {
                //cost += lowCostRouteCosts[perm[i], perm[i + 1]];
                cost += routeCostsDict_default[(perm[i], perm[i + 1])];
                pathString += " -> " + perm[i + 1];
            }

            pathString += " | Total Cost: " + cost.ToString("F2");
            //Debug.Log("Path: " + pathString); // Log each path and its cost

            if (cost < minCost)
            {
                minCost = cost;
                //Debug.Log("Found lower cost");

                bestPath = new List<int>() { -1 };
                bestPath.AddRange(perm);

                //Debug.Log("Updated bestPath count: " + bestPath.Count);
            }
        }

        // Ensure bestPath is valid
        if (bestPath == null)
        {
            bestPath = new List<int> { -1 }; // Default to just starting index, which is -1
            Debug.LogWarning("No valid paths found. Returning startIdx only.");
        }

        //Debug.Log("Final bestpath " + bestPath.Count);
        return (minCost, bestPath);
    }

    //private (float, List<int>) FindLowestCostPath_NoExplanation(List<int> goalIndices)
    //{
    //    float minCost = float.MaxValue;
    //    List<int> bestPath = null;

    //    // Get all permutations of adjusted goal indices
    //    foreach (var perm in GetPermutations(goalIndices, goalIndices.Count))
    //    {
    //        //Debug.Log("Perm 0 " + perm[0].ToString() + " perm1 " + perm[1].ToString());
    //        //float cost = lowCostRouteCosts[startIdx, perm[0]];
    //        float cost = routeCostsDict_noExplanation[(-1, perm[0])];
    //        string pathString = -1 + " -> " + perm[0]; // Start building path display

    //        for (int i = 0; i < perm.Count - 1; i++)
    //        {
    //            //cost += lowCostRouteCosts[perm[i], perm[i + 1]];
    //            cost += routeCostsDict_noExplanation[(perm[i], perm[i + 1])];
    //            pathString += " -> " + perm[i + 1];
    //        }

    //        pathString += " | Total Cost: " + cost.ToString("F2");
    //        //Debug.Log("Path: " + pathString); // Log each path and its cost

    //        if (cost < minCost)
    //        {
    //            minCost = cost;
    //            //Debug.Log("Found lower cost");

    //            bestPath = new List<int>() { -1 };
    //            bestPath.AddRange(perm);

    //            //Debug.Log("Updated bestPath count: " + bestPath.Count);
    //        }
    //    }

    //    // Ensure bestPath is valid
    //    if (bestPath == null)
    //    {
    //        bestPath = new List<int> { -1 }; // Default to just starting index, which is -1
    //        Debug.LogWarning("No valid paths found. Returning startIdx only.");
    //    }

    //    //Debug.Log("Final bestpath " + bestPath.Count);
    //    return (minCost, bestPath);
    //}

    //private (float, List<int>) FindLowestCostPath_High(int startIdx, List<int> goalIndices)
    //{
    //    float minCost = float.MaxValue;
    //    List<int> bestPath = null;
    //    List<int> adjustedGoalIndices;

    //    // Create a copy of goalIndices and subtract RoverDriving.goalCount from each index
    //    if (RoverDriving.goalCount == RoverDriving.totalGoals - buildMap.goalLocations.Length)
    //    {
    //        adjustedGoalIndices = goalIndices.Select(index => index - RoverDriving.goalCount).ToList();

    //    }
    //    else
    //    {
    //        adjustedGoalIndices = goalIndices;
    //    }

    //    // Get all permutations of goal indices
    //    foreach (var perm in GetPermutations(adjustedGoalIndices, adjustedGoalIndices.Count))
    //    {
    //        //Debug.Log("Perm " + perm.ToString());
    //        float cost = highCostRouteCosts[startIdx, perm[0]];
    //        string pathString = startIdx + " -> " + perm[0]; // Start building path display

    //        for (int i = 0; i < perm.Count - 1; i++)
    //        {
    //            cost += highCostRouteCosts[perm[i], perm[i + 1]];
    //            pathString += " -> " + perm[i + 1];
    //        }

    //        pathString += " | Total Cost: " + cost.ToString("F2");
    //        //Debug.Log("Path: " + pathString); // Log each path and its cost


    //        if (cost < minCost)
    //        {
    //            minCost = cost;
    //            //Debug.Log("Found lower cost");

    //            bestPath = new List<int>() { startIdx };
    //            bestPath.AddRange(perm);

    //            //Debug.Log("Updated bestPath count: " + bestPath.Count);
    //        }
    //    }

    //    // Ensure bestPath is valid
    //    if (bestPath == null)
    //    {
    //        bestPath = new List<int> { startIdx }; // Default to just startIdx if no valid path found
    //        Debug.LogWarning("No valid paths found. Returning startIdx only.");
    //    }

    //    //Debug.Log("Final bestpath " + bestPath.Count);
    //    return (minCost, bestPath);
    //}

    private IEnumerable<List<int>> GetPermutations(List<int> list, int length)
    {
        if (length == 1) return list.Select(t => new List<int> { t });

        return GetPermutations(list, length - 1)
            .SelectMany(t => list.Where(e => !t.Contains(e)),
                        (t1, t2) => t1.Concat(new List<int> { t2 }).ToList());
    }


    public List<Node_mouse> FindPath2(Vector3 startPoint, Vector3 endPoint, float hazardCost)
    {
        //Debug.Log($"Finding path with hazardCost: {hazardCost}");
        Node_mouse startNode = buildMap.GetNodeFromWorldPoint(startPoint);
        Node_mouse endNode = buildMap.GetNodeFromWorldPoint(endPoint);

        if (startNode == null || endNode == null || startNode.isBoulder || endNode.isBoulder)
        {
            if (startNode == null || startNode.isBoulder)
            {
                //Debug.LogWarning($"Adjusting start point from {startPoint} due to boulder or invalid node.");
                startNode = AdjustStartPoint(startPoint);
                if (startNode == null)
                {
                    Debug.LogError("No valid start point found.");
                    return new List<Node_mouse>();
                }
                startPoint = startNode.worldPosition;
            }

            if (endNode == null || endNode.isBoulder)
            {
                //Debug.LogError($"Invalid or blocked end point at {endPoint}.");
                endNode = FindValidEndPoint(endPoint);
            }
        }

        List<Node_mouse> openList = new List<Node_mouse> { startNode };
        HashSet<Node_mouse> closedList = new HashSet<Node_mouse>();

        Node_mouse closestNode = startNode;
        float closestDistance = float.MaxValue;

        while (openList.Count > 0)
        {
            // Find the node with the lowest fCost in the open list
            Node_mouse currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                return RetracePath2(startNode, endNode);
            }

            // Track the closest node for fallback
            float currentDistance = GetDistance(currentNode, endNode);
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestNode = currentNode;
            }

            // Process neighbors
            foreach (Node_mouse neighbor in buildMap.GetNeighbors(currentNode, 1))
            {

                if (neighbor.isBoulder || closedList.Contains(neighbor))
                {
                    continue;
                }

                // Calculate the cost to reach the neighbor
                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                // If neighbor is in a hazard zone, add hazard cost
                if (neighbor.weight > 0)  // added here
                {
                    newCostToNeighbor += Mathf.RoundToInt(hazardCost * neighbor.weight);
                }
                //Debug.Log($"Neighbor: {neighbor.worldPosition}, Weight: {neighbor.weight}, Cost with hazard: {newCostToNeighbor}");
                // If the new cost is better or neighbor not yet evaluated
                if (newCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endNode);
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("No path found. Returning closest path.");
        return RetracePath2(startNode, closestNode);
    }

    // This modifies the FindPath 2 method to modify the hazard, time, and goal factors for the "no explanation" condition
    //public List<Node_mouse> FindPath3(Vector3 startPoint, Vector3 endPoint, float hazardCost)
    //{
    //    //Debug.Log($"Finding path with hazardCost: {hazardCost}");
    //    Node_mouse startNode = buildMap.GetNodeFromWorldPoint(startPoint);
    //    Node_mouse endNode = buildMap.GetNodeFromWorldPoint(endPoint);

    //    if (startNode == null || endNode == null || startNode.isBoulder || endNode.isBoulder)
    //    {
    //        if (startNode == null || startNode.isBoulder)
    //        {
    //            //Debug.LogWarning($"Adjusting start point from {startPoint} due to boulder or invalid node.");
    //            startNode = AdjustStartPoint(startPoint);
    //            if (startNode == null)
    //            {
    //                Debug.LogError("No valid start point found.");
    //                return new List<Node>();
    //            }
    //            startPoint = startNode.worldPosition;
    //        }

    //        if (endNode == null || endNode.isBoulder)
    //        {
    //            //Debug.LogError($"Invalid or blocked end point at {endPoint}.");
    //            endNode = FindValidEndPoint(endPoint);
    //        }
    //    }

    //    List<Node_mouse> openList = new List<Node_mouse> { startNode };
    //    HashSet<Node_mouse> closedList = new HashSet<Node_mouse>();

    //    Node_mouse closestNode = startNode;
    //    float closestDistance = float.MaxValue;

    //    // Added hazard factor modifications for "no explanation" condition
    //    //hazardCost = hazardCost * Random.Range(0.5f, 100000.0f);  // +/-change in hazard cost. x100,000 makes it equivalent to the "avoid hazards" route planning (500,000 cost)
    //    //Debug.Log($"Hazard cost is: {hazardCost}");
    //    //float timeFactor = Random.Range(0.9f, 1.1f);        // +/-10% change in time cost
    //    //Debug.Log($"Time factor modified to: {timeFactor}");

    //    while (openList.Count > 0)
    //    {
    //        // Find the node with the lowest fCost in the open list
    //        Node_mouse currentNode = openList[0];
    //        for (int i = 1; i < openList.Count; i++)
    //        {
    //            if (openList[i].fCost < currentNode.fCost ||
    //                (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
    //            {
    //                currentNode = openList[i];
    //            }
    //        }

    //        openList.Remove(currentNode);
    //        closedList.Add(currentNode);

    //        if (currentNode == endNode)
    //        {
    //            return RetracePath2(startNode, endNode);
    //        }

    //        // Track the closest node for fallback
    //        float currentDistance = GetDistance(currentNode, endNode);
    //        if (currentDistance < closestDistance)
    //        {
    //            closestDistance = currentDistance;
    //            closestNode = currentNode;
    //        }

    //        // Process neighbors
    //        foreach (Node_mouse neighbor in buildMap.GetNeighbors(currentNode, 1))
    //        {

    //            if (neighbor.isBoulder || closedList.Contains(neighbor))
    //            {
    //                continue;
    //            }

    //            // Calculate the cost to reach the neighbor
    //            int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

    //            // Add in a minor random factor to the cost
    //            newCostToNeighbor = Mathf.RoundToInt(newCostToNeighbor);

    //            // If neighbor is in a hazard zone, add hazard cost
    //            if (neighbor.weight > 0)  // added here
    //            {
    //                newCostToNeighbor += Mathf.RoundToInt(hazardCost * neighbor.weight);
    //            }
    //            //Debug.Log($"Neighbor: {neighbor.worldPosition}, Weight: {neighbor.weight}, Cost with hazard: {newCostToNeighbor}");
    //            // If the new cost is better or neighbor not yet evaluated
    //            if (newCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
    //            {
    //                neighbor.gCost = newCostToNeighbor;
    //                neighbor.hCost = GetDistance(neighbor, endNode);
    //                neighbor.parent = currentNode;

    //                if (!openList.Contains(neighbor))
    //                {
    //                    openList.Add(neighbor);
    //                }
    //            }
    //        }
    //    }

    //    Debug.LogWarning("No path found. Returning closest path.");
    //    return RetracePath2(startNode, closestNode);
    //}
    // Adjust end point to avoid boulders or invalid Node_mouses
    public Node_mouse FindValidEndPoint(Vector3 endPoint)
    {
        Node_mouse endNode = buildMap.GetNodeFromWorldPoint(endPoint);

        if (endNode == null || endNode.isBoulder)
        {
            //Debug.LogWarning($"Invalid or blocked end point at {endPoint}. Attempting to adjust...");

            // Define a search radius and step size
            float searchRadius = 5f; // Adjust as needed
            float stepSize = 0.5f;

            // Search in a grid pattern around the original point
            for (float x = -searchRadius; x <= searchRadius; x += stepSize)
            {
                for (float y = -searchRadius; y <= searchRadius; y += stepSize)
                {
                    Vector3 adjustedPoint = new Vector3(endPoint.x + x, endPoint.y + y, endPoint.z);
                    Node_mouse adjustedNode = buildMap.GetNodeFromWorldPoint(adjustedPoint);

                    if (adjustedNode != null && !adjustedNode.isBoulder)
                    {
                        //Debug.Log($"Adjusted end point to {adjustedPoint}.");
                        return adjustedNode; // or continue pathfinding from here
                    }
                }
            }

            // If no valid point is found, log an error
            Debug.LogError($"Unable to find a valid end point near {endPoint}.");
            return null;
        }

        // If the original endPoint is valid, return the endNode
        return endNode;
    }
    // Adjust start point to avoid boulders or invalid nodes
    private Node_mouse AdjustStartPoint(Vector3 startPoint)
    {
        float stepSize = 3f;
        int searchDistance = buildMap.maxX;

        for (int i = 1; i <= searchDistance; i++)
        {
            Vector3[] directions =
            {
            startPoint + Vector3.left * stepSize * i,
            startPoint + Vector3.right * stepSize * i,
        };

            foreach (Vector3 point in directions)
            {
                Node_mouse adjustedNode = buildMap.GetNodeFromWorldPoint(point);
                if (adjustedNode != null && !adjustedNode.isBoulder)
                {
                    //Debug.Log($"Adjusted start point found: {point}");
                    return adjustedNode;
                }
            }
        }

        return null; // No valid start point found
    }




    public List<Node_mouse> RetracePath2(Node_mouse startNode, Node_mouse endNode)
    {
        //Debug.Log("Start node " + startNode.worldPosition + " end node " + endNode.worldPosition);
        List<Node_mouse> retracepath = new List<Node_mouse>();
        Node_mouse currentNode = endNode;

        while (currentNode != startNode)
        {
            retracepath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        retracepath.Add(startNode);
        retracepath.Reverse();

        return retracepath;


    }
    // Method to return the path as a List<Node_mouse>
    public List<Node_mouse> GetNavPath()
    {
        return buildMap.path;
    }

    // Method to return the total cost of the path
    public float FindPathCost(List<Node_mouse> currentPath, float hazardCost)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogError("No valid path found!");
            return -1; // Indicate error (no path found)
        }

        float totalCost = 0;

        // Iterate through the path and sum the gCost of each node
        for (int i = 0; i < currentPath.Count; i++)
        {
            Node_mouse currentNode = currentPath[i];
            totalCost += currentNode.weight * hazardCost;  // Add the cost of the current node's hazard cost
                                                           // Add the distance to the next node if it's not the last node
            if (i < currentPath.Count - 1)
            {
                Node_mouse nextNode = currentPath[i + 1];
                totalCost += Vector3.Distance(currentNode.worldPosition, nextNode.worldPosition) * RoverDriving.timeFactor; // Or use a 2D vector equivalent if applicable
            }

        }
        //Debug.Log("Total path cost: " + totalCost + " || hazard cost is " + hazardCost);
        return totalCost;
    }
    int GetDistance(Node_mouse nodeA, Node_mouse nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
        //return dstX + dstY;
    }


    public float FindPredictedHazardTime(List<Node_mouse> currentPath)
    {
        
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogError("No valid path found!");
            return -1; // Indicate error (no path found)
        }

        float hazardTime = 0;
        int predictedTotalSteps = 0;
        int steps = 0;
        // Iterate through the path and add up time spend if in hazard node
        for(int i=1; i<currentPath.Count; i++)
        {

            if (currentPath[i].weight > 0)
            {
                steps = Mathf.CeilToInt(Vector3.Distance(currentPath[i - 1].worldPosition, currentPath[i].worldPosition) / (RoverDriving.speed * Time.fixedDeltaTime));
                predictedTotalSteps += steps;
                
            }
        }
        
        return hazardTime = predictedTotalSteps * Time.fixedDeltaTime; ;
    }

    public float FindPredictedRouteScore(List<Node_mouse> currentPath)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogError("No valid path found!");
            return -1; // Indicate error (no path found)
        }

        float routeTime = RoverDriving.CalculateRouteTime(currentPath, RoverDriving.speed);
        //Debug.Log("Expected route time is: " + routeTime);
        int numGoalsForScorePred = RoverDriving.totalGoals - RoverDriving.goalCount;
        //Debug.Log("numGoals for scorepred = " + numGoalsForScorePred);
        float routeScore = -routeTime * RoverDriving.timeFactor + numGoalsForScorePred * RoverDriving.goalFactor;   // POSSIBLY FIX IF NOT GETTING ALL GOALS/////
        Debug.Log($"Route time {routeTime} number of goals {numGoalsForScorePred} and initial score {routeScore}");
        //Iterate through the path to calculate the total predicted score
        for (int i = 1; i < currentPath.Count; i++)
        {
            int steps = Mathf.CeilToInt(Vector3.Distance(currentPath[i - 1].worldPosition, currentPath[i].worldPosition) / (RoverDriving.speed * Time.fixedDeltaTime));
            //Debug.Log("steps per node is: " + steps);
            routeScore -= steps * Time.fixedDeltaTime * currentPath[i].weight * RoverDriving.hazardFactor;  // Deductions for time in hazard node
        }
        
        Debug.Log("Predicted score is " + routeScore);
        return routeScore;
    }

    public float FindPredictedRouteScore_PostRound(List<Node_mouse> currentPath)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogError("No valid path found!");
            return -1; // Indicate error (no path found)
        }
        float routeTime = RoverDriving.CalculateRouteTime(currentPath, RoverDriving.speed);
        //Debug.Log("Expected route time is: " + routeTime);
        int numGoalsForScorePred = RoverDriving.goalNumbers.Count;
        Debug.Log("numGoals for scorepred = " + numGoalsForScorePred);
        float routeScore = -routeTime * RoverDriving.timeFactor + numGoalsForScorePred * RoverDriving.goalFactor;   // POSSIBLY FIX IF NOT GETTING ALL GOALS/////
        //Debug.Log($"Route time {routeTime} number of goals {numGoalsForScorePred} and initial score {routeScore}");
        for (int i = 1; i < currentPath.Count; i++)
        {
            int steps = Mathf.CeilToInt(Vector3.Distance(currentPath[i - 1].worldPosition, currentPath[i].worldPosition) / (RoverDriving.speed * Time.fixedDeltaTime));
            //Debug.Log("steps per node is: " + steps);
            routeScore -= steps * Time.fixedDeltaTime * currentPath[i].weight * RoverDriving.hazardFactor;  // Deductions for time in hazard node
        }
        //Debug.Log("Predicted score is " + routeScore);
        return routeScore;
    }
}
