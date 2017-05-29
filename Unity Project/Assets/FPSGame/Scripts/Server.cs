using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

    public string ip;
    public int port;
    public bool bindToIp;
    public string mapName;
    [SerializeField] private GameObject spawnPointPrefab;
    private NetworkManager manager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(CreateGame()); 
    }

    private IEnumerator CreateGame()
    {
        manager = NetworkManager.singleton;

        MenuScript.instance.startMenuCanvas.SetActive(false);

        manager.networkPort = port;
        manager.networkAddress = ip;
        if (bindToIp)
        {
            manager.serverBindToIP = true;
            manager.serverBindAddress = ip;
        }
        manager.StartHost();

        yield return new WaitForSeconds(1F);

        GameManager.instance.mapName = mapName;

        // Load the map
        MapManager.instance.CreateMap(mapName, true, false);
       
        // Get rid of minimap
        MiniMap.instance.gameObject.SetActive(false);
    }

}
