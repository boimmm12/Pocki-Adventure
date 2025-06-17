using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pocki pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pocki pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Pocki pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} 's paralyzed and cant move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pocki pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} 's is not frozen anymore");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has been asleep",
                OnStart = (Pocki pokemon) =>
                {
                    pokemon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"will be asleep for {pokemon.StatusTime} moves");
                },
                OnBeforeMove = (Pocki pokemon) =>
                {
                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping");
                    return false;
                }
            }
        },
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Pocki pokemon) =>
                {
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"will be confused for {pokemon.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Pocki pokemon) =>
                {
                    if(pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} kicked out of confusion!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;
                    if(Random.Range(1,3) == 1)
                        return true;

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"It hurt itself due to confusion");
                    return false;
                }
            }
        },
        {
            ConditionID.regeneration,
            new Condition()
            {
                Name = "Regeneration",
                StartMessage = "started regenerating!",
                OnStart = (Pocki pokemon) =>
                {
                    pokemon.VolatileStatusTime = Random.Range(2, 5);
                    Debug.Log($"{pokemon.Base.Name} will regenerate for {pokemon.VolatileStatusTime} turns.");
                },
                OnBeforeMove = (Pocki pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s regeneration faded.");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is regenerating...");
                    return true;
                },
                OnAfterTurn = (Pocki pokemon) =>
                {
                    int heal = pokemon.MaxHp / 7;
                    pokemon.IncreaseHP(heal);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} restored some HP due to regeneration.");
                }
            }
        },
        {
            ConditionID.leech,
            new Condition()
            {
                Name = "Leech",
                StartMessage = "was cursed with Leech!",
                OnStart = (Pocki pokemon) =>
                {
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"{pokemon.Base.Name} will be leeched for {pokemon.VolatileStatusTime} turns.");
                },
                OnBeforeMove = (Pocki pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s leech faded.");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} feels drained by leech...");
                    return true;
                },
                OnAfterTurn = (Pocki pokemon) =>
                {
                    int damage = pokemon.MaxHp / 9;
                    pokemon.DecreaseHP(damage);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to leech.");

                    var playerPokemon = PockiParty.GetPlayerParty()?.GetHealthyPocki();
                    if (playerPokemon != null)
                    {
                        int healAmount = Mathf.FloorToInt(damage * 0.8f);
                        playerPokemon.IncreaseHP(healAmount);
                        playerPokemon.StatusChanges.Enqueue($"{playerPokemon.Base.Name} absorbed health from leech!");
                    }
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.brn || condition.Id == ConditionID.psn || condition.Id == ConditionID.par || condition.Id == ConditionID.leech)
            return 1.5f;
        else if (condition.Id == ConditionID.regeneration)
            return 0.5f;

        return 1f;
    }
}
public enum ConditionID
{
    none, psn, brn, par, frz, slp,
    confusion, regeneration, leech
}
