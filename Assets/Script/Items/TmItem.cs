using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";
    public override bool Use(Pocki pokemon)
    {
        if (pokemon.HasMove(move)) return false;
        if (!CanBeTaught(pokemon)) return false;

        if (pokemon.Moves.Count < PockiBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(move);
            return true;
        }

        return false;
    }

    public bool CanBeTaught(Pocki pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(move);
    }

    public override bool IsReusable => isHM;
    public override bool CanUseInBattle => false;
    public MoveBase Move => move;
    public bool IsHM => isHM;

}
