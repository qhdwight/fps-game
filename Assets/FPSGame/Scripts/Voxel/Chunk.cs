using UnityEngine;
using System;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public delegate void BlocksUpdatedDelegate();
    public static BlocksUpdatedDelegate blocksUpdatedCallback;

    public static int chunkSize = 8;
    public static int chunkHeight = 64;
    public int grassDepth = 4;
    public PhysicMaterial colliderMaterial;
    public string blockLayerName;
    public bool update = true;
    public Block[,,] blockArray;
    public WorldPosition chunkPosition;
    public BlockManager blockManager;

    public Mesh mesh;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;

    private ChunkWater waterChunk;

    private void Awake()
    {
        blockArray = new Block[chunkSize, chunkHeight, chunkSize];

        mesh = GetComponent<MeshFilter>().mesh;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.material = colliderMaterial;

        waterChunk = GetComponentInChildren<ChunkWater>();

        gameObject.layer = LayerMask.NameToLayer(blockLayerName);
    }

    private void Update()
    {
        if (update)
        {
            update = false;
            UpdateBlockMeshData();
        }
    }

    public Block GetBlock(int x, int y, int z)
    {
        if ((x < chunkSize && y < chunkHeight - 1 && z < chunkSize) && (x >= 0 && y > 0 && z >= 0))
        {
            return blockArray[x, y, z];
        }
        else
        {
            return blockManager.GetBlock(this, chunkPosition.x + x, chunkPosition.y + y, chunkPosition.z + z);
        }
    }

    public void SetBlock(string blockType, int x, int y, int z)
    {
        //If it is a air block remove the block instead
        if (blockType == "Air")
        {
            RemoveBlock(x, y, z, true);
        }

        if (blockArray[x, y, z] == null)
        {
            blockArray[x, y, z] = (Block)Activator.CreateInstance(Type.GetType(blockType));
        }
    }

    public void ReplaceBlock(string blockType, int x, int y, int z, bool checkBreakable)
    {
        //If it is a air block remove the block instead
        if (blockType == "Air")
        {
            RemoveBlock(x, y, z, checkBreakable);
        }
        else
        {
            blockArray[x, y, z] = (Block)Activator.CreateInstance(Type.GetType(blockType));
        }
    }

    public void RemoveBlock(int x, int y, int z, bool checkBreakable)
    {
        try
        {
            if (blockArray[x, y, z] != null) {
                if (checkBreakable) {
                    if (blockArray[x, y, z].breakable) blockArray[x, y, z] = null;
                } else {
                    blockArray[x, y, z] = null;
                }

                if (blocksUpdatedCallback != null) blocksUpdatedCallback.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Illegal block modified at x: " + x.ToString() + ", y: " + y.ToString() + ", z: " + z.ToString());
        }
    }

    private void CreateBlockCube()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    SetBlock("GrassBlock", x, y, z);
                }
            }
        }
    }

    private void CreateBlockPlane(int height)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                SetBlock("GrassBlock", x, height, z);
                update = true;
            }
        }
    }

    public void ClearChunk()
    {
        //Clear existing block array
        Array.Clear(blockArray, 0, blockArray.Length);
    }

    public void CreateNewTerrain(CreateMapInfo info)    
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                //Calculate height based on perlin noise
                float height = Mathf.PerlinNoise((chunkPosition.x + x) / (float)info.lateralScale, (chunkPosition.z + z) / (float)info.lateralScale);
                int roundedHeight = Mathf.RoundToInt(height * info.verticalScale);

                //Create new blocks
                for (int yOffset = 0; yOffset < info.mapHeight; yOffset++)
                {
                    int y = roundedHeight + yOffset;

                    if (yOffset == 0)
                        SetBlock("BedrockBlock", x, y, z);
                    else if (yOffset < info.mapHeight - grassDepth)
                        SetBlock("StoneBlock", x, y, z);
                    else
                        SetBlock("GrassBlock", x, y, z);
                }

                //Make sure mesh updates
                update = true;
            }
        }
    }

    private void RenderMesh(MeshData meshData)
    {
        // Clear the mesh first
        mesh.Clear();

        // Mesh
        mesh.vertices = meshData.vertexList.ToArray();
        mesh.triangles = meshData.triangleList.ToArray();
        mesh.uv = meshData.UVList.ToArray();
        mesh.RecalculateNormals();

        // Collider
        //Mesh colMesh = new Mesh();
        //colMesh.vertices = meshData.colVertexList.ToArray();
        //colMesh.triangles = meshData.colTriangleList.ToArray();
        meshCollider.sharedMesh = mesh;
    }

    private void UpdateBlockMeshData()
    {
        MeshData blockMeshData = new MeshData(), waterMeshData = new MeshData();

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    Block block = GetBlock(x, y, z);
                    if (block != null)
                    {
                        if (block.isWater)
                            waterMeshData = blockArray[x, y, z].Blockdata(this, x, y, z, waterMeshData);
                        else
                            blockMeshData = blockArray[x, y, z].Blockdata(this, x, y, z, blockMeshData);
                    }
                }
            }
        }

        RenderMesh(blockMeshData);
        waterChunk.RenderWaterMesh(waterMeshData);
    }

    public bool drawGUI = false;

    private void OnGUI()
    {
        if (drawGUI)
        {
            GUI.Label(new Rect(200, 100, 300, 20), "X: " + chunkPosition.x.ToString());
            GUI.Label(new Rect(200, 120, 300, 20), "Y: " + chunkPosition.y.ToString());
            GUI.Label(new Rect(200, 140, 300, 20), "Z: " + chunkPosition.z.ToString());
        }
    }
}
