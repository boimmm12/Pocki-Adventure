using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Button confirmBuy;
    [SerializeField] Button closeMenu;
    int selectedItem;

    List<ItemBase> availableItems;
    Action<ItemBase> onItemSelected;
    Action onBack;
    List<ItemSlotUI> slotUIList;
    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack)
    {
        this.availableItems = availableItems;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;

        gameObject.SetActive(true);
        UpdateItemList();
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        var prevSelection = selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

        if (prevSelection != selectedItem)
            UpdateItemSelection(selectedItem);

        confirmBuy.onClick.RemoveAllListeners();
        confirmBuy.onClick.AddListener(() =>
        {
            onItemSelected?.Invoke(availableItems[selectedItem]);
            confirmBuy.gameObject.SetActive(false);
        });

        closeMenu.onClick.RemoveAllListeners();
        closeMenu.onClick.AddListener(() => { onBack?.Invoke(); });
    }

    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        foreach (var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetNameAndPrice(item);

            slotUIList.Add(slotUIObj);
        }

        SetupItemButtons();
        UpdateItemSelection(selectedItem);
    }

    private void UpdateItemSelection(int selectedItem)
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = Color.blue;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        if (availableItems.Count > 0)
        {
            var item = availableItems[selectedItem];
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
    }
    public void SetupItemButtons()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            Button button = slotUIList[i].GetComponent<Button>();

            int index = i;
            button.onClick.AddListener(() =>
            {
                confirmBuy.gameObject.SetActive(true);
                selectedItem = index;
                UpdateItemSelection(selectedItem);
            });
        }
    }
}
