using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

[Serializable]
public struct CreateMapInfo
{
    public CreateMapInfo(int mapSizeX, int mapSizeZ, int mapHeight, int lateralScale, int verticalScale)
    {
        this.mapSizeX = mapSizeX;
        this.mapSizeZ = mapSizeZ;
        this.mapHeight = mapHeight;
        this.lateralScale = lateralScale;
        this.verticalScale = verticalScale;
    }
    public int mapSizeX, mapSizeZ, mapHeight, lateralScale, verticalScale;
}

public enum OtherModel
{
    RedFlag,
    BlueFlag,
    RedSpawn,
    BlueSpawn,
    Spawn,
}

public enum Tab
{
    Blocks = 0, Models = 1, Other = 2
}

public class WorldEditScript : MonoBehaviour {

    public static WorldEditScript instance = null;
    private NetworkManager networkManager = null;
    private int currentMapIndex = 0;
    private string[] localMapList = Directory.GetDirectories("maps/");
    public static string selectedMapName = "";
    [SerializeField] private Text existingMapText, mapXText, mapZText, mapHeightText, lateralScaleText, verticalScaleText, alertText;
    [SerializeField] private GameObject playerPrefab, loadingScreenPrefab;
    [SerializeField] private GameObject crosshairPrefab;

    [SerializeField] public GameObject chooseMapCanvas, createMapCanvas, existingMapCanvas, worldEditMenuCanvas, alertCanvas;

    private const int
        MAX_MAP_SIZE_X = 24,
        MAX_MAP_SIZE_Z = 24,
        MAX_TERRAIN_HEIGHT = 32,
        MAX_LATERAL_SCALE = 48, MAX_VERTICAL_SCALE = 48;
    private CreateMapInfo newMapInfo = new CreateMapInfo(10, 10, 16, 20, 6);

    private GameObject playerWorldEdit;
    public PlayerWorldEdit playerWorldEditScript;

    private static Vector3 defaultWorldEditSpawn = new Vector3(4F, 68F, 4F);

    private void Start()
    {
        // Create random seed
        int randomSeed = (int)DateTime.Now.Ticks;
        UnityEngine.Random.InitState(randomSeed);
        Debug.Log("Random seed: " + randomSeed.ToString());

        // Create the selection entries
        var values = Enum.GetValues(typeof(Tab));
        foreach (var val in values)
        {
            CreateEntries((Tab)val);
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    #region Edit Map Settings

    public void OnExistingMap()
    {
        chooseMapCanvas.SetActive(false);
        existingMapCanvas.SetActive(true);
    }

    public void OnCreateMap()
    {
        chooseMapCanvas.SetActive(false);
        createMapCanvas.SetActive(true);
    }

    public void OnMapLateralScaleUp()
    {
        if (newMapInfo.lateralScale < MAX_LATERAL_SCALE)
            newMapInfo.lateralScale++;

        lateralScaleText.text = "Lateral Scale: " + newMapInfo.lateralScale.ToString();
    }

    public void OnMapLateralScaleDown()
    {
        if (newMapInfo.lateralScale > 1)
            newMapInfo.lateralScale--;

        lateralScaleText.text = "Lateral Scale: " + newMapInfo.lateralScale.ToString();
    }

    public void OnMapVerticalScaleUp()
    {
        if (newMapInfo.verticalScale < MAX_VERTICAL_SCALE)
            newMapInfo.verticalScale++;

        verticalScaleText.text = "Vertical Scale: " + newMapInfo.verticalScale.ToString();
    }

    public void OnMapVerticalScaleDown()
    {
        if (newMapInfo.verticalScale > 1)
            newMapInfo.verticalScale--;

        verticalScaleText.text = "Vertical Scale: " + newMapInfo.verticalScale.ToString();
    }

    public void OnMapHeightUp()
    {
        if (newMapInfo.mapHeight < MAX_TERRAIN_HEIGHT)
            newMapInfo.mapHeight++;

        mapHeightText.text = "Map Height: " + newMapInfo.mapHeight.ToString();
    }

    public void OnMapHeightDown()
    {
        if (newMapInfo.mapHeight > 1)
            newMapInfo.mapHeight--;

        mapHeightText.text = "Map Height: " + newMapInfo.mapHeight.ToString();
    }

    public void OnMapXUp()
    {
        if (newMapInfo.mapSizeX < MAX_MAP_SIZE_X)
            newMapInfo.mapSizeX++;

        // Update text
        mapXText.text = "Map Length: " + (newMapInfo.mapSizeX * Chunk.chunkSize).ToString();
    }

    public void OnMapXDown()
    {
        if (newMapInfo.mapSizeX > 1)
            newMapInfo.mapSizeX--;

        // Update text
        mapXText.text = "Map Length: " + (newMapInfo.mapSizeX * Chunk.chunkSize).ToString();
    }

    public void OnMapZUp()
    {
        if (newMapInfo.mapSizeZ < MAX_MAP_SIZE_X)
            newMapInfo.mapSizeZ++;

        // Update text
        mapZText.text = "Map Width: " + (newMapInfo.mapSizeZ * Chunk.chunkSize).ToString();
    }

    public void OnMapZDown()
    {
        if (newMapInfo.mapSizeZ > 1)
            newMapInfo.mapSizeZ--;

        // Update text
        mapZText.text = "Map Width: " + (newMapInfo.mapSizeZ * Chunk.chunkSize).ToString();
    }

    #endregion

    #region World Edit Functions

    public void Fill()
    {
        worldEditMenuCanvas.SetActive(false);
        alertCanvas.SetActive(true);

        playerWorldEditScript.Fill();
    }

    public void Replace()
    {
        worldEditMenuCanvas.SetActive(false);
        alertCanvas.SetActive(true);

        playerWorldEditScript.Replace();
    }

    public void Remove()
    {
        worldEditMenuCanvas.SetActive(false);
        alertCanvas.SetActive(true);

        playerWorldEditScript.Remove();
    }

    public void SetAlert(string alert)
    {
        alertText.text = alert;
    }

    #endregion

    #region Selection Menu

    [Header("Selection")]
    [SerializeField] public GameObject selectionCanvas;
    [SerializeField] private GameObject[] tabContents;
    [SerializeField] private GameObject tabPrefab;
    private List<GameObject> selectionItem = new List<GameObject>();

    public void SetSelectionCanvas(bool enabled)
    {
        selectionCanvas.SetActive(enabled);
    }

    public Dictionary<object, string>[] dictionaries =
    {
        new Dictionary<object, string>()
        {
            { "BedrockBlock"    , "Bedrock"     },
            { "DirtBlock"       , "Dirt"        },
            { "GrassBlock"      , "Grass"       },
            { "RedBrickBlock"   , "Red Brick"   },
            { "SandBlock"       , "Sand"        },
            { "StoneBlock"      , "Stone"       },
            { "StoneBrickBlock" , "Stone Brick" },
            { "WaterBlock"      , "Water"       },
            { "WoodBlock"       , "Wood"        },
            { "SnowBlock"       , "Snow"        },
        },

        new Dictionary<object, string>()
        {
            { ModelIdentity.Tree    ,   "Tree"      },
            { ModelIdentity.Chair   ,   "Chair"     },
            { ModelIdentity.Table   ,   "Table"     },
            { ModelIdentity.PalmTree,   "Palm Tree" },
            { ModelIdentity.Grass   ,   "Grass"     },
        },

        new Dictionary<object, string>()
        {
            { OtherModel.RedFlag    , "Red Flag"    },
            { OtherModel.BlueFlag   , "Blue Flag"   },
            { OtherModel.RedSpawn   , "Red Spawn"   },
            { OtherModel.BlueSpawn  , "Blue Spawn"  },
            { OtherModel.Spawn      , "Spawn"       },
        },
    };

    public void CreateEntries(Tab tab)
    {
        foreach (KeyValuePair<object, string> entry in dictionaries[(int)tab])
        {
            Transform parent = tabContents[(int)tab].transform;
            GameObject selectionInstance = Instantiate(tabPrefab, parent);
            SelectionItem selectionItem = selectionInstance.GetComponent<SelectionItem>();
            selectionItem.SetData(tab, entry.Value, entry.Key, OnSetSelection);
        }
    }

    public void OnSetSelection(Tab tab, object selection)
    {
        playerWorldEditScript.SetSelection(tab, selection);
        playerWorldEditScript.currentSelectionVal = selection;
    }

    public void OnBlockSet(string newBlockName)
    {
        playerWorldEditScript.currentBlock = newBlockName;
    }

    public void OnSetModel(ModelIdentity modelId)
    {
        playerWorldEditScript.currentModelId = modelId;
    }

    public void OnBlocksTabSelected()
    {
        SetTabOpen(Tab.Blocks);
    }

    public void OnModelsTabSelected()
    {
        SetTabOpen(Tab.Models);
    }

    public void OnOtherTabSelected()
    {
        SetTabOpen(Tab.Other);
    }

    private void SetTabOpen(Tab tab)
    {
        foreach (GameObject content in tabContents)
        {
            content.SetActive(false);
        }
        tabContents[(int)tab].SetActive(true);
    }

    #endregion

    public void OnWorldEdit()
    {
        UpdateMatchMapText();
    }

    public void OnStartNewMap()
    {
        StartCoroutine(StartWorldEdit(true));
    }

    public void OnStartExistingMap()
    {
        StartCoroutine(StartWorldEdit(false));
    }

    public IEnumerator StartWorldEdit(bool isNewMap)
    {
        // Create loading screen
        GameObject loadingScreen = Instantiate(loadingScreenPrefab, Vector3.zero, Quaternion.identity);
        DontDestroyOnLoad(loadingScreen);

        // Start host
        networkManager = NetworkManager.singleton;
        networkManager.StartHost();

        // Wait until loaded into next level
        while (!ClientScene.ready) { yield return null; }
        yield return new WaitForSeconds(0.5F);

        // Create crosshair
        GameObject crosshairInstance = Instantiate(crosshairPrefab);
        crosshairInstance.name = crosshairPrefab.name;

        // Enable save script
        SaveScript.instance.gameObject.SetActive(true);

        // Get rid of world edit canvas
        existingMapCanvas.SetActive(false);
        createMapCanvas.SetActive(false);

        // Get rid of round timer
        RoundInfoScript.singleton.gameObject.SetActive(false);

        /* Create map */
        GameManager.instance.mapName = selectedMapName;
        SceneCameraScript.instance.SetSceneCameraActive(false);

        // Generate terrain if we are creating a new map
        if (isNewMap)
        {
            MapManager.instance.CreateMap(newMapInfo);
        }
        // Else load map from the file
        else
        {
            MapManager.instance.CreateMap(selectedMapName, true, true);
        }

        // Create world edit player
        playerWorldEdit = Instantiate(playerPrefab);
        playerWorldEditScript = playerWorldEdit.GetComponent<PlayerWorldEdit>();

        yield return new WaitForSeconds(0.2F);

        playerWorldEdit.transform.position = defaultWorldEditSpawn;

        // Destroy loading screen
        Destroy(loadingScreen);
    }

    public void OnWorldEditBack()
    {
        // Change back to menu script canvas
        chooseMapCanvas.SetActive(false);
        createMapCanvas.SetActive(false);
        existingMapCanvas.SetActive(false);
        MenuScript.instance.startMenuCanvas.SetActive(true);
    }

    public void OnMatchMapUp()
    {
        if (currentMapIndex < localMapList.Length - 1)
            currentMapIndex++;

        UpdateMatchMapText();
    }

    public void OnMatchMapDown()
    {
        if (currentMapIndex > 0)
            currentMapIndex--;

        UpdateMatchMapText();
    }

    public void UpdateMatchMapText()
    {
        localMapList = Directory.GetDirectories("maps/");

        string shortenedMapName = localMapList[currentMapIndex].Replace("maps/", "");
        selectedMapName = shortenedMapName;

        existingMapText.text = shortenedMapName;
    }
}
