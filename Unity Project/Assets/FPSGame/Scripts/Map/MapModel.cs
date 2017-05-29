using UnityEngine;

public class MapModel : MonoBehaviour {

    [HideInInspector] public WorldPosition pos;
    [HideInInspector] public WorldPosition[] anchorBlocks;
    [HideInInspector] public Quaternion rotation;
    private bool isWorldEdit;
    private MeshRenderer[] meshRenderers;
    private Color[][] cachedColors;

    // ================== Set in the editor ===================
    [SerializeField] public ModelIdentity id;
    [SerializeField] public WorldPosition[] anchorBlockOffsets;
    [SerializeField] public Collider worldEditSelectionTrigger;
    [SerializeField] public bool canMove;
    // ========================================================

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        anchorBlocks = new WorldPosition[anchorBlockOffsets.Length];
    }

    private void Start()
    {
        cachedColors = GetMeshColors(meshRenderers);
    }

    private Color[][] GetMeshColors(MeshRenderer[] meshRenderers)
    {
        Color[][] cachedColors = new Color[meshRenderers.Length][];
        for (int j = 0; j < meshRenderers.Length; j++)
        {
            int numOfMaterials = meshRenderers[j].materials.Length;
            cachedColors[j] = new Color[numOfMaterials];

            for (int i = 0; i < numOfMaterials; i++)
            {
                cachedColors[j][i] = meshRenderers[j].materials[i].color;
            }
        }
        return cachedColors;
    }

    public void SetData(WorldPosition pos, Quaternion rotation, bool isWorldEdit)
    {
        this.pos = pos;
        this.rotation = rotation;
        this.isWorldEdit = isWorldEdit;

        Init();
    }

    public void BlocksUpdated()
    {
        foreach (WorldPosition anchorBlock in anchorBlocks) {
            if (BlockManager.singleton.GetBlock(anchorBlock.x, anchorBlock.y, anchorBlock.z) == null)
                ModelManager.singleton.RemoveModelAtPos(pos);
        }
    }

    public void Init()
    {
        SetAnchorBlocks();

        if (isWorldEdit)
        {
            worldEditSelectionTrigger.enabled = true;
            gameObject.AddComponent<WorldEditSelectable>().Setup(true, PositionUpdated, SelectedByWorldEdit, Deleted);
        }
    }

    public void SetAnchorBlocks()
    {
        for (int i = 0; i < anchorBlockOffsets.Length; i++) {
            anchorBlocks[i] = new WorldPosition(
                pos.x + anchorBlockOffsets[i].x,
                pos.y + anchorBlockOffsets[i].y,
                pos.z + anchorBlockOffsets[i].z);
        }
    }

    private void ShiftMeshColors(bool selected)
    {
        for (int j = 0; j < meshRenderers.Length; j++)
        {
            for (int i = 0; i < meshRenderers[j].materials.Length; i++)
            {
                Color col = selected ? Util.ShiftColor(cachedColors[j][i]) : cachedColors[j][i];
                meshRenderers[j].materials[i].color = col;
            }
        }
    }

    // Callback function
    private void PositionUpdated(WorldPosition newPos, Quaternion newRot)
    {
        // Update position in dictionary and move to position whether or not it went through
        bool successfullMove = ModelManager.singleton.MoveModel(this, newPos);
        transform.position = successfullMove
            ? Util.WorldPosToVector3(newPos)
            : Util.WorldPosToVector3(pos);
        if (successfullMove) { pos = newPos; rotation = newRot; }
    }

    // Callback function
    private void SelectedByWorldEdit(bool selected)
    {
        ShiftMeshColors(selected);
    }

    // Callback function
    private void Deleted()
    {
        ModelManager.singleton.RemoveModelAtPos(pos);
    }
}
    