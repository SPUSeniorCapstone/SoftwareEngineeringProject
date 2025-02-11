using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.AssetImporters;
using UnityEngine;
using static VoxelImporter;

[ScriptedImporter(1, "voxel")]
public class VoxelImporter : ScriptedImporter
{
    public static string materialPath = "Materials/VoxelModel";

    public override void OnImportAsset(AssetImportContext ctx)
    {
        Mesh mesh = LoadVoxelMeshFromFile(ctx.assetPath);
        ctx.AddObjectToAsset("Mesh", mesh);

        var obj = new GameObject();
        ctx.AddObjectToAsset("main obj", obj);
        ctx.SetMainObject(obj);


        var meshfilter = obj.AddComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;
        
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = Resources.Load(materialPath, typeof(Material)) as Material;
    }

    Mesh LoadVoxelMeshFromFile(string path)
    {
        var stream = new MemoryStream(File.ReadAllBytes(path));
        BinaryReader reader = new BinaryReader(stream);

        // Primitive way of checking for valid file format
        if (reader.ReadUInt32() != fileCheckID)
        {
            Debug.LogError("Invalid Voxel Object File");
            return null;
        }

        var size = reader.ReadVector3Int();

        Voxel[,,] voxels = new Voxel[size.x, size.y, size.z];

        Vector3 max = Vector3.zero;
        Vector3 min = Vector3.zero;

        Vector3 center = Vector3.zero;
        int count = 0;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    voxels[x, y, z] = reader.ReadVoxel();
                    if (voxels[x, y, z].solid)
                    {
                        center += new Vector3(x, y, z);
                        count++;
                        //max = new Vector3(Mathf.Max(max.x, x), Mathf.Max(max.y, y), Mathf.Max(max.z, z));
                        //min = new Vector3(Mathf.Min(min.x, x), Mathf.Min(min.y, y), Mathf.Min(min.z, z));
                    }
                }
            }
        }

        center /= count;
        center = center.ToVector3Int();

        //Vector3 center = Vector3.Lerp(max, min, 0.5f);

        int index = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    DrawVoxel(new Vector3Int(x, y, z), voxels[x,y,z]);
                }
            }
        }

        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();

        return mesh;

        void DrawVoxel(Vector3Int pos, Voxel voxel)
        {
            if (!voxel.solid)
            {
                return;
            }

            ///
            /// 
            /// Help from https://github.com/b3agz/Code-A-Game-Like-Minecraft-In-Unity/blob/master/01-the-first-voxel/Assets/Scripts/Chunk.cs
            for (int i = 0; i < 6; i++)
            {
                int X = pos.x + VoxelData.faceChecks[i].x;
                int Y = pos.y + VoxelData.faceChecks[i].y;
                int Z = pos.z + VoxelData.faceChecks[i].z;

                if ( (X < 0 || X > size.x) || (Y < 0 || Y > size.y) || (Z < 0 || Z > size.z) || !voxels[X,Y,Z].solid)
                {
                    vertices.Add(pos + VoxelData.Vertices[VoxelData.Triangles[i, 0]] - center);
                    vertices.Add(pos + VoxelData.Vertices[VoxelData.Triangles[i, 1]] - center);
                    vertices.Add(pos + VoxelData.Vertices[VoxelData.Triangles[i, 2]] - center);
                    vertices.Add(pos + VoxelData.Vertices[VoxelData.Triangles[i, 3]] - center);
                    colors.Add(voxel.color);
                    colors.Add(voxel.color);
                    colors.Add(voxel.color);
                    colors.Add(voxel.color);
                    triangles.Add(index);
                    triangles.Add(index + 1);
                    triangles.Add(index + 2);
                    triangles.Add(index + 2);
                    triangles.Add(index + 1);
                    triangles.Add(index + 3);
                    index += 4;
                }
            }
        }
    }

    const uint fileCheckID = 0x766f7865; // "voxe"  

    [Serializable]
    public struct Voxel
    {
        public bool solid;

        public Color color;

        public Voxel(Color color)
        {
            this.color = color;
            solid = true;
        }


    }
}

public static class VoxelHelper
{
    public static Voxel ReadVoxel(this BinaryReader reader)
    {
        Voxel v = new Voxel();
        v.solid = reader.ReadBoolean();
        v.color = reader.ReadColor32();
        return v;
    }
}