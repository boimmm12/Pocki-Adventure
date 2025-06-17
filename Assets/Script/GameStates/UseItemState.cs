using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    public bool ItemUsed { get; private set; }

    // Output

    public static UseItemState i { get; private set; }
    Inventory inventory;
    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        inventory = Inventory.GetInventory();
        ItemUsed = false;
        partyScreen.CanInteract = false;
        StartCoroutine(UseItem());
    }

    IEnumerator UseItem()
    {
        var item = inventoryUI.SelectedItem;
        var pokemon = partyScreen.SelectedMember;

        if (item == null)
        {
            Debug.LogError("[UseItemState] Selected item is null!");
            yield break;
        }

        if (pokemon == null)
        {
            Debug.LogError("[UseItemState] Selected Pokémon is null!");
            yield break;
        }

        if (item is TmItem)
        {
            partyScreen.ShowIfTmIsUsable(item as TmItem);
            yield return HandleTmItems();
        }
        else
        {
            //Handle Evolution Items
            if (item is EvolutionItem)
            {
                var evolution = pokemon.CheckForEvolution(pokemon.Level, item);

                if (evolution != null)
                {
                    yield return EvolutionState.i.Evolve(pokemon, evolution);
                }
                else
                {
                    var evoByItemOnly = pokemon.Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);

                    if (evoByItemOnly != null && pokemon.Level < evoByItemOnly.RequiredLevel)
                    {
                        yield return DialogManager.Instance.ShowDialogText($"Level is too low to use {item.Name}!");
                    }
                    else
                    {
                        yield return DialogManager.Instance.ShowDialogText($"It won't have any effect!");
                    }

                    gc.StateMachine.Pop();
                    partyScreen.CanInteract = true;
                    yield break;
                }
            }

            var usedItem = inventory.UseItem(item, partyScreen.SelectedMember);
            if (usedItem != null)
            {
                ItemUsed = true;

                if (usedItem is RecoveryItem)
                    yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");
                else if (usedItem is XpScrollItem)
                {
                    int levelAfter = pokemon.Level;
                    yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is now Level {levelAfter}!");

                    partyScreen.CanInteract = true;
                    partyScreen.SetPartyData();

                    var newMove = pokemon.GetLearnableMoveAtCurrLevel();

                    if (newMove != null)
                    {
                        if (pokemon.Moves.Count < PockiBase.MaxNumOfMoves)
                        {
                            pokemon.LearnMove(newMove.Base);
                            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} learned {newMove.Base.Name}!");
                        }
                        else
                        {
                            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is trying to learn {newMove.Base.Name}");
                            yield return DialogManager.Instance.ShowDialogText($"But it can't learn more than {PockiBase.MaxNumOfMoves} moves.");
                            yield return DialogManager.Instance.ShowDialogText($"Choose a move to forget:");

                            MoveToForgetState.i.CurrentMoves = pokemon.Moves.Select(x => x.Base).ToList();
                            MoveToForgetState.i.NewMove = newMove.Base;
                            yield return GameController.Instance.StateMachine.PushAndWait(MoveToForgetState.i);

                            var moveIndex = MoveToForgetState.i.Selection;

                            if (moveIndex == PockiBase.MaxNumOfMoves || moveIndex == -1)
                            {
                                yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} did not learn {newMove.Base.Name}");
                                pokemon.DeclineMove(newMove.Base);
                            }
                            else
                            {
                                var selectedMove = pokemon.Moves[moveIndex].Base;
                                yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} forgot {selectedMove.Name} and learned {newMove.Base.Name}");
                                pokemon.Moves[moveIndex] = new Move(newMove.Base);
                            }
                        }
                        partyScreen.CanInteract = true;
                        partyScreen.SetPartyData();
                    }
                }

            }
            else
            {
                if (inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                    yield return DialogManager.Instance.ShowDialogText($"It won't have any affect!");
            }
        }
        partyScreen.CanInteract = true;
        gc.StateMachine.Pop();
    }

    IEnumerator HandleTmItems()
    {
        var tmItem = inventoryUI.SelectedItem as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} already know {tmItem.Move.Name}");
            yield break;
        }

        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} can't learn {tmItem.Move.Name}");
            yield break;
        }

        if (pokemon.Moves.Count < PockiBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} learned {tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is trying to learn {tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {PockiBase.MaxNumOfMoves} moves");

            yield return DialogManager.Instance.ShowDialogText($"Choose a move you want to forget", true, false);

            MoveToForgetState.i.CurrentMoves = pokemon.Moves.Select(x => x.Base).ToList();
            MoveToForgetState.i.NewMove = tmItem.Move;
            yield return gc.StateMachine.PushAndWait(MoveToForgetState.i);

            var moveIndex = MoveToForgetState.i.Selection;
            if (moveIndex == PockiBase.MaxNumOfMoves || moveIndex == -1)
            {
                // Don't learn the new move
                yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} did not learn {tmItem.Move.Name}");
            }
            else
            {
                // Forget the selected move and learn new move
                var selectedMove = pokemon.Moves[moveIndex].Base;
                yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} forgot {selectedMove.Name} and learned {tmItem.Move.Name}");

                pokemon.Moves[moveIndex] = new Move(tmItem.Move);
            }
        }
        if (!tmItem.IsReusable)
        {
            inventory.RemoveItem(tmItem);
            ItemUsed = true;
        }
    }
}
