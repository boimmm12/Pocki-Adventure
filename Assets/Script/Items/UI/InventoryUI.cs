using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] private Button closeMenu;
    [SerializeField] private GameObject itemList;
    [SerializeField] private ItemSlotUI itemSlotUI;
    [SerializeField] private Text categoryText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemDescription;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button confirmItem;

    int selectedCategory = 0;
    List<ItemSlotUI> slotUIList;
    private List<ItemSlot> currentSortedSlots;
    private Inventory inventory;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
    }

    private void Start()
    {
        selectedCategory = 0;
        categoryText.text = Inventory.ItemCategories[selectedCategory];

        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;

        leftButton.onClick.AddListener(OnLeftButton);
        rightButton.onClick.AddListener(OnRightButton);
    }

    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        currentSortedSlots = inventory.GetSlotsByCategories(selectedCategory)
            .OrderBy(slot => GetDetailedItemOrder(slot.Item))
            .ThenBy(slot => slot.Item.Name)
            .ToList();

        foreach (var itemSlot in currentSortedSlots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        SetItems(slotUIList.Select(s => s.GetComponent<TextSlot>()).ToList());
        SetupItemButtons();
        UpdateSelectionInUI();
    }

    public void SetupItemButtons()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            Button button = slotUIList[i].GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() =>
            {
                selectedItem = index;
                UpdateSelectionInUI();
            });
        }
    }

    private void OnLeftButton()
    {
        selectedCategory--;
        WrapCategory();
    }

    private void OnRightButton()
    {
        selectedCategory++;
        WrapCategory();
    }

    public override void HandleUpdate()
    {
        int prevCategory = selectedCategory;

        selectedCategory = Mathf.Clamp(selectedCategory, 0, Inventory.ItemCategories.Count - 1);

        if (prevCategory != selectedCategory)
        {
            ResetSelection();
            categoryText.text = Inventory.ItemCategories[selectedCategory];
            UpdateItemList();
        }

        confirmItem.onClick.RemoveAllListeners();
        confirmItem.onClick.AddListener(OnConfirmButton);

        closeMenu.onClick.RemoveAllListeners();
        closeMenu.onClick.AddListener(OnBackButton);

        base.HandleUpdate();
    }

    private void WrapCategory()
    {
        if (selectedCategory > Inventory.ItemCategories.Count - 1)
            selectedCategory = 0;
        else if (selectedCategory < 0)
            selectedCategory = Inventory.ItemCategories.Count - 1;

        categoryText.text = Inventory.ItemCategories[selectedCategory];
        ResetSelection();
        UpdateItemList();
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        if (currentSortedSlots.Count > 0 && selectedItem >= 0 && selectedItem < currentSortedSlots.Count)
        {
            var item = currentSortedSlots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
        else
        {
            itemIcon.sprite = null;
            itemDescription.text = "";
        }
    }

    void ResetSelection()
    {
        selectedItem = 0;
        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    int GetDetailedItemOrder(ItemBase item)
    {
        if (item is RecoveryItem recovery)
        {
            if (recovery.restoreMaxHP || recovery.hpAmount > 0) return 0;
            if (recovery.restoreMaxPP || recovery.ppAmount > 0) return 1;
            if (recovery.revive || recovery.maxRevive) return 2;
            if (recovery.recoverAllStatus || recovery.status != ConditionID.none) return 3;
            return 4;
        }

        if (item is XpScrollItem xp)
            return 10 + xp.ExpAmount;

        if (item is CaptureBall ball)
            return 100 + Mathf.RoundToInt(ball.catchRateModifier * 100);

        if (item is TmItem) return 999;
        return 1000;
    }

    public ItemBase SelectedItem => currentSortedSlots[selectedItem].Item;
    public int SelectedCategory => selectedCategory;
}
