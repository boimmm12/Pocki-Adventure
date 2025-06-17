using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Fish")]
public class FishItem : ScriptableObject
{
    [Range(0f, 100f)]
    public float CatchChance = 100f; // misal Blue Tang = 89, Eugene = 0.1

    public string FishName;
    public Sprite Icon;

    [Header("Berat (kg)")]
    public float minWeight = 1f;
    public float maxWeight = 5f;

    [Header("Rarity")]
    public FishRarity rarity;

    public int basePricePerKg;

    public float GetRandomWeight()
    {
        return UnityEngine.Random.Range(minWeight, maxWeight);
    }
}

public enum FishRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
[System.Serializable]
public class CaughtFishData
{
    public string fishName;
    public float weight;
    public FishRarity Rarity;

    [NonSerialized]
    public FishItem FishItem;  // Tidak diserialisasi langsung

    public CaughtFishData(FishItem fishItem, float weight, FishRarity rarity)
    {
        FishItem = fishItem;
        this.weight = weight;
        Rarity = rarity;
    }

    public int GetPrice()
    {
        float basePrice = FishItem.basePricePerKg;
        int rarityMultiplier = GetRarityMultiplier(Rarity);
        return Mathf.RoundToInt(basePrice * rarityMultiplier * weight);
    }

    private int GetRarityMultiplier(FishRarity rarity)
    {
        return rarity switch
        {
            FishRarity.Common => 1,
            FishRarity.Uncommon => 2,
            FishRarity.Rare => 3,
            FishRarity.Epic => 5,
            FishRarity.Legendary => 10,
            _ => 1
        };
    }

}






