using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenerator
{
    int terrainResolution;
    float cellSize;


    float scale, eccentricity;
    float layerDivider = 0.5f;
    int seed;



    public TerrainData GenerateTerrainData()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainResolution;



        int resolution = terrainResolution;
        int gridSize = (int)(resolution / cellSize) + 1;
        float[,] heightMap = new float[gridSize, gridSize];
        Debug.Log(resolution);
        float[,] tree = new float[resolution, resolution];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float h = Mathf.PerlinNoise((x + seed) * scale, (y + seed) * scale);
                if (h > layerDivider)
                {
                    h = 1f;
                }
                else if (h < layerDivider - 0.05f)
                {
                    h = 0;
                }
                heightMap[x, y] = h * eccentricity;
            }
        }

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {

                tree[x, y] = heightMap[(int)(x / cellSize), (int)(y / cellSize)];
            }
        }
        Terrain.activeTerrain.terrainData.SetHeightsDelayLOD(0, 0, tree);
        Terrain.activeTerrain.terrainData.SyncHeightmap();

        return terrainData;
    }

    public struct layer
    {
        float height;

    }

}
