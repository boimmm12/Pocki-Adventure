using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

public class AboutToUseState : State<BattleSystem>
{
    public Pocki NewPokemon { get; set; }
    public static AboutToUseState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;

    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        StartCoroutine(StartState());
    }

    IEnumerator StartState()
    {
        yield return bs.DialogBox.TypeDialog($"{bs.Trainer.Name} is about to use {NewPokemon.Base.Name}. Do you want to change pokemon?");
        bs.DialogBox.EnableChoiceBox(true);
    }

    public override void Execute()
    {
        if (!bs.DialogBox.IsChoiceBoxEnabled)
            return;
        // Prevent stacking by clearing existing listeners first
        bs.yesButton.onClick.RemoveAllListeners();
        bs.noButton.onClick.RemoveAllListeners();

        // yes option
        bs.yesButton.onClick.AddListener(() =>
        {
            bs.DialogBox.EnableChoiceBox(false);
            StartCoroutine(SwitchAndContinueBattle());
        });

        // no option
        bs.noButton.onClick.AddListener(() =>
        {
            bs.DialogBox.EnableChoiceBox(false);
            StartCoroutine(ContinueBattle());
        });
    }

    IEnumerator SwitchAndContinueBattle()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;
        if (selectedPokemon != null)
        {
            yield return bs.SwitchPokemon(selectedPokemon);
        }

        yield return ContinueBattle();
    }
    IEnumerator ContinueBattle()
    {
        yield return bs.SendNextTrainerPokemon();
        bs.StateMachine.Pop();
    }
}
