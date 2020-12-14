using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

    public ChunkCoord coord;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    GameObject chunkObject;
    MeshCollider meshCollider;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public int[,,] blocksInChunk = new int[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    World world;

    private bool _isActive;
    public bool areBlocksInChunk = false;

    public Chunk(World _world, ChunkCoord _coord, bool generateOnLoad)
    {
        coord = _coord;
        world = _world;
        _isActive = true;

        if (generateOnLoad)
        {
            Init();
        }

    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "chunk [" + coord.x + "], [" + coord.z + "]";

        AddBlocksToChunk();
        UpdateChunk();



    }

    void AddBlocksToChunk()
    {

        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {

                    blocksInChunk[x, y, z] = world.GetBlock(new Vector3(x, y, z) + position);


                }
            }
        }
        areBlocksInChunk = true;
    }

    public GameObject getChunkObj()
    {
        return chunkObject;
    }

    void UpdateChunk()
    {

        ClearMeshData();
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if (world.blocktypes[blocksInChunk[x, y, z]].isSolid || world.blocktypes[blocksInChunk[x, y, z]].isTransparent)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }

                }
            }
        }

        CreateMesh();

    }

    void ClearMeshData()
    {
        Object.Destroy(chunkObject.GetComponent<MeshCollider>());
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    bool isBlockInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
        {
            return false;
        }
        return true;
    }

    public void EditVoxel(Vector3 pos, int newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        UpdateSurroundingBlocks(xCheck, yCheck, zCheck);

        blocksInChunk[xCheck, yCheck, zCheck] = newID;


        UpdateChunk();
    }

    void UpdateSurroundingBlocks(int x, int y, int z)
    {
        Vector3 thisBlock = new Vector3(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentBlock = thisBlock + VoxelData.faceChecks[i];

            if (!isBlockInChunk((int)currentBlock.x, (int)currentBlock.y, (int)currentBlock.z))
            {
                world.GetChunkVector3(currentBlock + position).UpdateChunk();
            }
        }
    }

    public bool CheckBlock(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);



        if (!isBlockInChunk(x, y, z))
        {
            return world.CheckForBlock(pos + position);
        }


        return world.blocktypes[blocksInChunk[x, y, z]].isSolid;

    }


    public int GetBlockFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return blocksInChunk[xCheck, yCheck, zCheck];
    }



    void UpdateMeshData(Vector3 pos)
    {

        for (int p = 0; p < 6; p++)
        {

            if (!CheckBlock(pos + VoxelData.faceChecks[p]))
            {

                int blockID = blocksInChunk[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.verts[VoxelData.tris[p, 0]]);
                vertices.Add(pos + VoxelData.verts[VoxelData.tris[p, 1]]);
                vertices.Add(pos + VoxelData.verts[VoxelData.tris[p, 2]]);
                vertices.Add(pos + VoxelData.verts[VoxelData.tris[p, 3]]);

                AddTexture(world.blocktypes[blockID].GetTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;

            }
        }

    }

    void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;

    }

    void AddTexture(int textureID)
    {

        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));


    }

}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int xCoord, int zCoord)
    {
        x = xCoord;
        z = zCoord;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.chunkWidth;
        z = zCheck / VoxelData.chunkWidth;
    }

    public bool CompareChunks(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        }
        else if (other.x == x && other.z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}