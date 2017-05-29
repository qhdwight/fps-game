using UnityEngine;
using System.Collections;

public class SceneCameraScript : MonoBehaviour {

    public static SceneCameraScript instance = null;
    private Vector3 initialPos, diePos;
    private bool isFloatingUp, firstCinematic = true;

    private void Awake()
    {
        if (instance != null)
            Debug.LogError("More than one scene camera in the scene.");
        else
            instance = this;
    }

    private void Start()
    {
        initialPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (isFloatingUp)
        {
            transform.position += new Vector3(0F, 2F, 0F) * Time.deltaTime;
            transform.position -= transform.forward * 4F * Time.deltaTime;
            Vector3 rot = new Vector3(12F, 0F, 0F) * Time.deltaTime;
            transform.rotation *= Quaternion.Euler(rot);
        }
    }

    private IEnumerator CinematicWait()
    {
        yield return new WaitForSeconds(2F);
        isFloatingUp = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void ReturnToInitialPos()
    {
        transform.position = initialPos;
    }

    public void PlayerDiedCinematic(Vector3 diePos, Vector3 forward)
    {
        // Don't play the cinematic the first time the player dies
        if (firstCinematic)
        {
            firstCinematic = false;
            return;
        }

        SetSceneCameraActive(true);

        this.diePos = diePos;
    
        transform.position = diePos + Vector3.up;
        transform.forward = forward;

        isFloatingUp = true;
        StartCoroutine(CinematicWait());
    }

    public void SetSceneCameraActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
