using System.Collections;
using UnityEngine;

public class WorldEditSelectable : MonoBehaviour {

	public delegate void DataUpdatedDelegate(WorldPosition pos, Quaternion rot);
	public delegate void SelectedDelegate(bool selected);
	public delegate void DeletedDelegate();
	private DataUpdatedDelegate dataUpdatedCallback;
	private SelectedDelegate selectedCallback;
	private DeletedDelegate deletedCallback;

	private bool selected, snapRotation;

	public static bool mousedOver;

	public void Setup(bool snapRotation, DataUpdatedDelegate dataUpdatedCallback, SelectedDelegate selectedCallback, DeletedDelegate deletedCallback)
	{
        this.snapRotation = snapRotation;
		this.dataUpdatedCallback = dataUpdatedCallback;
		this.selectedCallback = selectedCallback;
		this.deletedCallback = deletedCallback;
	}

    public IEnumerator SetSelected(bool selected)
    {
        yield return new WaitForSeconds(0.1F);

        this.selected = selected;
    }

    private void OnMouseEnter()
    {
		mousedOver = true;
		Selected(true);
    }

    private void OnMouseOver()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            StartCoroutine(SetSelected(true));
        }
    }

    private void OnMouseExit()
    {
        if (!selected)
        {
            mousedOver = false;
            Selected(false);
        }
    }

    private void Selected(bool selected)
    {
		selectedCallback.Invoke(selected);
    }

    private void Update()
    {
        if (selected)
        {
            if (Input.GetButtonDown("Delete"))
            {
                deletedCallback.Invoke();
				mousedOver = false;
            }
            if (Input.GetButtonDown("Fire2"))
            {
                /* Flag placed at new position */

                StartCoroutine(SetSelected(false));

                dataUpdatedCallback.Invoke(Util.Vector3ToWorldPos(transform.position), transform.rotation);
            }
            else
            {
                RaycastHit hit;
                if (PlayerWorldEdit.singleton.SendOutBlockRaycast(out hit))
                {
                    transform.position = Util.WorldPosToVector3(ModifyTerrain.GetBlockPos(hit)) + Vector3.up/2;
                    if (snapRotation)
                        transform.rotation = Util.SnapQuaternion(PlayerWorldEdit.singleton.transform.rotation);
                }
            }
        }
    }

	private void OnDisable()
	{
		mousedOver = false;
	}
}
