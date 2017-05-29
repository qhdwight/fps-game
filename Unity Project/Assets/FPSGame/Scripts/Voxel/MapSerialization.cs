using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class MapSerialization
{
    public static string saveFolderName = "maps", mapInfoString = "map_info", mapModelsString = "models";

    public static string SaveLocation(string worldName)
    {
        string saveLocation = saveFolderName + "/" + worldName + "/";

        if (!Directory.Exists(saveLocation))
        {
            Directory.CreateDirectory(saveLocation);
        }

        return saveLocation;
    }

    public static string FileName(WorldPosition chunkLocation)
    {
        string fileName = chunkLocation.x + "," + chunkLocation.y + "," + chunkLocation.z + ".bin";
        return fileName;
    }

    public static void SaveModels(string mapName, ModelsSave save)
    {
        string saveFile = SaveLocation(mapName);
        saveFile += mapModelsString + ".bin";

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, save);
        stream.Close();
    }

    public static void SaveModels(string mapName, NewModelsSave save)
    {
        string saveFile = SaveLocation(mapName);
        saveFile += mapModelsString + ".bin";

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, save);
        stream.Close();
    }

    public static void SaveChunk(Chunk chunk, string mapName)
    {
        Save save = new Save(chunk);
        if (save.blockDictionary.Count == 0)
            return;

        string saveFile = SaveLocation(mapName);
        saveFile += FileName(chunk.chunkPosition);

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, save);
        stream.Close();
    }

    public static void SaveMapInfo(MapInfo mapInfo)
    {
        // Get directory of the MapInfo
        string saveFile = SaveLocation(mapInfo.mapName);
        saveFile += mapInfoString + ".json";

        // Turn the object into json
        string json = JsonUtility.ToJson(mapInfo);
        json = Util.FormatJSON(json);

        try
        {
            // Try to write the json string to the json file
            File.WriteAllText(saveFile, json);
        }
        catch (Exception e)
        {
            // Log the warning
            Debug.LogWarning(e);
        }
    }

    public static MapInfo LoadMapInfo(string mapName)
    {
        // Get directory of the MapInfo
        string saveFile = SaveLocation(mapName);
        saveFile += mapInfoString + ".json";

        // Try to get one big string from the json file
        string jsonFile = File.ReadAllText(saveFile);

        MapInfo mapInfo = null;

        try
        {
            // Try to read the string json to a object
            mapInfo = JsonUtility.FromJson<MapInfo>(jsonFile);
            mapInfo.mapName = mapName;
        }
        catch (Exception e)
        {
            // Log the warning
            Debug.LogWarning(e);
        }
        return mapInfo;
    }

    public static ModelsSave LoadOldModels(string mapName)
    {
        // Get directory of the ModelsSave
        string saveFile = SaveLocation(mapName);
        saveFile += mapModelsString + ".bin";

        // Check if file exists
        if (!File.Exists(saveFile))
            return null;

        // Create formatter and stream
        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

        // Get map info from directory
        ModelsSave save = (ModelsSave)formatter.Deserialize(stream);

        // Close the stream
        stream.Close();

        return save;
    }

    public static NewModelsSave LoadModels(string mapName)
    {
        // Get directory of the ModelsSave
        string saveFile = SaveLocation(mapName);
        saveFile += mapModelsString + ".bin";

        // Check if file exists
        if (!File.Exists(saveFile))
            return null;

        // Create formatter and stream
        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

        // Get map info from directory
        NewModelsSave save = formatter.Deserialize(stream) as NewModelsSave;

        // Close the stream
        stream.Close();

        return save;
    }

    public static bool LoadChunk(Chunk chunk, string mapName, out uint checksum)
    {
        checksum = 0;

        string saveFile = SaveLocation(mapName);
        saveFile += FileName(chunk.chunkPosition);

        if (!File.Exists(saveFile))
            return false;

        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(saveFile, FileMode.Open);

        Save save = (Save)formatter.Deserialize(stream);

        // Set the blocks in the chunk
        foreach (var block in save.blockDictionary)
        {
            checksum += (uint)block.Key.x;
            checksum += (uint)block.Key.y;
            checksum += (uint)block.Key.z;
            chunk.blockArray[block.Key.x, block.Key.y, block.Key.z] = block.Value;
        }

        stream.Close();
        return true;
    }
}