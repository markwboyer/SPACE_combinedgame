using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainDifficultyParams
{
    public float heightMultiplier;
    public float scale;
    public float persistance;
    public float lacunarity;
    public float fuelEstMultiplier;
}

static public class DifficultyData
{
    static public TerrainDifficultyParams getTerrainDifficulty(int difficultyNumber)
    {
        float[] diffData = new float[5];
        TerrainDifficultyParams parameters = new TerrainDifficultyParams();
        switch (difficultyNumber)
        {
            case 1:
                parameters.heightMultiplier = 24;
                parameters.scale = 100;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 2.25f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;
            case 2:
                parameters.heightMultiplier = 26;
                parameters.scale = 90;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 2.15f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;
            case 3:
                parameters.heightMultiplier = 28;
                parameters.scale = 80;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 2.05f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;
            case 4:
                parameters.heightMultiplier = 30;
                parameters.scale = 70;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 1.95f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;
            case 5:
                parameters.heightMultiplier = 32;
                parameters.scale = 60;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 1.85f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;
            case 6:
                parameters.heightMultiplier = 34;
                parameters.scale = 50;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 1.75f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;
            case 7:
                parameters.heightMultiplier = 36;
                parameters.scale = 40;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 1.65f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;
            case 8:
                parameters.heightMultiplier = 38;
                parameters.scale = 30;
                parameters.persistance = 0.5f;
                parameters.lacunarity = 1.55f;
                parameters.fuelEstMultiplier = 0.0045f;
                break;   
        }
        return parameters;
    }
}
