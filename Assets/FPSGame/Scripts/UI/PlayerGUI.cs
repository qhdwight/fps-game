using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerGUI : MonoBehaviour {

    public static PlayerGUI singleton;

    public PlayerWeaponManager weaponManager = null;
    public Player player = null;

    public const float HITMARKER_DELAY = 0.2F;

    [SerializeField] public GameObject crosshair, hitmarker;
    [SerializeField] public GameObject healthText;
    [SerializeField] public GameObject ammoText;
    [SerializeField] public GameObject progressBarFill, progressBarContainer;

    private RectTransform fillBarRect;
    private Text _ammoText;

    private Sequence currentSequence;

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        fillBarRect = (RectTransform)progressBarFill.transform;
        _ammoText = ammoText.GetComponent<Text>();
    }

    private void OnGUI()
    {
        if (weaponManager.currentPlayerItem != null && weaponManager != null)
        {
            if (weaponManager.currentPlayerItem is PlayerGun)
            {
                PlayerGun gun = weaponManager.currentPlayerItem as PlayerGun;
                _ammoText.text = (
                    gun.currentAmmo.ToString()
                    + " / "
                    + gun.clipSize.ToString());
            }
            else if (weaponManager.currentPlayerItem is PlayerGrenade)
            {
                PlayerGrenade grenade = weaponManager.currentPlayerItem as PlayerGrenade;
                _ammoText.text = grenade.currentAmmo.ToString();
            }

            healthText.GetComponent<Text>().text =
                player.currentHealth.ToString()
                + " / "
                + player.maxHealth.ToString();
        }
    }

    public void SetHitmarker()
    {
        StartCoroutine(OnSetHitmarker());
    }

    private IEnumerator OnSetHitmarker()
    {
        hitmarker.SetActive(true);
        yield return new WaitForSeconds(HITMARKER_DELAY);
        hitmarker.SetActive(false);
    }

    public void SetProgressBarVisible(bool enabled = true)
    {
        progressBarContainer.SetActive(enabled);
    }

    public void StartProgressBar(float time)
    {
        ResetProgressBar(true);
        RectTransform rt = fillBarRect;
        //float width = ((RectTransform)progressBarContainer.transform).rect.width;
        currentSequence = DOTween.Sequence();
        currentSequence.Append
        (
            DOTween.To
            (
                ()=> rt.offsetMax,
                x=> rt.offsetMax = x,
                new Vector2(0F, fillBarRect.offsetMax.y),
                time
            )
            .OnComplete( ()=> ResetProgressBar() )
        );
        currentSequence.Play();

        // for (ushort i = 0; i < ACCURACY; i++) {
        //     rt.offsetMax = new Vector2(rt.offsetMax.x + inc, rt.offsetMax.y);
        //     yield return new WaitForSecondsRealtime(t);
        // }
    }

    public void CancelProgressBar()
    {
        ResetProgressBar();
    }

    private void ResetProgressBar(bool visible = false)
    {
        SetProgressBarVisible(visible);
        currentSequence.Kill();
        float width = ((RectTransform)progressBarContainer.transform).rect.width;
        fillBarRect.offsetMax = new Vector2(-width, fillBarRect.offsetMax.y);
    }
}
