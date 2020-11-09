using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blocktypes;
    public GameObject player;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    private void Start()
    {
        GenerateWorld();
        
    }

    void GenerateWorld()
    {
        for (int x = 0; x < VoxelData.WorldSizeInChunks; x++)
        {
            for (int z = 0; z < VoxelData.WorldSizeInChunks; z++)
            {
                CreateChunk(x, z);
            }
        }
    }

    public int GetBlock(Vector3 pos)
    {

        int yPos = Mathf.FloorToInt(pos.y);
        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }


        //Debug.Log(noise);
        float scale = 0.5f;
        float offset = 100;

        if (pos.y < 1)
        {
            return 1;
        }

        float noise = Mathf.PerlinNoise((pos.x) / VoxelData.chunkWidth * scale + offset, (pos.z) / VoxelData.chunkWidth * scale + offset);
        int height = Mathf.FloorToInt(VoxelData.chunkHeight * noise);
        if (yPos == height)
        {
            return 3;

        }
        else if(yPos > height)
        {
            return 0;
        }
        else if(yPos >= yPos - 3)
        {
            return 4;
        }
        else
        {
            return 2;
        }
    }

    void CreateChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(this, new ChunkCoord(x, z));
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if(coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks -1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }
        return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if(pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.chunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        return false;
    }

}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("ERROR");
                return 0;


        }
    }
}
