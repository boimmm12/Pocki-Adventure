using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<PokemonEncounterRecord> wildPokemons;
    [SerializeField] List<PokemonEncounterRecord> wildWaterPokemons;
    [SerializeField] List<PokemonEncounterRecord> wildSandPokemons;
    [SerializeField] List<PokemonEncounterRecord> wildSnowPokemons;

    [HideInInspector]
    [SerializeField] int totalChance;
    [HideInInspector]
    [SerializeField] int totalChanceWater;
    [HideInInspector]
    [SerializeField] int totalChanceSand;
    private void OnValidate()
    {
        CalculateChancePercentage();
    }

    void Start()
    {
        CalculateChancePercentage();
    }

    void CalculateChancePercentage()
    {
        totalChance = -1;
        totalChanceWater = -1;

        if (wildPokemons.Count > 0)
        {
            totalChance = 0;
            foreach (var record in wildPokemons)
            {
                record.chanceLower = totalChance;
                record.chanceUpper = totalChance + record.changePercentage;

                totalChance = totalChance + record.changePercentage;
            }
        }

        if (wildWaterPokemons.Count > 0)
        {
            totalChanceWater = 0;
            foreach (var record in wildWaterPokemons)
            {
                record.chanceLower = totalChanceWater;
                record.chanceUpper = totalChanceWater + record.changePercentage;

                totalChanceWater = totalChanceWater + record.changePercentage;
            }
        }

        if (wildSandPokemons.Count > 0) // ✅ Untuk pasir
        {
            totalChanceSand = 0;
            foreach (var record in wildSandPokemons)
            {
                record.chanceLower = totalChanceSand;
                record.chanceUpper = totalChanceSand + record.changePercentage;
                totalChanceSand += record.changePercentage;
            }
        }
    }

    public Pocki GetRandomWildPokemon(BattleTrigger trigger)
    {
        List<PokemonEncounterRecord> pokemonList = trigger switch
        {
            BattleTrigger.LongGrass => wildPokemons,
            BattleTrigger.Water => wildWaterPokemons,
            BattleTrigger.Sand => wildSandPokemons,
            BattleTrigger.Snow => wildSnowPokemons,
            _ => wildPokemons
        };

        int randVal = Random.Range(1, 101);
        var pokemonRecord = pokemonList.First(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);

        var levelRange = pokemonRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);

        var wildPokemon = new Pocki(pokemonRecord.pokemon, level);
        wildPokemon.Init();
        return wildPokemon;
    }
}

[System.Serializable]
public class PokemonEncounterRecord
{
    public PockiBase pokemon;
    public Vector2Int levelRange;
    public int changePercentage;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }
}

