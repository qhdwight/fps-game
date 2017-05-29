using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

[Serializable]
public struct BlockData
{
    public int x, y, z;
    public string blockName;
    public BlockData(int x, int y, int z, string blockName)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.blockName = blockName;
    }
}

[Serializable]
public struct ExplosionData
{
    public ushort radius;
    public WorldPosition pos;
    public ExplosionData(ushort radius, WorldPosition pos)
    {
        this.radius = radius;
        this.pos = pos;
    }
}

public class BlockManager : NetworkBehaviour
{
    public static BlockManager singleton;

    public Dictionary<WorldPosition, Chunk> chunkDictionary = new Dictionary<WorldPosition, Chunk>();

    [SerializeField] public GameObject chunkPrefab;

    // Blocks that are synced
    public class SyncListBlocks : SyncListStruct<BlockData> { }
    public SyncListBlocks changedBlocks = new SyncListBlocks();
    private List<BlockData> clientChangedBlocks = new List<BlockData>();

    // Sync explosions
    public class SyncListExplosions : SyncListStruct<ExplosionData> { }
    public SyncListExplosions explosions = new SyncListExplosions();

    private MapInfo currentMapInfo;
        
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);
    }
        
    private void Start()
    {
        // Set the callbacks for the sync lists
        changedBlocks.Callback = SetChangedBlock;
        explosions.Callback = SetChangedExplosion;
    }

    public uint CreateMap(MapInfo mapInfo, CreateMapInfo? info = null)
    {
        currentMapInfo = mapInfo;
        return CreateChunks(info);
    }

    public void SaveMap(string mapName)
    {
        // Save chunks
        foreach (KeyValuePair<WorldPosition, Chunk> entry in chunkDictionary)
        {
            MapSerialization.SaveChunk(entry.Value, mapName);
        }
    }

    private uint CreateChunks(CreateMapInfo? info = null)
    {
        uint checksum = 0;
        // Get map name to load

        Debug.Log("Map name loaded: " + currentMapInfo.mapName);

        // Create chunks
        for (int x = 0; x < currentMapInfo.chunkAmountX; x++)
        {
            for (int z = 0; z < currentMapInfo.chunkAmountZ; z++)
            {
                for (int y = 0; y < 1; y++)
                {
                    int xx = x * Chunk.chunkSize, yy = y * Chunk.chunkSize, zz = z * Chunk.chunkSize;
                    if (info.HasValue) checksum += CreateChunk(xx, yy, zz, info.Value);
                    else checksum += CreateChunk(xx, yy, zz);
                }
            }
        }
        return checksum;
    }

    private uint CreateChunk(int x, int y, int z, CreateMapInfo? info = null)
    {
        uint checksum = 0;

        //Coordinates of this chunk in the world
        WorldPosition chunkPosition = new WorldPosition(x, y, z);

        //Create the chunk at the coordinates in the world
        GameObject newChunkInstance = Instantiate(chunkPrefab, new Vector3(x, y, z), Quaternion.identity);

        //Set the parent and name so its not just floating around + name isn't (clone)
        newChunkInstance.transform.SetParent(transform);
        newChunkInstance.transform.name = "Chunk";

        //Get the object's chunk script
        Chunk newChunk = newChunkInstance.GetComponent<Chunk>();

        //Set the chunk object's chunk position and its blockmanager
        newChunk.chunkPosition = chunkPosition;
        newChunk.blockManager = this;

        //Add it to the chunks dictionary with the position as the key
        chunkDictionary.Add(chunkPosition, newChunk);

        // Load block data for the new chunk from maps folder
        if (!info.HasValue) MapSerialization.LoadChunk(newChunk, currentMapInfo.mapName, out checksum);
        else newChunk.CreateNewTerrain(info.Value);

        //Update the chunk
        newChunk.update = true;

        return checksum;
    }

    [Client]
    private void SetChangedBlock(SyncListBlocks.Operation op, int itemIndex)
    {
        if (!isServer)
        {
            switch (op)
            {
                case SyncList<BlockData>.Operation.OP_ADD:
                    BlockData blockData = changedBlocks.GetItem(itemIndex);
                    if (blockData.blockName == "Air")
                        RemoveBlockClient(blockData.x, blockData.y, blockData.z, false);
                    else
                        ReplaceBlockClient(blockData.blockName, blockData.x, blockData.y, blockData.z, false);
                    break;
                case SyncList<BlockData>.Operation.OP_REMOVEAT:
                    BlockData blockToBreak = clientChangedBlocks[itemIndex];
                    RemoveBlockClient(blockToBreak.x, blockToBreak.y, blockToBreak.z, false);
                    break;
            }

            clientChangedBlocks.Clear();
            for (int i = 0; i < changedBlocks.Count; i++)
            {
                clientChangedBlocks.Add(changedBlocks.GetItem(i));
            }
        }
    }

    [Client]
    private void SetChangedExplosion(SyncListExplosions.Operation op, int itemIndex)
    {
        if (!isServer)
        {
            ExplosionData explosion = explosions.GetItem(itemIndex);
            DestroyBlocksInRadiusClient(explosion.radius, explosion.pos, true);
        }
    }

    [Client]
    public void SetChangedBlocks()
    {
        foreach (BlockData blockData in changedBlocks)
        {
            if (blockData.blockName == "Air")
                RemoveBlockClient(blockData.x, blockData.y, blockData.z, false);
            else
                ReplaceBlockClient(blockData.blockName, blockData.x, blockData.y, blockData.z, false);
        }
    }

    [Client]
    public void SetChangedExplosions()
    {
        foreach (ExplosionData explosion in explosions)
        {
            DestroyBlocksInRadiusClient(explosion.radius, explosion.pos, true);
        }
    }

    public void FillArea(string blockName, WorldPosition _start, WorldPosition _end)
    {
        WorldPosition start = new WorldPosition(Math.Min(_start.x, _end.x), Math.Min(_start.y, _end.y), Math.Min(_start.z, _end.z));
        WorldPosition end = new WorldPosition(Math.Max(_start.x, _end.x), Math.Max(_start.y, _end.y), Math.Max(_start.z, _end.z));

        int width = Math.Abs(start.x - end.x) + 1;
        int height = Math.Abs(start.y - end.y) + 1;
        int length = Math.Abs(start.z - end.z) + 1;

        Debug.Log("Filling " + width * height * length + " blocks.");

        for (int x = start.x; x < width + start.x; x++)
        {
            for (int y = start.y; y < height + start.y; y++)
            {
                for (int z = start.z; z < length + start.z; z++)
                {
                    //Debug.Log(x + ":" + y + ":" + z);
                    //Debug.Log(blockName);
                    ReplaceBlockClient(blockName, x, y, z, false);
                }
            }
        }
    }

    public void ReplaceArea(string blockNameToReplace, string blockNameReplace, WorldPosition _start, WorldPosition _end)
    {
        WorldPosition start = new WorldPosition(Math.Min(_start.x, _end.x), Math.Min(_start.y, _end.y), Math.Min(_start.z, _end.z));
        WorldPosition end = new WorldPosition(Math.Max(_start.x, _end.x), Math.Max(_start.y, _end.y), Math.Max(_start.z, _end.z));

        int width = Math.Abs(start.x - end.x) + 1;
        int height = Math.Abs(start.y - end.y) + 1;
        int length = Math.Abs(start.z - end.z) + 1;

        Debug.Log("Filling " + width * height * length + " blocks.");

        for (int x = start.x; x < width + start.x; x++)
        {
            for (int y = start.y; y < height + start.y; y++)
            {
                for (int z = start.z; z < length + start.z; z++)
                {
                    Block block = GetBlock(x, y, z);
                    if (blockNameToReplace == "AirBlock")
                    {
                        if (block == null)
                        {
                            ReplaceBlockClient(blockNameReplace, x, y, z, false);
                        }
                    }
                    else if (block != null)
                    {
                        string blockName = block.GetName();
                        if (blockName.Equals(blockNameToReplace))
                        {
                            ReplaceBlockClient(blockNameReplace, x, y, z, false);
                        }
                    }
                }
            }
        }
    }

    [Client]
    public void ReplaceBlockClient(string blockType, int x, int y, int z, bool checkBreakable)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.ReplaceBlock(
                blockType,
                x - chunk.chunkPosition.x,
                y - chunk.chunkPosition.y,
                z - chunk.chunkPosition.z,
                checkBreakable);
            chunk.update = true;

            UpdateChunkIfEqual(x - chunk.chunkPosition.x, 0, new WorldPosition(x - 1, y, z));
            UpdateChunkIfEqual(x - chunk.chunkPosition.x, Chunk.chunkSize - 1, new WorldPosition(x + 1, y, z));
            UpdateChunkIfEqual(z - chunk.chunkPosition.z, 0, new WorldPosition(x, y, z - 1));
            UpdateChunkIfEqual(z - chunk.chunkPosition.z, Chunk.chunkSize - 1, new WorldPosition(x, y, z + 1));
        }
    }
  
    [Server]
    public void DestroyBlocksInRadius(ushort r, WorldPosition pos, bool checkBreakable) {
        // Destroy the blocks on the server
        DestroyBlocksInRadiusClient(r, pos, checkBreakable);

        // Add the explosion to the sync list
        ExplosionData explosion = new ExplosionData(r, pos);
        explosions.Add(explosion);
    }

    [Client]
    public void DestroyBlocksInRadiusClient(ushort r, WorldPosition pos, bool checkBreakable)
    {
        for (int x = -r + pos.x; x < r + pos.x; x++)
        {
            for (int y = -r + pos.y; y < r + pos.y; y++)
            {
                for (int z = -r + pos.z; z < r + pos.z; z++)
                {
                    if (Util.GetDistance(new WorldPosition(x, y, z), pos) < r)
                        RemoveBlockClient(x, y, z, checkBreakable);
                }
            }
        }
    }

    #region SetBlock

    [Server]
    public void SetBlock(string blockType, int x, int y, int z)
    {
        // Set the block on the server
        SetBlockClient(blockType, x, y, z);

        // Add the block to the list of changed blocks
        BlockData blockData = new BlockData(x, y, z, blockType);
        changedBlocks.Add(blockData);
    }

    [Client]
    public void SetBlockClient(string blockType, int x, int y, int z)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.SetBlock(blockType, x - chunk.chunkPosition.x, y - chunk.chunkPosition.y, z - chunk.chunkPosition.z);
            chunk.update = true;

            UpdateChunkIfEqual(x - chunk.chunkPosition.x, 0, new WorldPosition(x - 1, y, z));
            UpdateChunkIfEqual(x - chunk.chunkPosition.x, Chunk.chunkSize - 1, new WorldPosition(x + 1, y, z));
            UpdateChunkIfEqual(z - chunk.chunkPosition.z, 0, new WorldPosition(x, y, z - 1));
            UpdateChunkIfEqual(z - chunk.chunkPosition.z, Chunk.chunkSize - 1, new WorldPosition(x, y, z + 1));

        }
    }
    #endregion

    #region RemoveBlock

    [Server]
    public void RemoveBlock(int x, int y, int z, bool checkBreakable)
    {
        //RpcRemoveBlock(x, y, z);

        RemoveBlockClient(x, y, z, checkBreakable);

        BlockData blockData = new BlockData(x, y, z, "Air");
          
        foreach (BlockData changedBlockData in changedBlocks)
        {
            if (blockData.x == changedBlockData.x && blockData.y == changedBlockData.y && blockData.z == changedBlockData.z)
            {
                changedBlocks.RemoveAt(changedBlocks.IndexOf(changedBlockData));
                return;
            }
        }

        changedBlocks.Add(blockData);
    }

    [Client]
    public void RemoveBlockClient(int x, int y, int z, bool checkBreakable)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.RemoveBlock(
                x - chunk.chunkPosition.x,
                y - chunk.chunkPosition.y,
                z - chunk.chunkPosition.z,
                checkBreakable);
            chunk.update = true;

            UpdateChunkIfEqual(x - chunk.chunkPosition.x, 0, new WorldPosition(x - 1, y, z));
            UpdateChunkIfEqual(x - chunk.chunkPosition.x, Chunk.chunkSize - 1, new WorldPosition(x + 1, y, z));
            UpdateChunkIfEqual(z - chunk.chunkPosition.z, 0, new WorldPosition(x, y, z - 1));
            UpdateChunkIfEqual(z - chunk.chunkPosition.z, Chunk.chunkSize - 1, new WorldPosition(x, y, z + 1));
        }
    }
    #endregion

    private void UpdateChunkIfEqual(int value1, int value2, WorldPosition pos)
    {
        if (value1 == value2)
        {
            Chunk chunk = GetChunk(pos.x, pos.y, pos.z);
            if (chunk != null)
                chunk.update = true;
        }
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        WorldPosition chunkPosition = new WorldPosition();
        float multiple = Chunk.chunkSize;
        chunkPosition.x = Mathf.FloorToInt(x / multiple) * Chunk.chunkSize;
        //chunkPosition.y = Mathf.FloorToInt(y / multiple) * Chunk.chunkSize;
        chunkPosition.y = 0;
        chunkPosition.z = Mathf.FloorToInt(z / multiple) * Chunk.chunkSize;

        Chunk containerChunk = null;

        chunkDictionary.TryGetValue(chunkPosition, out containerChunk);

        return containerChunk;
    }

    public Block GetBlock(Chunk callingChunk, int x, int y, int z)
    {
        Chunk containerChunk = GetChunk(x, y, z);

        if (callingChunk == containerChunk)
            return null;
        else if (containerChunk != null)
        {
            return containerChunk.GetBlock(
                x - containerChunk.chunkPosition.x,
                y - containerChunk.chunkPosition.y,
                z - containerChunk.chunkPosition.z);
        }
        else
            return null;
    }

    public Block GetBlock(int x, int y, int z)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            return chunk.GetBlock(x - chunk.chunkPosition.x, y - chunk.chunkPosition.y, z - chunk.chunkPosition.z);
        }
        else
        {
            return null;
        }
    }
}