using System.Collections.Generic;
using UnityEngine;

public enum ModelIdentity
{
    Tree,
    Chair,
    Table,
    PalmTree,
    Grass,
}

public class ModelManager : MonoBehaviour {

    public static ModelManager singleton;

    private WorldPosition lastModelPos;

    [SerializeField] public GameObject[] modelPrefabs;

    private Dictionary<WorldPosition, MapModel> models = new Dictionary<WorldPosition, MapModel>();

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        Chunk.blocksUpdatedCallback += BlocksUpdated;
    }

    private void OnDisable()
    {
        Chunk.blocksUpdatedCallback -= BlocksUpdated;
    }

    public void RemoveLastModelPlaced()
    {
        RemoveModelAtPos(lastModelPos);
    }

    public void RemoveModelAtPos(WorldPosition pos)
    {
        // Get model
        MapModel model;
        if (models.TryGetValue(pos, out model))
        {
            // Remove it from dictionary
            models.Remove(pos);
            // Destroy the GameObject
            Destroy(model.gameObject);
        }
    } 

    public void BlocksUpdated()
    {
        // Loop through each model and check if their anchor block is gone, and is so delete the model
        WorldPosition[] buffer = new WorldPosition[models.Count];
        models.Keys.CopyTo(buffer, 0);
        foreach (WorldPosition pos in buffer)
        {
            MapModel model;
            if (models.TryGetValue(pos, out model))
            {
                model.BlocksUpdated();
                //WorldPosition anchorPos = model.anchorBlock;
                //if (BlockManager.singleton.GetBlock(anchorPos.x, anchorPos.y, anchorPos.z) == null)
                //    RemoveModelAtPos(pos);
            }
        }
    }

    public void AddModelAtPos(WorldPosition pos, Quaternion rotation, ModelIdentity modelId, bool isWorldEdit)  
    {
        // Check that a model doesn't already exist there
        if (models.ContainsKey(pos))
            return;
        // Create model at coordinates
        GameObject modelInstance = Instantiate(modelPrefabs[(int)modelId], new Vector3(pos.x, pos.y + 0.5F, pos.z), rotation, transform);
        // Get Map Model script from instatiated object
        MapModel modelScript = modelInstance.GetComponent<MapModel>();
        // Set data on the script
        modelScript.SetData(pos, rotation, isWorldEdit);
        // Add the model to list of models in the world
        models.Add(pos, modelScript);
        // Set last model position for undo
        lastModelPos = pos;
    }

    public bool MoveModel(MapModel model, WorldPosition newPos)
    {
        // Make sure model isn't already there
        if (!models.ContainsKey(newPos))
        {
            // Move model
            models.Remove(model.pos);
            models.Add(newPos, model);
            return true;
        }
        return false;
    }

    public void SaveModels(string mapName)
    {
        NewModelsSave save = new NewModelsSave(models);
        MapSerialization.SaveModels(mapName, save);
    }

    public void LoadModels(bool isWorldEdit)
    {
        // Get map name from GameManager
        string mapName = GameManager.instance.mapName;
        // Load models save file
        NewModelsSave save = MapSerialization.LoadModels(mapName);
        if (save != null)
        {

            // Create each model in save file
            int num = 0;
            foreach (KeyValuePair<WorldPosition, ModelInfo> pair in save.models)
            {
                num++;
                AddModelAtPos(pair.Key, pair.Value.rotation.ToQuaternion(), pair.Value.id, isWorldEdit);
            }
            Debug.Log(num + " models loaded for map " + mapName);
        }
        else
        {
            Debug.Log("models.bin not found for map " + mapName);
        }
    }
}
