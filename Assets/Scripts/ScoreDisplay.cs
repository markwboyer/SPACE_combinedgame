using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text predictedScoreText; // UI Text for Predicted Score
    public Text actualScoreText; // UI Text for Actual Score

    public float predictedScore; // Predicted score based on path cost
    public float actualScore; // Actual score accumulating during gameplay

    // Reference to the RoverDriving script for tracking gameplay
    private RoverDriving rover;

    void Start()
    {
        rover = FindObjectOfType<RoverDriving>();

        // Initialize scores
        predictedScore = 0f;
        actualScore = 0f;
        UpdateScoreDisplays();
    }

    // Call this method when generating a path to set the Predicted Score
    public void SetPredictedScore(float pathCost)
    {
        predictedScore = pathCost;
        UpdateScoreDisplays();
    }

    // Call this method during gameplay to update the Actual Score
    public void UpdateActualScore(int goalsReached, float elapsedTime, float hazardTime)
    {
        float goalFactor = rover.goalFactor;
        float timeFactor = rover.timeFactor;
        float hazardFactor = rover.hazardFactor;
        actualScore = (goalsReached * goalFactor) - (elapsedTime * timeFactor) - (hazardTime * hazardFactor);
        UpdateScoreDisplays();
    }

    private void UpdateScoreDisplays()
    {
        if (predictedScoreText != null)
        {
            predictedScoreText.text = $"Next Route: {predictedScore:F1}";
        }

        if (actualScoreText != null)
        {
            actualScoreText.text = $"Current Score: {actualScore:F1}";
        }
    }
}
