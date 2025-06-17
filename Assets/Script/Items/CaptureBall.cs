using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Capture Ball")]
public class CaptureBall : ItemBase
{
    [SerializeField] public float catchRateModifier = 1;
    public override bool Use(Pocki pokemon)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;
    public float CatchRateModifier => catchRateModifier;
}
