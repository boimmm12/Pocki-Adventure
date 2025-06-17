using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FishInventory : MonoBehaviour, ISavable
{
    [SerializeField] private List<CaughtFishData> fishList = new List<CaughtFishData>();
    [SerializeField] private List<BaitSlot> baitList = new List<BaitSlot>();

    public static FishInventory i { get; private set; }

    public event Action OnUpdated;

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (BaitSelectionUI.i == null)
            Debug.LogError("‼️ BaitSelectionUI.i masih null di Start");
    }

    // ==== FISH ====
    public List<CaughtFishData> GetFishList() => fishList;

    public void AddFish(CaughtFishData fish)
    {
        fishList.Add(fish);
        OnUpdated?.Invoke();
    }

    public void RemoveFish(CaughtFishData caughtFish)
    {
        fishList.Remove(caughtFish);
        OnUpdated?.Invoke();
    }

    public void ClearFish()
    {
        fishList.Clear();
        OnUpdated?.Invoke();
    }

    // ==== BAIT ====
    public List<BaitSlot> GetAllBaits() => baitList;

    public void AddBait(BaitItem bait, int amount = 1)
    {
        var slot = baitList.Find(b => b.bait == bait);
        if (slot != null)
            slot.count += amount;
        else
            baitList.Add(new BaitSlot(bait, amount));

        OnUpdated?.Invoke();
    }

    public void RemoveBait(BaitItem bait, int amount = 1)
    {
        var slot = baitList.Find(b => b.bait == bait);
        if (slot != null)
        {
            slot.count -= amount;
            if (slot.count <= 0)
                baitList.Remove(slot);

            OnUpdated?.Invoke();
        }
    }

    // ==== SAVING ====
    public object CaptureState()
    {
        return new FishSaveWrapper
        {
            fishData = fishList.Select(f => new FishSaveData
            {
                fishName = f.FishItem.name,
                weight = f.weight,
                rarity = f.Rarity
            }).ToList(),

            baitData = baitList.Select(b => new BaitSaveData
            {
                baitName = b.bait.name,
                count = b.count
            }).ToList()
        };
    }

    public void RestoreState(object state)
    {
        var saveWrapper = state as FishSaveWrapper;
        fishList.Clear();
        baitList.Clear();

        foreach (var fish in saveWrapper.fishData)
        {
            var fishItem = FishDB.GetObjectByName(fish.fishName);
            if (fishItem != null)
                fishList.Add(new CaughtFishData(fishItem, fish.weight, fish.rarity));
        }

        foreach (var bait in saveWrapper.baitData)
        {
            var baitItem = BaitDB.GetObjectByName(bait.baitName);
            if (baitItem != null)
            {
                baitList.Add(new BaitSlot(baitItem, bait.count));
            }
        }


        OnUpdated?.Invoke();
    }
}
[System.Serializable]
public class FishSaveWrapper
{
    public List<FishSaveData> fishData;
    public List<BaitSaveData> baitData;
}

[System.Serializable]
public class FishSaveData
{
    public string fishName;
    public float weight;
    public FishRarity rarity;
}

[System.Serializable]
public class BaitSaveData
{
    public string baitName;
    public int count;
}

[System.Serializable]
public class BaitSlot
{
    public BaitItem bait;
    public int count;

    public BaitSlot(BaitItem bait, int count)
    {
        this.bait = bait;
        this.count = count;
    }
}
