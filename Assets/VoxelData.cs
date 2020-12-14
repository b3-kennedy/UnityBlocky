using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{

    public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 128;
    public static readonly int WorldSizeInChunks = 50;
    public static readonly int ViewDistanceChunks = 16;

    


    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * chunkWidth; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] verts = new Vector3[8]
    {
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f),
        new Vector3(1.0f,1.0f,0.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(1.0f,0.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f),
        new Vector3(0.0f,1.0f,1.0f),
    };


    public static readonly int[,] tris = new int[6, 4]
    {
        {0,3,1,2 }, //Back
        {5,6,4,7}, //Front
        {3,7,2,6 }, //Top
        {1,5,0,4}, //Bottom
        {4,7,0,3 }, //Left
        {1,2,5,6 } //Right
        
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
{
        new Vector3(0.0f,0.0f,-1.0f), //behind
        new Vector3(0.0f,0.0f,1.0f), //in front
        new Vector3(0.0f,1.0f,0.0f), //above
        new Vector3(0.0f,-1.0f,0.0f), //below
        new Vector3(-1.0f,0.0f,0.0f), //left
        new Vector3(1.0f,0.0f,0.0f)   //right
};

    public static readonly Vector2[] uvs = new Vector2[4]
    {
        new Vector2(0.0f,0.0f),
        new Vector2(0.0f,1.0f),
        new Vector2(1.0f,0.0f),
        new Vector2(1.0f,1.0f),
    };
}
