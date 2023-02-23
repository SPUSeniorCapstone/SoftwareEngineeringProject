using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Generate a 3D Voxel-based world
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    static WorldGenerator instance;

    World world => WorldController.Main.World;

    private void Awake()
    {
        if(instance != null)
        {
            seed = instance.seed;
            scale = instance.scale;
            Destroy(instance.gameObject);
            instance = this;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public int seed;
    public void SetSeed(string seed) { this.seed = int.Parse(seed); }

    int terrainSeed;

    [Range(0.001f, 3.0f)]
    public float scale;
    public void SetScale(float scale) { this.scale = scale; }

    [Range(1, 10)]
    public int octaves;
    [Range(0.001f, 10)]
    public float persistance;
    [Range(0.001f, 1)]
    public float lacunarity;


    public bool useLayerHeights = true;
    public bool useFalloff = false;

    float[,] falloff;
    float[,] terrainMap;

    [Range(1f, Chunk.height)]
    public float eccentricity;

    public WorldLayer[] layers;

    Vector3Int terrainOffset;
    Vector3Int tempOffset;
    Vector3Int humidityOffset;

    public void InitWorldGen(int seed)
    {
        UnityEngine.Random.InitState(seed);
        terrainSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    public void GenerateWorld()
    {

        world.seed = seed;
        InitWorldGen(seed);

        terrainMap = NoiseGenerator.GenerateNoiseMap(Chunk.width * world.widthInChunks, Chunk.length * world.lengthInChunks, terrainSeed, scale, octaves, persistance, lacunarity, Vector2.zero);

        if (useFalloff)
        {
            falloff = NoiseGenerator.GenerateFalloffMap(Chunk.width * world.widthInChunks, Chunk.length * world.lengthInChunks);
        }

        for (int x = 0; x < world.widthInChunks; x++)
        {
            for (int z = 0; z < world.lengthInChunks; z++)
            {
                var pos = new Vector3Int(x, 0, z);
                if (world.Chunks[x, z] == null)
                {
                    world.Chunks[x, z] = Chunk.CreateChunk(pos, GenerateChunk(pos), world.transform, world.worldMaterial);
                }
                else
                {
                    world.Chunks[x, z].SetChunk(pos, GenerateChunk(pos), world.worldMaterial);
                }
            }
        }

        for (int x = 0; x < world.widthInChunks; x++)
        {
            for (int z = 0; z < world.lengthInChunks; z++)
            {
                world.Chunks[x, z].UpdateChunk();
            }
        }

        float width = world.WidthInBlocks * world.BlockScale;
        float height = Chunk.height * world.BlockScale;
        float length = world.LengthInBlock * world.BlockScale;

        Vector3 center = new Vector3(width/2,width,length/2);
        world.bounds = new Bounds(center.Flat(), center);
    }

    public Block[,,] GenerateChunk(Vector3Int position)
    {
        //int terrainOffsetX = terrainOffset.x + (position.x * Chunk.width);
        //int terrainOffsetZ = terrainOffset.z + (position.z * Chunk.length);

        //int tempOffsetX = tempOffset.x + (position.x * Chunk.width);
        //int tempOffsetZ = tempOffset.z + (position.z * Chunk.length);

        //int humidityOffsetX = humidityOffset.x + (position.x * Chunk.width);
        //int humidityOffsetZ = humidityOffset.z + (position.z * Chunk.length);

        Vector2Int offset = new Vector2Int(position.x * Chunk.width, position.z * Chunk.length);

        Block[,,] blocks = new Block[Chunk.width, Chunk.height, Chunk.length];

        for (int x = 0; x < Chunk.width; x++)
        {
            for (int z = 0; z < Chunk.length; z++)
            {
                var h = terrainMap[offset.x + x, offset.y + z];

                if (useFalloff)
                {
                    h -= falloff[offset.x + x, offset.y + z];
                }

                if (h < 0) h = 0;

                int blockID = 0;
                int height = 0;
                int currBlockHeight = 0;

                for(int i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];
                    if (layer.height <= h * eccentricity)
                    {
                        if (useLayerHeights)
                            height = currBlockHeight + layer.thickness;
                        else
                            height = (int)(h * eccentricity);
                        currBlockHeight = height;
                        blockID = layer.BlockID;
                    }
                    else
                    {
                        break;
                    }
                }


                if (height > Chunk.height)
                {
                    height = Chunk.height;
                }

                for (int y = 0; y < height; y++)
                {
                    blocks[x, y, z] = new Block(blockID, new Vector3Int(x, y, z));
                }
            }
        }

        return blocks;
    }


}

[Serializable]
public struct WorldLayer
{
    public string name;
    public int BlockID;
    [Range(0, Chunk.height)]
    public int height;
    [Range(1, Chunk.height)]
    public int thickness;
}
