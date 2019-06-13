using UnityEngine;
using UnityEngine.UI;

public class SelectionItem : MonoBehaviour
{

    public delegate void SetSelectionDelegate(Tab tab, object selectionVal);
    private SetSelectionDelegate setSelectionCallback;

    public object selectionVal;
    [HideInInspector] public Tab tab;

    [SerializeField] private Text text;

    public void SetData(Tab tab, string displayText, object selectionVal, SetSelectionDelegate setSelectionCallback)
    {
        this.tab = tab;
        text.text = displayText;
        this.selectionVal = selectionVal;
        this.setSelectionCallback = setSelectionCallback;
    }

    public void OnSetSelection()
    {
        setSelectionCallback.Invoke(tab, selectionVal);
    }
}