using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

static public class SeedBank
{
    // 2D array containing all of the viable seeds for each of the 8 difficulty levels
    static int[][] seeds = {
        new int[] {1,3,5,6,7,10,12,21,25,28,40,55},
        new int[] {13,14,16,20,30,31,32,44,46,61,85,168},
        new int[] {18,31,55,65,66,69,75,81,84,86,153,178},
        new int[] {17,18,24,31,37,42,54,55,56,59,62,75,76,82,116},
        new int[] {1,5,11,13,20,21,26,41,46,47,63,76},
        new int[] {6,7,8,10,11,20,21,27,34,38,42,62},
        new int[] {3,8,9,17,20,21,34,37,45,50,56,58},
        new int[] {1,7,9,11,14,15,16,19,22,23,36,40}
        };

    // 2D array containing the corresponding fuel targets for each of the seeds
    static float[][] targetFuels = {
        new float[] {2.326395f, 1.773693f, 1.983616f, 1.79467f, 1.892138f, 2.161567f, 1.919587f, 2.116057f, 1.820945f, 1.909881f, 2.080431f,1.964568f},
        new float[] {2.017792f, 1.775604f, 2.087845f, 1.776013f, 1.958603f, 1.946653f, 1.900521f, 2.387774f, 1.996242f, 2.084346f,2.058321f,2.094527f},
        new float[] {2.077968f, 1.734087f, 1.87f, 1.993734f, 2.06571f, 1.870788f, 2.062909f, 1.971913f, 1.9004f, 2.005117f,1.970914f,2.21279f},
        new float[] {2.060474f, 1.993729f, 2.012447f, 1.983385f, 1.916821f, 2.15294f, 1.83335f, 2.0809f, 2.604417f, 2.248558f,1.915881f,2.186189f,2.169406f,2.10759f,1.975436f},
        new float[] {1.924f,2.158f,2.004f,1.76f,1.831f,1.786f,1.947f,2.063f,1.947f,1.907f,2.108426f,2.462487f},
        new float[] {2.233f,1.956f,2.066f,1.921f,2.033f,2.089f,1.885f,1.912f,1.826f,1.845f,2.1046f,2.0993f},
        new float[] {2.192031f,2.47142f,2.336946f,2.355671f,2.296194f,2.411698f,2.314358f,2.002473f,2.121198f,2.249044f,2.46568f,2.38787f},
        new float[] {2.431f,2.441f,2.458f,2.057f,2.208f,2.023f,2.215f,2.25f,2.234f,2.221f,2.287f,2.967f}
        };

    // Function that gets the navigation seed and sets 
    public static void getNavSeed(SimData playerData)
    {
        int terrainLevel = Convert.ToInt32(Math.Ceiling((double)playerData.terrainDifficulty / 3)) - 1;

        int[] levelSeeds = seeds[terrainLevel];
        int levelLen = levelSeeds.Length;
        int randy = UnityEngine.Random.Range(0, levelLen);
        playerData.seed = levelSeeds[randy];
        playerData.navTarget = targetFuels[terrainLevel][randy] + getFuelEstLeeway(playerData.terrainDifficulty);
        Debug.LogFormat("SEED: {0}", playerData.seed);
    }

    /*
     * primarily for sandbox mode, since getNavSeed only gets called during actual trinity setup
     */
    public static float getSandboxFuelEst(int difficulty, int seed)
    {
        int terrainLevel = Convert.ToInt32(Math.Ceiling((double)difficulty / 3)) - 1;
        int[] levelSeeds = seeds[terrainLevel];
        int seed_idx = Array.IndexOf(levelSeeds, seed);
        return targetFuels[terrainLevel][seed_idx] + getFuelEstLeeway(difficulty);
    }

    /*
     * adds a bit of allowable fuel consumption
     * level 1 -> 0.2
     * level 2 -> 0.1
     * level 3 -> 0.0
     * level 4 -> 0.2
     * and so on...
     */
    static public float getFuelEstLeeway(int difficulty)
    {
        int n = (((1 + (difficulty/3)) * 3) - difficulty) % 3;
        return 0.1f * n;
    }

}
