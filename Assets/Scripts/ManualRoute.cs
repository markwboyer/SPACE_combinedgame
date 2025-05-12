using System.Collections.Generic;
using UnityEngine;

public class ManualRoute : MonoBehaviour
{
    public GameObject waypointIconPrefab;
    public Material lineMaterial; // Material for the LineRenderer
    private List<Vector3> waypoints = new List<Vector3>();
    private List<GameObject> waypointIcons = new List<GameObject>();
    private bool isCreatingRoute = false;
    private LineRenderer lineRenderer;
    public BuildMap buildMap;
    public AStarPathfinder pathfinder;
    public RoverDriving roverDriving;
    private List<Node_mouse> waypointList = new List<Node_mouse>();
    private List<GameObject> lineObjects = new List<GameObject>();
    private List<Node_mouse> finalPath;
    private List<Node_mouse> manualPath;
    public SAGATPopupManager popupManager;
    private Camera SPACE_mouse_cam;

    void Start()
    {
        // Initialize LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;
        lineRenderer.positionCount = 0;

    }

    public void StartManualRoute()
    {
        GameObject spaceMouseCamObj = GameObject.Find("SPACE_mouse_cam");
        if (spaceMouseCamObj != null)
        {
            SPACE_mouse_cam = spaceMouseCamObj.GetComponent<Camera>();
            
        }
        else
        {
            Debug.LogError("SPACE_mouse_cam not found in the scene.");
        }
        isCreatingRoute = true;
        waypoints.Clear();
        waypointList.Clear();
        ClearWaypointIcons();
        ClearLineRenderers();
        lineRenderer.positionCount = 0; // Clear any previous path from the LineRenderer
        //Debug.Log("Waypoint list count " + waypointList.Count + " Waypoints count " + waypoints.Count);
        // Clear the final path
        if (finalPath != null)
        {
            finalPath.Clear();
            //Debug.Log("Clearing finalPath");
        }
    }

    void Update()
    {
        if (isCreatingRoute && Input.GetMouseButtonDown(0) && !popupManager.SAGATrunning)
        {
            Vector3 waypoint = GetMouseWorldPosition();
            if (waypoint.x < buildMap.maxX && waypoint.x > buildMap.minX && waypoint.y < buildMap.maxY && waypoint.y > buildMap.minY)
            {
                waypoints.Add(waypoint);

                // Instantiate the waypoint icon
                GameObject waypointIcon = Instantiate(waypointIconPrefab, waypoint, Quaternion.identity);

                // Set the layer for the waypoint icon
                SetLayerRecursively(waypointIcon, LayerMask.NameToLayer("MOUSELayer"));

                waypointIcons.Add(waypointIcon);

                Node_mouse newNode = buildMap.GetNodeFromWorldPoint(waypoint);
                waypointList.Add(newNode);

                // Draw the path as new waypoints are added
                DrawPath(waypointList);
            }
        }
    }
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer; // Set the layer for the current object

        // Recursively set the layer for all child objects
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public void CompleteManualRoute()
    {
        if (isCreatingRoute)
        {
            isCreatingRoute = false;
            List<Node_mouse> manualPath = SavePath();
            //Debug.Log("finalPath check  " + manualPath.Count);
            //Debug.Log("Manual path created with " + manualPath.Count + " nodes.");
            roverDriving.SetManualRoute();
            //Debug.Log("Manual path finished with " + manualPath.Count + " nodes.");

        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (SPACE_mouse_cam == null)
        {
            Debug.LogError("SPACE_mouse_cam is not assigned!");
            return Vector3.zero;
        }

        // Get the mouse position in screen space
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert screen position to world position using the SPACE_mouse_cam
        Vector3 mouseWorldPosition = SPACE_mouse_cam.ScreenToWorldPoint(mouseScreenPosition);

        // Set z to 0 if you're working in 2D
        mouseWorldPosition.z = 0;

        Debug.Log($"Clicked Position (World): {mouseWorldPosition}");
        return mouseWorldPosition;
    }

    public void ClearWaypointIcons()
    {
        foreach (var icon in waypointIcons)
        {
            Destroy(icon);
            //Debug.Log("Clearing icons");
        }
        waypointIcons.Clear();

    }

    private void DrawPath(List<Node_mouse> path)
    {
        if (path == null || path.Count < 2) return;

        GameObject lineObject = new GameObject("PathLine");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = path.Count;
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.white;
        
        lineRenderer.alignment = LineAlignment.View; // always faces the camera

        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i].worldPosition);
        }

        SetLayerRecursively(lineObject, LayerMask.NameToLayer("MOUSELayer"));

        // Add this line renderer to a tracking list to clear later
        lineObjects.Add(lineObject);
    }

    public List<Node_mouse> SavePath()
    {
        List<Node_mouse> finalPath = new List<Node_mouse>();

        // Clear old line renderers and waypoints
        ClearLineRenderers();
        ClearWaypointIcons();


        // Get the starting position of the rover
        Node_mouse startNode = buildMap.GetNodeFromWorldPoint(roverDriving.R1rigidbody.position);

        // Check if there are waypoints to process
        if (waypoints.Count > 0)
        {
            // Get the node for the first waypoint
            Node_mouse firstWaypointNode = buildMap.GetNodeFromWorldPoint(waypoints[0]);

            // Find the path from the rover's starting position to the first waypoint
            List<Node_mouse> initialPathSegment = FindPathBetweenNodes(startNode, firstWaypointNode);

            if (initialPathSegment != null && initialPathSegment.Count > 0)
            {
                finalPath.AddRange(initialPathSegment);
            }
            //Debug.Log("finalPath check 1 "+ finalPath.Count);
            // Continue to fill in paths between waypoints
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Node_mouse currentWaypointNode = buildMap.GetNodeFromWorldPoint(waypoints[i]);
                Node_mouse nextWaypointNode = buildMap.GetNodeFromWorldPoint(waypoints[i + 1]);

                List<Node_mouse> pathSegment = FindPathBetweenNodes(currentWaypointNode, nextWaypointNode);

                if (pathSegment != null && pathSegment.Count > 0)
                {
                    finalPath.AddRange(pathSegment);
                }
            }
            //Debug.Log("finalPath check 2 " + finalPath.Count);
            // Optionally, add the last waypoint node explicitly
            finalPath.Add(buildMap.GetNodeFromWorldPoint(waypoints[waypoints.Count - 1]));
            DrawPath(finalPath);
            //Debug.Log("finalPath check 3 " + finalPath.Count);
        }
        //waypoints.Clear();
        //Debug.Log("finalPath check 4 " + finalPath.Count);
        return finalPath;
    }

    public void ClearLineRenderers()
    {
        foreach (var lineObject in lineObjects)
        {
            //Debug.Log("Clearing linerenderers");
            Destroy(lineObject); // Destroy each LineRenderer GameObject
        }
        lineObjects.Clear(); // Clear the list of LineRenderer objects
        //Debug.Log("lineObjects remaining " + lineObjects.Count);
    }
    // This method should use your A* or other pathfinding logic to return a path between two nodes
    private List<Node_mouse> FindPathBetweenNodes(Node_mouse startNode, Node_mouse endNode)
    {
        // Replace this with your pathfinding logic (e.g., A* algorithm)
        return pathfinder.FindPath2(startNode.worldPosition, endNode.worldPosition, 0.0f);
    }

}
