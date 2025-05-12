using UnityEngine;

public class Goal
{
    public int GoalNumber { get; set; }
    public Vector2 Position { get; set; }
    public bool IsVisited { get; set; }

    public Goal(int goalNumber, Vector2 position)
    {
        GoalNumber = goalNumber;
        Position = position;
        IsVisited = false;
    }
}
