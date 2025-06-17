using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class DynamicMenuUI : SelectionUI<TextSlot>
{
    public void SetupDynamicMenuButtons(List<TextSlot> items)
    {
        Debug.Log($"[DynamicMenuUI] Jumlah item: {items.Count}");

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

        ClearItems();
        SetItems(items);
    }
    public override void UpdateSelectionInUI()
    {

    }
}
