using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public Material material;
    public BlockType[] blocktypes;
    public Transform player;
    [Header("Terrain")]
    public float scale = 0.5f;
    public float offset = 0;
    public float maxNoiseHeight = 24;
    public float minNoiseHeight = 24;
    public int treeDensity = 20;
    public int maxTreeSize = 7;
    public int minTreeSize = 4;
    int noiseHeight;
    public float waterLevel;
    [Header("Caves")]
    public float caveScale;
    public float caveOffset;
    public float caveThreshold;
    public int randomNum = 0;
    List<Vector3> treeCentre;
    Vector3 spawnPos;
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord lastVisitedChunk;
    ChunkCoord playerChunk;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    bool creatingChunks;

    private void Start()
    {
        Random.InitState(seed);

        Vector3 spawnPos = new Vector3((VoxelData.WorldSizeInChunks / 2) * 16, 200, (VoxelData.WorldSizeInChunks / 2) * 16);
        player.transform.position = spawnPos;
        
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

        if (chunksToCreate.Count > 0 && !creatingChunks)
        {
            StartCoroutine("CreateChunks");
        }


    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceChunks; z++)
            {
                chunks[x, z] = new Chunk(this, new ChunkCoord(x, z), true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
    }

    IEnumerator CreateChunks()
    {
        creatingChunks = true;
        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }
        creatingChunks = false;
    }


    ChunkCoord getChunkCoordVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        return chunks[x, z];
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
                        chunks[x, z] = new Chunk(this, new ChunkCoord(x, z), false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                        activeChunks.Add(thisChunk);
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        chunks[x, z].getChunkObj().GetComponent<MeshCollider>().enabled = true;

                    }


                }
                for (int i = 0; i < inactiveChunks.Count; i++)
                {

                    if (inactiveChunks[i].x == x && inactiveChunks[i].z == z)
                    {
                        inactiveChunks.RemoveAt(i);

                    }


                }
            }
        }

        foreach (ChunkCoord coord in inactiveChunks)
        {
            chunks[coord.x, coord.z].isActive = false;
        }


    }

    public bool CheckForBlock(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight)
        {
            return false;
        }

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].areBlocksInChunk)
        {
            return blocktypes[chunks[thisChunk.x, thisChunk.z].GetBlockFromGlobalVector3(pos)].isSolid;
        }

        return blocktypes[GetBlock(pos)].isSolid;
    }

    



    public int GetBlock(Vector3 pos)
    {

        
        randomNum = Random.Range(0, 1000);
        int yPos = Mathf.FloorToInt(pos.y);
        int xPos = Mathf.FloorToInt(pos.x);
        int zPos = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }



        if (pos.y < 1)
        {
            return 1;
        }


        float noise = Mathf.PerlinNoise((pos.x) / VoxelData.chunkWidth * scale + offset, (pos.z) / VoxelData.chunkWidth * scale + offset);

        noiseHeight = Mathf.FloorToInt(maxNoiseHeight * noise + minNoiseHeight);
        int value = 0;


        //surface
        if (yPos == noiseHeight)
        {
            value = 3;

        }
        else if (yPos > noiseHeight)
        {
            value = 0;
        }
        else if (yPos < noiseHeight && yPos > noiseHeight - 4)
        {
            value = 4;
        }

        else
        {
            value = 2;
        }




        //caves
        if (value == 2)
        {
            if (yPos > 1 && yPos < 255)
            {
                if (Get3DPerlin(pos, caveOffset, caveScale, caveThreshold))
                {
                    value = 0;
                }
            }
        }

        
        //water
        if(value == 0)
        {
            if(yPos > noiseHeight && yPos <= minNoiseHeight + waterLevel)
            {
                value = 5;
            }
        }

        //trees

        if (value != 5) 
        {
            if (randomNum < treeDensity)
            {
                if (yPos == noiseHeight + 1)
                {
                    value = 6;
                }
            }
        }

        



        return value;

    }


    bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        return ((AB + BC + AC + BA + CB + CA) / 6f > threshold);
    }


    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }
        return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.chunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
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
    public bool isTransparent;

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