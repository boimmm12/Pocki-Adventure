using GDEUtils.StateMachine;
using UnityEngine;
using UnityEngine.UI;


public class BattleSystemState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] GameObject Ui;
    //input
    public BattleTrigger trigger { get; set; }
    public TrainerController trainer { get; set; }

    public Pocki overrideWildPokemon { get; set; }
    public static BattleSystemState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;

        battleSystem.gameObject.SetActive(true);
        gc.WorldCamera.gameObject.SetActive(false);
        Ui.gameObject.SetActive(false);

        var playerParty = gc.PlayerController.GetComponent<PockiParty>();
        if (trainer == null)
        {
            Pocki wildPokemon;

            if (overrideWildPokemon != null)
            {
                wildPokemon = overrideWildPokemon;
                overrideWildPokemon = null; // Reset agar tidak terbawa
            }
            else
            {
                var wild = gc.CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(trigger);
                wildPokemon = new Pocki(wild.Base, wild.Level);
            }

            battleSystem.StartBattle(playerParty, wildPokemon, trigger);
        }
        else
        {
            var trainerParty = trainer.GetComponent<PockiParty>();
            battleSystem.StartTrainerBattle(playerParty, trainerParty);
        }
        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Exit()
    {
        battleSystem.gameObject.SetActive(false);
        gc.WorldCamera.gameObject.SetActive(true);
        Ui.gameObject.SetActive(true);

        battleSystem.OnBattleOver -= EndBattle;
    }

    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            StartCoroutine(trainer.HandleBattleEnd());
            trainer = null;
        }

        gc.StateMachine.Pop();
    }

    public BattleSystem BattleSystem => battleSystem;
}
