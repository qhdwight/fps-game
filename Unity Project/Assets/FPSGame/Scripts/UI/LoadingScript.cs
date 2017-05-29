using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScript : MonoBehaviour {

    [SerializeField] private Text loadingText;
    [SerializeField] private Image loadingImage;
    private string loadingString = "Loading";
    private int maxDots = 3;
    private bool rotateImage = false;

    public float rotateSpeed = 50F;

	private void Start () {
        if (loadingText != null)
        {
            loadingText.text = loadingString;
            StartCoroutine(Loading());
        }
        if (loadingImage != null)
        {
            rotateImage = true;
        }
	}

    private void FixedUpdate()
    {
        if (rotateImage)
        {
            loadingImage.rectTransform.Rotate(new Vector3(0F, 0F, rotateSpeed) * Time.deltaTime);
        }
    }

    private IEnumerator Loading()
    {
        do
        {
            yield return new WaitForSeconds(1f);
            if (loadingText.text.Length < loadingString.Length + 2 * maxDots)
                loadingText.text += " .";
            else
                loadingText.text = loadingString;
        }
        while (gameObject.activeSelf);
    }
}
