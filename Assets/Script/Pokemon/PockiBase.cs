using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PockiBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PockiType type1;
    [SerializeField] PockiType type2;

    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;
    [SerializeField] int catchRate = 45; // 0 - 255

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [SerializeField] List<Evolution> evolutions;
    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * level * level * level / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.MediumSlow)
        {
            return (6 * level * level * level / 5) - (15 * level * level) + (100 * level) - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * level * level * level / 4;
        }
        else if (growthRate == GrowthRate.Fluctuating)
        {
            if (level < 15)
            {
                return (int)(Mathf.Pow(level, 3) * (Mathf.Floor((level + 1) / 3f) + 24) / 50f);
            }
            else if (level < 36)
            {
                return (int)(Mathf.Pow(level, 3) * (level + 14) / 50f);
            }
            else
            {
                return (int)(Mathf.Pow(level, 3) * (Mathf.Floor(level / 2f) + 32) / 50f);
            }
        }
        else if (growthRate == GrowthRate.Erratic)
        {
            if (level < 25)
            {
                return (int)(Mathf.Pow(level, 3) * (100 - level) / 50f);
            }
            else if (level < 35)
            {
                return (int)(Mathf.Pow(level, 3) * (150 - level) / 100f);
            }
            else if (level < 50)
            {
                float inner = Mathf.Floor((1911 - 10 * level) / 3f);
                return (int)(Mathf.Pow(level, 3) * inner / 500f);
            }
            else
            {
                return (int)(Mathf.Pow(level, 3) * (160 - level) / 100f);
            }
        }

        return -1;
    }
    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public PockiType Type1
    {
        get { return type1; }
    }

    public PockiType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public List<MoveBase> LearnableByItems => learnableByItems;
    public List<Evolution> Evolutions => evolutions;
    public int CatchRate => catchRate;
    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}
[System.Serializable]

public class Evolution
{
    [SerializeField] PockiBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;
    [SerializeField] EvolutionType evolutionType;
    [SerializeField] PockiType dominantTypeForEvolution;

    public PockiBase EvolvesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
    public ItemBase RequiredItem => requiredItem;
    public EvolutionType EvolutionType => evolutionType;
    public PockiType DominantTypeForEvolution => dominantTypeForEvolution;

}
public enum EvolutionType
{
    Level,
    Item,
    LevelAndItem,
    BasedOnMoveType
}

public enum PockiType
{
    None,
    Strength,
    Vitality,
    Agility
}
public enum GrowthRate
{
    Fast, MediumFast, MediumSlow, Slow, Fluctuating, Erratic
}

public class TypeChart
{
    static float[][] chart =
    {
         //                         Str   Vit   Agi
        /*Stregth*/     new float[] {1f  ,   2f,   0.5f},
        /*Vitality*/    new float[] {0.5f,   1f,     2f},
        /*Agility*/     new float[] {2f  , 0.5f,     1f}
    };

    public static float GetEffectiveness(PockiType attactType, PockiType defenseType)
    {
        if (attactType == PockiType.None || defenseType == PockiType.None)
            return 1;

        int row = (int)attactType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    Accuracy,
    Evasion
}