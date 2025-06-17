using UnityEngine;

[CreateAssetMenu(fileName = "XpScroll", menuName = "Items/Xp Scroll")]
public class XpScrollItem : ItemBase
{
    [SerializeField] int expAmount;

    public override bool Use(Pocki pokemon)
    {
        if (pokemon == null) return false;

        pokemon.AddExp(expAmount);

        Debug.Log($"{pokemon.Base.Name} gained {expAmount} EXP from XP Scroll.");
        return true;
    }
    public override bool CanUseInBattle => false;
    public int ExpAmount => expAmount; // Hanya di luar battle
}
