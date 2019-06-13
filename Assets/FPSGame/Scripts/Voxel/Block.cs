using UnityEngine;
using System;

[Serializable]
public class Block {

    public delegate MeshData Action<T1, T2, T3, T4, T5, T6>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);

    public enum Direction { north, east, south, west, up, down };

    public struct Tile { public int x; public int y; }
    const float tileSize = 0.25f;
    const float offset = 0.005f;

    public bool breakable;
    public bool isWater;

    public Block()
    {

    }

    private struct AdjacentBlockInfo {
         public AdjacentBlockInfo(int x, int y, int z, Action<Block, Chunk, int, int, int, MeshData> action) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.action = action;
        }

        public int x, y, z;
        public Action<Block, Chunk, int, int, int, MeshData> action;
    }

    private static AdjacentBlockInfo[] adjacentCheck = {
        new AdjacentBlockInfo( 1,  0,  0, (block, chunk, x, y, z, meshData) => block.FaceDataEast(chunk, x, y, z, meshData)),
        new AdjacentBlockInfo(-1,  0,  0, (block, chunk, x, y, z, meshData) => block.FaceDataWest(chunk, x, y, z, meshData)),
        new AdjacentBlockInfo( 0,  1,  0, (block, chunk, x, y, z, meshData) => block.FaceDataUp(chunk, x, y, z, meshData)),
        new AdjacentBlockInfo( 0, -1,  0, (block, chunk, x, y, z, meshData) => block.FaceDataDown(chunk, x, y, z, meshData)),
        new AdjacentBlockInfo( 0,  0,  1, (block, chunk, x, y, z, meshData) => block.FaceDataNorth(chunk, x, y, z, meshData)),
        new AdjacentBlockInfo( 0,  0, -1, (block, chunk, x, y, z, meshData) => block.FaceDataSouth(chunk, x, y, z, meshData))
    };

    public virtual MeshData Blockdata
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        foreach (AdjacentBlockInfo abi in adjacentCheck)
        {
            Block block = chunk.GetBlock(x + abi.x, y + abi.y, z + abi.z);

            if (block == null || (block.isWater && !isWater))
            {
                meshData = abi.action.Invoke(this, chunk, x, y, z, meshData);
            }
        }

        return meshData;
    }

    protected virtual MeshData FaceDataUp
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));

        meshData.AddMeshTriangles();
        meshData.UVList.AddRange(FaceUVs(Direction.up));

        return meshData;
    }

    protected virtual MeshData FaceDataDown
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddMeshTriangles();
        meshData.UVList.AddRange(FaceUVs(Direction.down));

        return meshData;
    }

    protected virtual MeshData FaceDataNorth
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddMeshTriangles();
        meshData.UVList.AddRange(FaceUVs(Direction.north));

        return meshData;
    }

    protected virtual MeshData FaceDataEast
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));

        meshData.AddMeshTriangles();
        meshData.UVList.AddRange(FaceUVs(Direction.east));

        return meshData;
    }

    protected virtual MeshData FaceDataSouth
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddMeshTriangles();
        meshData.UVList.AddRange(FaceUVs(Direction.south));

        return meshData;
    }

    protected virtual MeshData FaceDataWest
        (Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
        meshData.AddMeshVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));

        meshData.AddMeshTriangles();
        meshData.UVList.AddRange(FaceUVs(Direction.west));

        return meshData;
    }

    //===================================================   UV Mapping   ===================================================

    public virtual Tile TexturePosition(Direction direction)
    {
        Tile tile = new Tile();
        tile.x = 0;
        tile.y = 0;

        return tile;
    }

    public virtual Vector2[] FaceUVs(Direction direction)
    {
        Vector2[] UVs = new Vector2[4];
        Tile tilePos = TexturePosition(direction);

        UVs[0] = new Vector2(
            tileSize * tilePos.x + tileSize - offset,
            tileSize * tilePos.y + offset);
        UVs[1] = new Vector2(
            tileSize * tilePos.x + tileSize - offset,
            tileSize * tilePos.y + tileSize - offset);
        UVs[2] = new Vector2(
            tileSize * tilePos.x + offset,
            tileSize * tilePos.y + tileSize - offset);
        UVs[3] = new Vector2(
            tileSize * tilePos.x + offset,
            tileSize * tilePos.y + offset);

        return UVs;
    }

    //======================================================================================================================

    public virtual bool IsSolid(Direction direction)
    {
        switch (direction)
        {
            case Direction.north:
                return true;
            case Direction.east:
                return true;
            case Direction.south:
                return true;
            case Direction.west:
                return true;
            case Direction.up:
                return true;
            case Direction.down:
                return true;
        }

        return false;
    }

    public string GetName()
    {
        return ToName();
    }

    protected virtual string ToName()
    {
        return "Block";
    }

}