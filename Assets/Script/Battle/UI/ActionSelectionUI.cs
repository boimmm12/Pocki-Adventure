using System.Linq;
using GDE.GenericSelectionUI;
using UnityEngine;
using UnityEngine.UI;

public class ActionSelectionUI : SelectionUI<TextSlot>
{
    private void Start()
    {
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }
    public void SetupActionButtons()
    {
        Debug.Log("SetupActionButtons dipanggil");

        var items = GetComponentsInChildren<TextSlot>().ToList();
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            items[i].OnClick = () =>
            {
                Debug.Log($"[TextSlot] Klik: {index}");
                OnItemClicked(index);
                OnConfirmButton();
            };
        }

        SetItems(items);
    }

    public override void UpdateSelectionInUI()
    {
        
    }
}
