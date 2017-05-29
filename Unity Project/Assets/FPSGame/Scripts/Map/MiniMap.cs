using UnityEngine;

public class MiniMap : MonoBehaviour {

    public static MiniMap instance;

    public Transform localPlayer = null;
    public int height = 128;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)  
            Destroy(gameObject);
    }

    private void LateUpdate()
    {
        int height = Screen.height, width = Screen.width;
        GetComponent<Camera>().pixelRect = new Rect(8f, height - width*0.2f - 8f, width * 0.2f, width * 0.2f);

        if (localPlayer != null)
        {
            transform.position = localPlayer.position + Vector3.up * 16;
            Vector3 rot = localPlayer.rotation.eulerAngles;
            rot.x = 90f;
            transform.rotation = Quaternion.Euler(rot);
        }
    }
}