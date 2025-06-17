using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Evolution Item")]
public class EvolutionItem : ItemBase
{
    public override bool Use(Pocki pokemon)
    {
        return true;
    }
}
