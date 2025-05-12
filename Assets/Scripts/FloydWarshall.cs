using System.Collections.Generic;
using UnityEngine;


public class FloydWarshall : MonoBehaviour
{
    public GameObject buildMapObject; //BuildMap reference
    private BuildMap buildMap;
    public Vector2[] goalLocations; // Array of all goalLocations retrieved from BuildMap

    public AStarPathfinder pathfinder; // Reference to the A* Pathfinder class

    private float[,] shortestPaths; // Stores shortest paths between all pairs
    private int[,] next; // Stores the next node for path reconstruction


    void Start()
    {
        // Assuming AStarPathfinder is attached to the same GameObject or assign it in the Inspector
        if (pathfinder == null)
        {
            pathfinder = GetComponent<AStarPathfinder>();
            if (pathfinder == null)
            {
                Debug.LogError("Failed to find AStarPathfinder component.");
            }
        }

    }

    // Method to run Floyd-Warshall algorithm including currentPosition
    public void RunFloydWarshall(List<Vector3> allNodes, float hazardCost)
    {
        int n = allNodes.Count;
        shortestPaths = new float[n, n];
        next = new int[n, n];

        // Initialize shortestPaths and next arrays
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    shortestPaths[i, j] = 0;
                }
                else
                {
                    // Assuming you have a way to get the distance between nodes
                    float distance = GetDistanceBetweenNodes(allNodes[i], allNodes[j], hazardCost);
                    shortestPaths[i, j] = distance;
                    next[i, j] = j;
                }
            }
        }

        // Floyd-Warshall algorithm to compute shortest paths
        for (int k = 0; k < n; k++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (shortestPaths[i, k] + shortestPaths[k, j] < shortestPaths[i, j])
                    {
                        shortestPaths[i, j] = shortestPaths[i, k] + shortestPaths[k, j];
                        next[i, j] = next[i, k];
                    }
                }
            }
        }
    }

    // Method to get the distance between two nodes using A* Pathfinder
    private float GetDistanceBetweenNodes(Vector3 nodeA, Vector3 nodeB, float hazardCost)
    {
        // Ensure the AStarPathfinder reference is not null
        if (pathfinder == null)
        {
            Debug.LogError("pathfinder reference is missing.");
            return float.MaxValue; // Return a very high cost if pathfinder is not available
        }

        // Call the pathfinder's FindPath method to calculate the path
        pathfinder.FindPath2(nodeA, nodeB, hazardCost);

        // Get the path and calculate the total cost
        List<Node_mouse> path = pathfinder.GetNavPath();

        if (path == null || path.Count == 0)
        {
            Debug.LogError("No valid path found between nodes " + nodeA + " and " + nodeB);
            return float.MaxValue; // Return a very high cost if no path is found
        }

        // Calculate the total path cost based on the number of nodes or actual path distance
        float totalCost = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalCost += Vector3.Distance(path[i].worldPosition, path[i + 1].worldPosition);
        }

        return totalCost;
    }

    // Method to get the path between two nodes after Floyd-Warshall
    public List<Vector3> GetPath(int start, int end, List<Vector3> allNodes)
    {
        List<Vector3> path = new List<Vector3>();
        if (next[start, end] == -1) return path; // No path exists

        path.Add(allNodes[start]);
        while (start != end)
        {
            start = next[start, end];
            path.Add(allNodes[start]);
        }

        return path;
    }

    // Method to find the optimal path from currentPosition to all goals
    public List<Vector3> FindOptimalPath(Vector3 currentPosition, Vector2[] goalLocations, float hazardCost)
    {
        // Convert goalLocations from Vector2 to Vector3 (if necessary)
        List<Vector3> allNodes = new List<Vector3> { currentPosition }; // Add currentPosition as the first node
        foreach (Vector2 goal in goalLocations)
        {
            allNodes.Add(new Vector3(goal.x, goal.y, 0)); // Assuming it's a 2D plane (z = 0)
        }


        // Step 1: Run Floyd-Warshall to calculate shortest paths
        RunFloydWarshall(allNodes, hazardCost);

        // Step 2: Find the optimal order of visiting the goals
        List<int> visited = new List<int> { 0 }; // Start from currentPosition (index 0)
        List<Vector3> optimalPath = new List<Vector3> { currentPosition };
        int currentNode = 0;

        while (visited.Count < allNodes.Count)
        {
            // Find the nearest unvisited node
            float minDistance = float.MaxValue;
            int nearestNode = -1;

            for (int i = 1; i < allNodes.Count; i++) // Skip currentPosition (index 0)
            {
                if (!visited.Contains(i) && shortestPaths[currentNode, i] < minDistance)
                {
                    minDistance = shortestPaths[currentNode, i];
                    nearestNode = i;
                }
            }

            if (nearestNode != -1)
            {
                // Add the path from currentNode to nearestNode
                optimalPath.AddRange(GetPath(currentNode, nearestNode, allNodes));
                visited.Add(nearestNode);
                currentNode = nearestNode; // Move to the next node
            }
        }
        //Debug.Log("Made it this far in FW");
        //return null;
        return optimalPath; // Return the full path visiting all goals
    }
}



