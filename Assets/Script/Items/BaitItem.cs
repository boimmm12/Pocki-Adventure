using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bait", menuName = "Items/Bait")]
public class BaitItem : ItemBase
{
    [Header("Fishing Parameters")]
    [Tooltip("Multiplier terhadap success rate memancing. Contoh: 0.8 = 80% success")]
    public float successRateMultiplier = 0.5f;

    [System.Serializable]
    public struct RarityWeight
    {
        public FishRarity rarity;
        [Range(0f, 100f)]
        public float weight;
    }

    [Tooltip("Distribusi rarity ikan saat menggunakan umpan ini")]
    public List<RarityWeight> rarityWeights;

    public Dictionary<FishRarity, float> GetRarityChanceDict()
    {
        var dict = new Dictionary<FishRarity, float>();
        float total = 0f;

        foreach (var rw in rarityWeights)
        {
            total += rw.weight;
        }

        foreach (var rw in rarityWeights)
        {
            dict[rw.rarity] = rw.weight / total;
        }

        return dict;
    }

    public FishRarity GetRandomRarity()
    {
        var chances = GetRarityChanceDict();

        Debug.Log("🎣 Rarity Chance Distribution:");
        foreach (var pair in chances)
            Debug.Log($"- {pair.Key}: {pair.Value * 100f:0.00}%");

        float roll = Random.value;
        float cumulative = 0f;

        foreach (var pair in chances)
        {
            cumulative += pair.Value;
            if (roll < cumulative)
                return pair.Key;
        }

        return FishRarity.Common;
    }


    public static FishRarity GetDefaultRandomRarity()
    {
        var defaultWeights = new Dictionary<FishRarity, float>()
    {
        { FishRarity.Common, 0.89f },
        { FishRarity.Uncommon, 0.1f },
        { FishRarity.Rare, 0.007f },
        { FishRarity.Epic, 0.002f },
        { FishRarity.Legendary, 0.001f },
    };

        float roll = Random.value;
        float cumulative = 0f;

        foreach (var pair in defaultWeights)
        {
            cumulative += pair.Value;
            if (roll < cumulative)
                return pair.Key;
        }

        return FishRarity.Common; // fallback
    }

}
