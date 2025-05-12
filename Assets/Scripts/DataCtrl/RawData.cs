using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class RawData : MonoBehaviour
{
    public float navPerformance;
    public float navTarget;
    public float robotDuration;
    public float obsRocks;
    public float obsSelected = -1; // TODO do we want this metric?
    public float obsScore;
}
