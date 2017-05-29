using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {

    public static List<PlayerSpawn> defaultSpawns = new List<PlayerSpawn> { new PlayerSpawn(new WorldPosition(8, 86, 8), Quaternion.identity, Team.None) };

    [SerializeField] private GameObject spawnPointPrefab;
    // Sorted in order of the number associated with the enum
    [SerializeField] private GameObject flagPrefab;
    public MapInfo currentMapInfo;

    public delegate void MapCreated();
    public MapCreated mapCreatedDelegate;

    public static MapManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public uint CreateMap(string mapName, bool isCreator, bool isWorldEdit)
    {
        // Get map info from name
        MapInfo mapInfo = MapSerialization.LoadMapInfo(mapName);
        currentMapInfo = mapInfo;

        // Load chunks
        uint checksum = BlockManager.singleton.CreateMap(mapInfo);

        // Load models
        CreateModels(isWorldEdit);

        // Create spawns
        CreateSpawns(mapInfo, isWorldEdit);

        // Create flags
        if (isCreator) CreateFlags(mapInfo, isWorldEdit);

        /* Map done loading */

        // Maps with no flags wont have a listener, so check if its null
        if (mapCreatedDelegate != null)
            mapCreatedDelegate.Invoke();

        return checksum;
    }

    public uint CreateMap(CreateMapInfo info)
    {
        currentMapInfo = new MapInfo("unnamed", info.mapSizeX, info.mapSizeZ, new List<PlayerSpawn>(), new List<FlagInfo>());

        return BlockManager.singleton.CreateMap(currentMapInfo, info);
    }

    public void SaveMap()
    {
        // Create and save MapInfo
        MapSerialization.SaveMapInfo(currentMapInfo);

        // Save blocks
        BlockManager.singleton.SaveMap(currentMapInfo.mapName);

        // Save models
        ModelManager.singleton.SaveModels(currentMapInfo.mapName);
    }

    public void SaveMapAs(string mapName)
    {
        currentMapInfo.mapName = mapName;
        SaveMap();
    }

    private void CreateModels(bool isWorldEdit)
    {
        ModelManager.singleton.LoadModels(isWorldEdit);
    }

    private void CreateSpawns(MapInfo mapInfo, bool isWorldEdit)
    {
        // Create spawn points
        for (ushort i = 0; i < mapInfo.spawns.Count; i++)
        {
            PlayerSpawn info = mapInfo.spawns[i];
            CreateSpawn(info, isWorldEdit, i);
        }
    }

    public GameObject CreateSpawn(PlayerSpawn info, bool isWorldEdit, int? num = 0)
    {
        GameObject spawnPointInstance = Instantiate(spawnPointPrefab, Util.WorldPosToVector3(info.pos), info.rot.ToQuaternion());
        spawnPointInstance.transform.Translate(Vector3.up/2);
        spawnPointInstance.GetComponent<MyNetworkStartPosition>().SetData(info, isWorldEdit);
        spawnPointInstance.name = info.team.ToString() + "SpawnPoint";
        if (num.HasValue)
            spawnPointInstance.name += (num + 1).ToString();

        return spawnPointInstance;
    }

    private void CreateFlags(MapInfo mapInfo, bool isWorldEdit)
    {
        if (mapInfo.flags == null)
            return;

        // Create flags
        for (ushort i = 0; i < mapInfo.flags.Count; i++)
        {
            FlagInfo flagInfo = mapInfo.flags[i];
            GameObject flagInstance = CreateFlag(flagInfo, isWorldEdit, i);
            NetworkServer.Spawn(flagInstance);
        }
    }

    public GameObject CreateFlag(FlagInfo info, bool isWorldEdit, ushort id = 0)
    {
        // Create the flag
        GameObject flagInstance = Instantiate(flagPrefab, Util.WorldPosToVector3(info.pos), Quaternion.identity);
        flagInstance.name = info.team.ToString() + "Flag";
        flagInstance.transform.Translate(Vector3.up / 2);

        // Set info on flag
        Flag flagInstanceScript = flagInstance.GetComponent<Flag>();
        flagInstanceScript.SetData(info, isWorldEdit, id);

        return flagInstance;
    }

    public FlagInfo GetFlagInfoFromID(ushort id) {
        return currentMapInfo.flags[id];
    }
}
