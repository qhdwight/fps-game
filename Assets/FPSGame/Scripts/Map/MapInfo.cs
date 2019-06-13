using System;
using System.Collections.Generic;

[Serializable]
public class MapInfo
{
    public MapInfo(string mapName, int chunkAmountX, int chunkAmountZ, List<PlayerSpawn> spawns, List<FlagInfo> flags)
    {
        this.chunkAmountX = chunkAmountX;
        this.chunkAmountZ = chunkAmountZ;
        this.spawns = spawns;
        this.flags = flags;
        this.mapName = mapName;
    }

    public int chunkAmountX, chunkAmountZ;
    public List<PlayerSpawn> spawns;
    public List<FlagInfo> flags;
    [NonSerialized] public string mapName;
}
