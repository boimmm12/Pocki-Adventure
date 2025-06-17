using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Recovery Item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] public int hpAmount;
    [SerializeField] public bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] public int ppAmount;
    [SerializeField] public bool restoreMaxPP;

    [Header("Status Condition")]
    [SerializeField] public ConditionID status;
    [SerializeField] public bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] public bool revive;
    [SerializeField] public bool maxRevive;

    public override bool Use(Pocki pokemon)
    {
        //revive
        if(revive || maxRevive)
        {
            if(pokemon.HP > 0)
                return false;
            
            if(revive)
                pokemon.IncreaseHP(pokemon.MaxHp / 2);
            else if(maxRevive)
                pokemon.IncreaseHP(pokemon.MaxHp);
            
            pokemon.CureStatus();

            return true;
        }

        if(pokemon.HP == 0)
            return false;

        //heal
        if (restoreMaxHP || hpAmount > 0)
        {
            if(pokemon.HP == pokemon.MaxHp)
                return false;
            
            if(restoreMaxHP)
                pokemon.IncreaseHP(pokemon.MaxHp);
            else
                pokemon.IncreaseHP(hpAmount);
        }

        //status
        if(recoverAllStatus || status != ConditionID.none)
        {
            if(pokemon.Status == null && pokemon.VolatileStatus == null)
                return false;

            if(recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            }
            else
            {
                if(pokemon.Status.Id == status)
                    pokemon.CureStatus();
                else if(pokemon.VolatileStatus.Id == status)
                    pokemon.CureVolatileStatus();
                else
                    return false;
            }
        }

        //restore PP
        if(restoreMaxPP)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if(ppAmount > 0)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }
        return true;
    }
}
