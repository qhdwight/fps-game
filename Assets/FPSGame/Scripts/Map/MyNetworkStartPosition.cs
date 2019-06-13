using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkStartPosition : NetworkStartPosition {

    private Color cachedColor;
    private PlayerSpawn info;
    private bool isWorldEdit;

    private SphereCollider sphereCollider;
    private MeshRenderer graphicsRenderer;

    private MapInfo currentMapInfo;

    public Team Team { get { return info.team; } }

    public void SetData(PlayerSpawn info, bool isWorldEdit)
    {
        // Set data
        this.info = info;
        this.isWorldEdit = isWorldEdit;

        Init();
    }

    private void Init()
    {
        if (isWorldEdit)
        {
            // Get components
            sphereCollider = GetComponent<SphereCollider>();
            graphicsRenderer = GetComponentInChildren<MeshRenderer>();

            // Enabled collider and renderer
            sphereCollider.enabled = true;
            graphicsRenderer.enabled = true;

            // Get reference to current map info
            currentMapInfo = MapManager.instance.currentMapInfo;

            // Add selection script
            gameObject.AddComponent<WorldEditSelectable>().Setup(true, SpawnPositionUpdated, SelectedByWorldEdit, Deleted);

            // Change color of spawn
            cachedColor = GameManager.GetTeamColor(info.team);
            cachedColor.a = 0.5F;
            graphicsRenderer.material.color = cachedColor;
        }
    }

    // Callback function
    private void SpawnPositionUpdated(WorldPosition pos, Quaternion rot)
    {
        // Update position in map info
        info.pos = pos;
        info.rot = new MyQuaternion(rot);
    }

    // Callback function
    private void SelectedByWorldEdit(bool selected)
    {
        if (selected) graphicsRenderer.material.color = Util.ShiftColor(cachedColor);
        else graphicsRenderer.material.color = cachedColor;
    }

    // Callback function
    private void Deleted()
    {
        currentMapInfo.spawns.Remove(info);
        Destroy(gameObject);
    }
}