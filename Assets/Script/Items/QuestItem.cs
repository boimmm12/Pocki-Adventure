using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Quest Item")]
public class QuestItem : ItemBase
{
    public override bool Use(Pocki pokemon)
    {
        return true;
    }
}
