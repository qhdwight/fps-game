using UnityEngine;

public static class ModifyTerrain {

    public static WorldPosition GetBlockPos(Vector3 position)
    {

        WorldPosition blockPosition = new WorldPosition(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y),
            Mathf.RoundToInt(position.z)
            );

        return blockPosition;
    }
    
    public static WorldPosition GetBlockPos(RaycastHit hit, bool adjacent = false)
    {
        Vector3 position = new Vector3(
            MoveWithinBlock(hit.point.x, hit.normal.x, adjacent),
            MoveWithinBlock(hit.point.y, hit.normal.y, adjacent),
            MoveWithinBlock(hit.point.z, hit.normal.z, adjacent)
            );

        return GetBlockPos(position);
    }

    static float MoveWithinBlock(float position, float norm, bool adjacent = false)
    {
        if (position - (int)position == 0.5F || position - (int)position == -0.5F)
        {
            if (adjacent)
            {
                position += (norm / 2F);
            }
            else
            {
                position -= (norm / 2F);
            }
        }

        return (float)position;
    }

    public static bool SetBlock(RaycastHit hit, string blockType, bool adjacent)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return false;

        WorldPosition blockPosition = GetBlockPos(hit, adjacent);

        chunk.blockManager.SetBlock(blockType, blockPosition.x, blockPosition.y, blockPosition.z);

        return true;
    }

    public static bool RemoveBlock(RaycastHit hit, bool adjacent, bool checkBreakable)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return false;

        WorldPosition blockPosition = GetBlockPos(hit, adjacent);

        chunk.blockManager.RemoveBlock(blockPosition.x, blockPosition.y, blockPosition.z, checkBreakable);

        return true;
    }

    public static Block GetBlock(RaycastHit hit, bool adjacent)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return null;

        WorldPosition position = GetBlockPos(hit, adjacent);

        Block block = chunk.blockManager.GetBlock(position.x, position.y, position.z);

        return block;
    }
}
