using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

    public ChunkCoord coord;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
    GameObject chunkObject;
    MeshCollider meshCollider;

	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
	List<Vector2> uvs = new List<Vector2> ();

	int[,,] blocksInChunk = new int[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    World world;

    public Chunk(World _world, ChunkCoord _coord)
    {
        coord = _coord;
        world = _world;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "chunk [" + coord.x + "], [" + coord.z + "]";

        AddBlocksToChunk();
        CreateMeshData();
        CreateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

    }

	void AddBlocksToChunk ()
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

	}

    public GameObject getChunkObj()
    {
        return chunkObject;
    }

	void CreateMeshData ()
    {

		for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
			for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
				for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if (world.blocktypes[blocksInChunk[x, y, z]].isSolid)
                    {
                        AddBlockDataToChunk(new Vector3(x, y, z));
                    }
					

				}
			}
		}

	}

    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value);  }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    bool isBlockInChunk(int x, int y, int z)
    {
        if(x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
        {
            return false;
        }
        return true;
    }

	bool CheckBlock (Vector3 pos)
    {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		if (!isBlockInChunk(x,y,z))
        {
            return world.blocktypes[world.GetBlock(pos + position)].isSolid;
        }
			

		return world.blocktypes[blocksInChunk [x, y, z]].isSolid;

	}




    void AddBlockDataToChunk (Vector3 pos)
    {

		for (int p = 0; p < 6; p++)
        { 

			if (!CheckBlock(pos + VoxelData.faceChecks[p]))
            {

                int blockID = blocksInChunk[(int)pos.x, (int)pos.y, (int)pos.z];

				vertices.Add (pos + VoxelData.verts [VoxelData.tris [p, 0]]);
				vertices.Add (pos + VoxelData.verts [VoxelData.tris [p, 1]]);
				vertices.Add (pos + VoxelData.verts [VoxelData.tris [p, 2]]);
				vertices.Add (pos + VoxelData.verts [VoxelData.tris [p, 3]]);

                AddTexture(world.blocktypes[blockID].GetTextureID(p));

				triangles.Add (vertexIndex);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 3);
				vertexIndex += 4;

			}
		}

	}

	void CreateMesh ()
    {

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.uv = uvs.ToArray ();

		mesh.RecalculateNormals ();

		meshFilter.mesh = mesh;

	}

    void AddTexture (int textureID)
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
    
    public ChunkCoord(int xCoord, int zCoord)
    {
        x = xCoord;
        z = zCoord;
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