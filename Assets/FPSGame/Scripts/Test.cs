using UnityEngine;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    void Start()
    {
        //UpdateModelSaves();
    }

    private void UpdateModelSaves()
    {
        string[] mapNames = { "castle", "apartment", "town" };
        foreach (string mapName in mapNames)
        {
            ModelsSave modelSave = MapSerialization.LoadOldModels(mapName);
            Dictionary<WorldPosition, int> oldDict = modelSave.models;
            Dictionary<WorldPosition, ModelInfo> newDict = new Dictionary<WorldPosition, ModelInfo>();
            foreach (KeyValuePair<WorldPosition, int> entry in oldDict)
            {
                newDict.Add(entry.Key, new ModelInfo((ModelIdentity)entry.Value, Quaternion.identity));
            }
            NewModelsSave newModelsSave = new NewModelsSave(newDict);
            MapSerialization.SaveModels(mapName, newModelsSave);
        }
    }
}
