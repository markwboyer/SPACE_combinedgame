using UnityEngine;

public class Node_mouse
{
    public bool isBoulder;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public float weight;  // This will store the Z value as the movement cost or weight

    public int gCost;  // Distance from the start node
    public int hCost;  // Heuristic (distance from the end node)
    public Node_mouse parent;

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public Node_mouse(bool _isBoulder, Vector3 _worldPosition, int _gridX, int _gridY, float _weight)
    {
        isBoulder = _isBoulder;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
        weight = _weight;
    }
}
