using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModelsSave {

    public Dictionary<WorldPosition, int> models = new Dictionary<WorldPosition, int>();

    public ModelsSave(Dictionary<WorldPosition, MapModel> models)
    {
        foreach (KeyValuePair<WorldPosition, MapModel> pair in models)
        {
            this.models.Add(pair.Key, (int)pair.Value.id);
        }
    }
}

[Serializable]
public class NewModelsSave
{
    public Dictionary<WorldPosition, ModelInfo> models = new Dictionary<WorldPosition, ModelInfo>();

    public NewModelsSave(Dictionary<WorldPosition, MapModel> models)
    {
        foreach (KeyValuePair<WorldPosition, MapModel> pair in models)
        {
            this.models.Add(pair.Key, new ModelInfo(pair.Value.id, pair.Value.rotation));
        }
    }

    public NewModelsSave(Dictionary<WorldPosition, ModelInfo> models)
    {
        this.models = models;
    }
}

[Serializable]
public class ModelInfo
{

    public ModelIdentity id;
    public MyQuaternion rotation;

    public ModelInfo(ModelIdentity id, Quaternion rotation)
    {
        this.id = id;
        this.rotation = new MyQuaternion(rotation);
    }
}

[Serializable]
public struct MyQuaternion
{
    public float x, y, z, w;

    public MyQuaternion(Quaternion quat)
    {
        x = quat.x; y = quat.y; z = quat.z; w = quat.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}
