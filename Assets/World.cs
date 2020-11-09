using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blocktypes;
    public Transform player;
    Vector3 spawnPos;
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord lastVisitedChunk;
    ChunkCoord playerChunk;

    private void Start()
    {
        spawnPos = new Vector3(VoxelData.WorldSizeInChunks / 2, VoxelData.chunkHeight, VoxelData.WorldSizeInChunks / 2);
        GenerateWorld();
        lastVisitedChunk = getChunkCoordVector3(player.position);
        
    }

    private void Update()
    {
        playerChunk = getChunkCoordVector3(player.position);
        if (!getChunkCoordVector3(player.position).Equals(lastVisitedChunk))
        {
            CheckViewDistance();
        }
        
        
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceChunks; z++)
            {

                CreateChunk(x, z);
                

            }
        }
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        player.position = spawnPos;
    }

    ChunkCoord getChunkCoordVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        return new ChunkCoord(x, z);
    }

    private void CheckViewDistance()
    {

        int chunkX = Mathf.FloorToInt(player.position.x / VoxelData.chunkWidth);
        int chunkZ = Mathf.FloorToInt(player.position.z / VoxelData.chunkWidth);

        lastVisitedChunk = playerChunk;

        List<ChunkCoord> inactiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = chunkX - VoxelData.ViewDistanceChunks / 2; x < chunkX + VoxelData.ViewDistanceChunks / 2; x++)
        {
            for (int z = chunkZ - VoxelData.ViewDistanceChunks / 2; z < chunkZ + VoxelData.ViewDistanceChunks / 2; z++)
            {

  
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {

                    ChunkCoord thisChunk = new ChunkCoord(x, z);

                    if (chunks[x, z] == null)
                    {
                        CreateChunk(thisChunk.x, thisChunk.z);
                    }                      
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        chunks[x, z].getChunkObj().GetComponent<MeshCollider>().enabled = true;
                        activeChunks.Add(thisChunk);
                    }

                    for (int i = 0; i < inactiveChunks.Count; i++)
                    {

                        if (inactiveChunks[i].x == x && inactiveChunks[i].z == z)
                        {
                            inactiveChunks.RemoveAt(i);
                            //chunks[x, z].getChunkObj().GetComponent<MeshCollider>().enabled = false;
                        }
                            

                    }

                }
            }
        }

        foreach (ChunkCoord coord in inactiveChunks)
            chunks[coord.x, coord.z].isActive = false;

    }



    public int GetBlock(Vector3 pos)
    {

        int yPos = Mathf.FloorToInt(pos.y);
        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }


        float scale = 0.5f;
        float offset = 100;
        float noiseMaxHeight = 15;
        float noiseMinHeight = 15;

        if (pos.y < 1)
        {
            return 1;
        }

        float noise = Mathf.PerlinNoise((pos.x) / VoxelData.chunkWidth * scale + offset, (pos.z) / VoxelData.chunkWidth * scale + offset);
        int height = Mathf.FloorToInt(noiseMaxHeight * noise +  noiseMinHeight);
        
        if (yPos == height)
        {
            return 3;

        }
        else if(yPos > height)
        {
            return 0;
        }
        else if(yPos < height &&  yPos > height - 4)
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
        activeChunks.Add(new ChunkCoord(x, z));
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
