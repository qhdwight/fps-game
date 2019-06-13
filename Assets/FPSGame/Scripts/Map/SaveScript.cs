using System.Text.RegularExpressions;
using UnityEngine;

public class SaveScript : MonoBehaviour {

    public static SaveScript instance;

    private static Regex regex = new Regex("^[0-9A-Za-z ]+$");

    [SerializeField] public GameObject canvas;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void SaveMap()
    {
        MapManager.instance.SaveMap();
    }

    public void SaveMapAs(string mapName)
    {
        // Test if map name is legal
        if
        (
                mapName.Trim(' ') == string.Empty
            ||  !regex.IsMatch(mapName)
            ||  mapName.Length > 16
        )
        return;

        MapManager.instance.SaveMapAs(mapName);
    }
}
