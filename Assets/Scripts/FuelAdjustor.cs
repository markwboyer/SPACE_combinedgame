using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FuelAdjustor
{
    // The purpose of this script is to account for edge cases that are sometimes produced by A*
    // If an estimate is too high or low, it will be fixed in this script
    public static float adjustEstimate(int diff, float estimate)
    {
        float[] minArr = {0,0,0,0,0,0,0,0};
        float[] maxArr = {0,0,0,0,0,0,0,0};

        float maxFuel = maxArr[diff-1];
        float minFuel = minArr[diff-1];

        if (estimate < minFuel)
        {
            return minFuel;
        }
        if (estimate > maxFuel)
        {
            return maxFuel;
        }
        return estimate;
    }
    /*
        Record of mins and maxs for each difficulty:
        1:
            min: 1.45
            max: 1.7 (max is hard)
        2:
            min:
            max:
        3:
            min:
            max:
        4:
            min:
            max:
        5:
            min:
            max:
        6:
            min:
            max:
        7:
            min:
            max:
        8:
            min:
            max:
    */
}
