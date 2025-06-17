using System.Collections;
using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    public Pocki SelectedPokemon { get; private set; }
    bool isSwitchingPosition;
    int selectedIndexForSwitching = 0;
    public static PartyState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    PockiParty playerParty;

    private void Start()
    {
        playerParty = PlayerController.i.GetComponent<PockiParty>();
    }
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        SelectedPokemon = null;
        partyScreen.gameObject.SetActive(true);
        partyScreen.OnSelected += OnPokemonSelected;
        partyScreen.OnBack += OnBack;

        if (gc.StateMachine.GetPrevState() == InventoryState.i)
        {
            var item = inventoryUI.SelectedItem;
            if (item is TmItem tmItem)
            {
                partyScreen.ShowIfTmIsUsable(tmItem);
            }
        }
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        partyScreen.ResetTmUsableMessages();
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnPokemonSelected;
        partyScreen.OnBack -= OnBack;
    }

    void OnPokemonSelected(int selection)
    {
        SelectedPokemon = partyScreen.SelectedMember;

        StartCoroutine(PokemonSelectedAction(selection));
    }

    IEnumerator PokemonSelectedAction(int selectedPokemonIndex)
    {
        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == InventoryState.i)
        {
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleSystemState.i)
        {
            var battleState = prevState as BattleSystemState;

            DynamicMenuState.i.MenuItems = new List<string>() { "Shift", "Summary", "Cancel" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if (DynamicMenuState.i.SelectedItem == 0)
            {
                if (SelectedPokemon.HP <= 0)
                {
                    partyScreen.SetMessageText("You cant send out a fainted pocki");
                    yield break;
                }
                if (SelectedPokemon == battleState.BattleSystem.PlayerUnit.Pokemon)
                {
                    partyScreen.SetMessageText("You cant switch with the same pocki");
                    yield break;
                }

                gc.StateMachine.Pop();
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                SummaryState.i.SelectedPokemonIndex = selectedPokemonIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i);
            }
            else
            {
                yield break;
            }
        }
        else
        {
            if (isSwitchingPosition)
            {
                if (selectedIndexForSwitching == selectedPokemonIndex)
                {
                    partyScreen.SetMessageText("You cant switch with the same pocki");
                    yield break;
                }
                isSwitchingPosition = false;

                var tmpPokemon = playerParty.Pockies[selectedIndexForSwitching];
                playerParty.Pockies[selectedIndexForSwitching] = playerParty.Pockies[selectedPokemonIndex];
                playerParty.Pockies[selectedPokemonIndex] = tmpPokemon;
                playerParty.PartyUpdated();

                yield break;
            }
            DynamicMenuState.i.MenuItems = new List<string>() { "Summary", "Switch Position", "Cancel" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if (DynamicMenuState.i.SelectedItem == 0)
            {
                SummaryState.i.SelectedPokemonIndex = selectedPokemonIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i);
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                //TODO : Switch Position
                isSwitchingPosition = true;
                selectedIndexForSwitching = selectedPokemonIndex;
                partyScreen.SetMessageText("Choose position to switch");
            }
            else
            {
                partyScreen.ResetSelection();
                yield break;
            }
        }
    }

    IEnumerator GoToUseItemState()
    {
        yield return gc.StateMachine.PushAndWait(UseItemState.i);
        gc.StateMachine.Pop();
    }
    void OnBack()
    {
        SelectedPokemon = null;

        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == BattleSystemState.i)
        {
            var battleState = prevState as BattleSystemState;

            if (battleState.BattleSystem.PlayerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a pocki to continue");
                return;
            }
        }
        gc.StateMachine.Pop();
    }
    public void ResetSelectionInParty()
    {
        partyScreen.ResetSelection();
    }

}
