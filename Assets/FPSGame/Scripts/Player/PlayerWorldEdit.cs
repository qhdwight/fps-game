using System.Collections;
using UnityEngine;

public class PlayerWorldEdit : MonoBehaviour {

    [SerializeField] private LayerMask blockLayerMask;
    [SerializeField] private int maxReachDistance;
    [SerializeField] private GameObject spawnPrefab, flagPrefab;

    private MapInfo currentMapInfo;

    [HideInInspector] public Camera cam;
    private bool canPlaceBlock = true;

    public string currentBlock = "DirtBlock";
    public ModelIdentity currentModelId = ModelIdentity.Chair;
    public OtherModel currentOther = OtherModel.RedFlag;
    public Tab currentTab;
    public object currentSelectionVal = "DirtBlock";

    private bool tabKeyDown, tabKeyUp, inventoryKeyDown, inventoryKeyUp;

    public static PlayerWorldEdit singleton;

    private void Awake()
    {
        singleton = this;
    }

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();

        // Set minimap
        MiniMap.instance.localPlayer = transform;
        // Set reference to current map info
        currentMapInfo = MapManager.instance.currentMapInfo;

        CursorManagement.CorrectLockMode();
    }

    private void Update()
    {
        // Inputs

        tabKeyDown = Input.GetButtonDown("Scoreboard");
        tabKeyUp = Input.GetButtonUp("Scoreboard");

        inventoryKeyDown = Input.GetButtonDown("Inventory");
        inventoryKeyUp = Input.GetButtonUp("Inventory");

        // Model and block modification loops
        if (!CursorManagement.IsMenuOpen() && !CursorManagement.IsWorldEditCanvasOpen() && !WorldEditSelectable.mousedOver)
        {
            MapModification();
        }  

        // Manage the tab menu
        if (tabKeyDown)
        {
            WorldEditScript.instance.worldEditMenuCanvas.SetActive(true);

            CursorManagement.CorrectLockMode();
        }
        else if (tabKeyUp)
        {
            WorldEditScript.instance.worldEditMenuCanvas.SetActive(false);

            CursorManagement.CorrectLockMode();
        }
        // Manage the inventory menu
        else if (inventoryKeyDown)
        {
            WorldEditScript.instance.SetSelectionCanvas(true);

            CursorManagement.CorrectLockMode();
        }
        else if (inventoryKeyUp)
        {
            WorldEditScript.instance.SetSelectionCanvas(false);

            CursorManagement.CorrectLockMode();
        }
    }

    public void SetSelection(Tab tab, object val)
    {
        currentTab = tab;
        switch(tab) {
            case Tab.Blocks: {
                currentBlock = (string)val;
                break;
            }
            case Tab.Models: {
                currentModelId = (ModelIdentity)val;
                break;
            }
            case Tab.Other: {
                currentOther = (OtherModel)val;
                break;
            }
        }
    }

    private void MapModification()
    {
        switch (currentTab)
        {
            case Tab.Blocks: {
                TerrainModification();
                break;
            }
            case Tab.Models: {
                ModelModification();
                break;
            }
            case Tab.Other: {
                OtherModification();
                break;
            }
        }
    }

    private void TerrainModification()
    {
        // Destroying blocks
        if ((Input.GetKeyDown(KeyCode.M) || Input.GetButtonDown("Fire1")) && !WorldEditScript.instance.alertCanvas.activeSelf)
        {
            DestroyBlock();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            SetBlock(currentBlock);
        }
    }

    private void ModelModification()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            SetModel(currentModelId);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            ModelManager.singleton.RemoveLastModelPlaced();
        }
    }

    private void OtherModification()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            switch (currentOther)
            {
                case OtherModel.RedFlag:
                    {
                        CreateFlag(Team.Red);
                        break;
                    }
                case OtherModel.BlueFlag:
                    {
                        CreateFlag(Team.Blue);
                        break;
                    }
                case OtherModel.RedSpawn:
                    {
                        CreateSpawn(Team.Red);
                        break;
                    }
                case OtherModel.BlueSpawn:
                    {
                        CreateSpawn(Team.Blue);
                        break;
                    }
                case OtherModel.Spawn:
                    {
                        CreateSpawn(Team.None);
                        break;
                    }
            }
        }
    }

    private void CreateFlag(Team team)
    {
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            // Get world position of hit
            WorldPosition pos = ModifyTerrain.GetBlockPos(hit.point);
            // Create new flag info with info, id should be unique
            FlagInfo flagInfo = new FlagInfo(pos, team);
            // Create the actual flag and add it to the list
            MapManager.instance.CreateFlag(flagInfo, true);
            currentMapInfo.flags.Add(flagInfo);
        }
    }

    private void CreateSpawn(Team team)
    {
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            // Get world position of hit
            WorldPosition pos = ModifyTerrain.GetBlockPos(hit.point);
            // Create new flag info with info, id should be unique
            PlayerSpawn spawnInfo = new PlayerSpawn(pos, transform.rotation, team);
            // Create the actual flag and add it to the list
            MapManager.instance.CreateSpawn(spawnInfo, true);
            currentMapInfo.spawns.Add(spawnInfo);
        }
    }

    private void SetBlock(string blockName)
    {
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            ModifyTerrain.SetBlock(hit, blockName, true);
        }
    }

    private void DestroyBlock()
    {
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            ModifyTerrain.RemoveBlock(hit, false, false);
        }
    }

    private void SetModel(ModelIdentity id)
    {
        RaycastHit hit;
        if (SendOutBlockRaycast(out hit))
        {
            WorldPosition pos = ModifyTerrain.GetBlockPos(hit);
            ModelManager.singleton.AddModelAtPos(pos, Quaternion.identity, id, true);
        }
    }

    public bool SendOutBlockRaycast(out RaycastHit hit)
    {
        return Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxReachDistance, blockLayerMask);
    }

    public void Fill()
    {
        currentTab = Tab.Blocks;
        StartCoroutine(OnFill(currentBlock));
    }

    public void Remove()
    {
        currentTab = Tab.Blocks;
        StartCoroutine(OnFill("Air"));
    }

    public IEnumerator OnFill(string blockName)
    {
        WorldPosition firstBlockPos, secondBlockPos;

        WorldEditScript.instance.SetAlert("Select first block");

        while (!Input.GetButtonDown("Fire1")) { yield return null; }

        RaycastHit hit1;
        if (SendOutBlockRaycast(out hit1))
        {
            firstBlockPos = ModifyTerrain.GetBlockPos(hit1, false);
            Debug.Log("First Block Pos: " + firstBlockPos.x + ", " + firstBlockPos.y + ", " + firstBlockPos.z);

            WorldEditScript.instance.SetAlert("Select second block");

            yield return null;
            while (!Input.GetButtonDown("Fire1")) { yield return null; }

            WorldEditScript.instance.SetAlert("");
            WorldEditScript.instance.alertCanvas.SetActive(false);

            RaycastHit hit2;
            if (SendOutBlockRaycast(out hit2))
            {
                secondBlockPos = ModifyTerrain.GetBlockPos(hit2, false);
                Debug.Log("Second Block Pos: " + secondBlockPos.x + ", " + secondBlockPos.y + ", " + secondBlockPos.z);

                BlockManager.singleton.FillArea(blockName, firstBlockPos, secondBlockPos);
            }
        }
    }

    public void Replace()
    {
        currentTab = Tab.Blocks;
        StartCoroutine(OnReplace());
    }

    public IEnumerator OnReplace()
    {
        string cachedBlock;

        WorldPosition firstBlockPos, secondBlockPos;

        WorldEditScript.instance.SetAlert("Select first block");

        while (!Input.GetButtonDown("Fire1")) { yield return null; }

        RaycastHit hit1;
        if (SendOutBlockRaycast(out hit1))
        {
            firstBlockPos = ModifyTerrain.GetBlockPos(hit1, false);
            Debug.Log("First Block Pos: " + firstBlockPos.x + ", " + firstBlockPos.y + ", " + firstBlockPos.z);

            WorldEditScript.instance.SetAlert("Select second block");

            yield return null;
            while (!Input.GetButtonDown("Fire1")) { yield return null; }

            RaycastHit hit2;
            if (SendOutBlockRaycast(out hit2))
            {
                secondBlockPos = ModifyTerrain.GetBlockPos(hit2, false);
                Debug.Log("Second Block Pos: " + secondBlockPos.x + ", " + secondBlockPos.y + ", " + secondBlockPos.z);

                WorldEditScript.instance.SetAlert("Select block to replace");

                cachedBlock = currentBlock;
                while (currentBlock == cachedBlock) {yield return null;}
                yield return new WaitForSeconds(0.1F);
                string blockNameToReplace = currentBlock;

                WorldEditScript.instance.SetAlert("Select block that will replace previous");

                cachedBlock = currentBlock;
                while (currentBlock == cachedBlock) { yield return null; }
                yield return new WaitForSeconds(0.1F);
                string blockNameReplace = currentBlock;

                WorldEditScript.instance.alertCanvas.SetActive(false);

                BlockManager.singleton.ReplaceArea(blockNameToReplace, blockNameReplace, firstBlockPos, secondBlockPos);
            }
        }
    }

    private bool NumberKeyIsDown()
    {
        return
            Input.GetKeyDown(KeyCode.Alpha0) ||
            Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Alpha6) ||
            Input.GetKeyDown(KeyCode.Alpha7) ||
            Input.GetKeyDown(KeyCode.Alpha8) ||
            Input.GetKeyDown(KeyCode.Alpha9);
    }

    private void OnGUI()
    {
        //GUI.Label(new Rect(64, Screen.height - 32, Screen.width, 64), "1: Stone, 2: Dirt, 3: Grass, 4: Sand, 5: Bedrock, 6: Red Brick, 7: Stone Brick, 8: Wood Block");
        if (Input.GetButton("Debug")) GUI.Label(new Rect(64, Screen.height - 32, Screen.width, 64), "Current  ID: " + currentModelId);

        if (currentSelectionVal != null) {
            string display = string.Empty;
            WorldEditScript.instance.dictionaries[(int)currentTab].TryGetValue(currentSelectionVal, out display);
            GUI.Label(new Rect(64, Screen.height - 64, Screen.width, 64), "Current selection: " + display);
        }
    }
}

