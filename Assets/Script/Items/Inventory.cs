using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { Items, CaptureBalls, Tms, QuestItems }
public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> ballSlots;
    [SerializeField] List<ItemSlot> tmSlots;
    [SerializeField] List<ItemSlot> questItemSlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { slots, ballSlots, tmSlots, questItemSlots };
    }
    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS", "CAPTURE BALLS", "TMs & HMs", "QUEST ITEMS"
    };

    public List<ItemSlot> GetSlotsByCategories(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlots = GetSlotsByCategories(categoryIndex);
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pocki selectedPokemon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        return UseItem(item, selectedPokemon);
    }
    public ItemBase UseItem(ItemBase item, Pocki selectedPokemon)
    {
        Debug.Log($"[UseItem] Memakai item: {item?.name} pada: {selectedPokemon?.Base?.Name}");
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            if (!item.IsReusable)
                RemoveItem(item);

            return item;
        }

        return null;
    }
    public void AddItem(ItemBase item, int count = 1)
    {
        Debug.Log($"[Inventory] AddItem dipanggil: {item.name}, count: {count}");
        int category = (int)GetCatagoryFromItem(item);
        var currentSlots = GetSlotsByCategories(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        currentSlots.Sort((a, b) => string.Compare(a.Item.Name, b.Item.Name));

        OnUpdated?.Invoke();
    }

    public int GetItemCount(ItemBase item)
    {
        int category = (int)GetCatagoryFromItem(item);
        var currentSlots = GetSlotsByCategories(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);

        if (itemSlot != null)
            return itemSlot.Count;
        else
            return 0;
    }
    public void RemoveItem(ItemBase item, int countToRemove = 1)
    {
        int category = (int)GetCatagoryFromItem(item);
        var currentSlots = GetSlotsByCategories(category);

        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count -= countToRemove;
        if (itemSlot.Count == 0)
            currentSlots.Remove(itemSlot);

        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCatagoryFromItem(item);
        var currentSlots = GetSlotsByCategories(category);

        return currentSlots.Exists(slot => slot.Item == item);
    }
    public bool HasItemByName(string itemName)
    {
        return questItemSlots.Any(slot => slot.Item.name == itemName);
    }

    ItemCategory GetCatagoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem || item is EvolutionItem || item is XpScrollItem)
            return ItemCategory.Items;
        else if (item is CaptureBall)
            return ItemCategory.CaptureBalls;
        else if (item is TmItem)
            return ItemCategory.Tms;
        else
            return ItemCategory.QuestItems;
    }
    public int GetCategoryIndex(ItemBase item)
    {
        if (item is RecoveryItem || item is EvolutionItem || item is XpScrollItem)
            return 0; // Items
        else if (item is CaptureBall)
            return 1; // Capture Balls
        else if (item is TmItem)
            return 2; // TMs
        else
            return 3; // Quest Items
    }


    public static Inventory GetInventory()
    {
        if (PlayerController.i == null)
        {
            Debug.LogError("[Inventory] PlayerController.i belum tersedia!");
            return null;
        }

        return PlayerController.i.GetComponent<Inventory>();
    }

    public List<T> GetAllItemsOfType<T>() where T : ItemBase
    {
        var results = new List<T>();

        var allCategories = new List<List<ItemSlot>>() { slots, ballSlots, tmSlots, questItemSlots };

        foreach (var category in allCategories)
        {
            foreach (var slot in category)
            {
                if (slot.Item is T item)
                    results.Add(item);
            }
        }

        return results;
    }


    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            captureBalls = ballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList(),
            questItems = questItemSlots.Select(i => i.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        ballSlots = saveData.captureBalls.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();
        questItemSlots = saveData.questItems.Select(i => new ItemSlot(i)).ToList();

        slots.Sort((a, b) => string.Compare(a.Item.Name, b.Item.Name));
        ballSlots.Sort((a, b) => string.Compare(a.Item.Name, b.Item.Name));
        tmSlots.Sort((a, b) => string.Compare(a.Item.Name, b.Item.Name));
        questItemSlots.Sort((a, b) => string.Compare(a.Item.Name, b.Item.Name));

        allSlots = new List<List<ItemSlot>>() { slots, ballSlots, tmSlots, questItemSlots };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }
    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }
    public int Count
    {
        get => count;
        set => count = value;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> captureBalls;
    public List<ItemSaveData> tms;
    public List<ItemSaveData> questItems;
}