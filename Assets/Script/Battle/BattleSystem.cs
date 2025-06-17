using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using GDEUtils.StateMachine;

public enum BattleAction { Move, SwitchPokemon, UseItem, Run }
public enum BattleTrigger { LongGrass, Water, Sand, Snow, Trainer }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] public BattleUnit playerUnit;
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprites;
    [SerializeField] public Button yesButton;
    [SerializeField] public Button noButton;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Background Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite grassBackground;
    [SerializeField] Sprite waterBackground;
    [SerializeField] Sprite sandBackground;
    [SerializeField] Sprite snowBackground;
    [SerializeField] Sprite trainerBackground;

    public StateMachine<BattleSystem> StateMachine { get; private set; }

    public event Action<bool> OnBattleOver;
    public int SelectedMove { get; set; }
    public BattleAction SelectedAction { get; set; }
    public Pocki SelectedPokemon { get; set; }
    public ItemBase SelectedItem { get; set; }
    public bool IsBattleOver { get; private set; }
    public PockiParty PlayerParty { get; private set; }
    public PockiParty TrainerParty { get; private set; }
    public Pocki WildPokemon { get; private set; }

    public bool IsTrainerBattle { get; private set; } = false;
    PlayerController player;
    public TrainerController Trainer { get; private set; }

    public int EscapeAttempts { get; set; }

    BattleTrigger battleTrigger;

    public void StartBattle(PockiParty playerParty, Pocki wildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.PlayerParty = playerParty;
        this.WildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        IsTrainerBattle = false;

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PockiParty playerParty, PockiParty trainerParty, BattleTrigger trigger = BattleTrigger.Trainer)
    {
        this.PlayerParty = playerParty;
        this.TrainerParty = trainerParty;

        IsTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        Trainer = trainerParty.GetComponent<TrainerController>();

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);

        playerUnit.Clear();
        enemyUnit.Clear();

        switch (battleTrigger)
        {
            case BattleTrigger.LongGrass:
                backgroundImage.sprite = grassBackground;
                break;
            case BattleTrigger.Water:
                backgroundImage.sprite = waterBackground;
                break;
            case BattleTrigger.Sand:
                backgroundImage.sprite = sandBackground;
                break;
            case BattleTrigger.Snow:
                backgroundImage.sprite = snowBackground;
                break;
            case BattleTrigger.Trainer:
                backgroundImage.sprite = trainerBackground;
                break;
            default:
                backgroundImage.sprite = grassBackground;
                break;
        }

        if (!IsTrainerBattle)
        {
            var healthyPlayerPokemon = PlayerParty.GetHealthyPocki();
            if (healthyPlayerPokemon == null)
            {
                yield return dialogBox.TypeDialog("You don't have any healthy pocki!");
                BattleOver(false);
                yield break;
            }

            playerUnit.Setup(healthyPlayerPokemon);
            enemyUnit.Setup(WildPokemon);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");
        }

        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            var preset = GlobalCharacterPresets.i.GetPreset(PlayerController.i.PresetIndex);
            playerImage.sprite = preset.battleSprite;
            trainerImage.sprite = Trainer.Sprite;

            yield return dialogBox.TypeDialog($"{Trainer.Name} wants to battle");

            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = TrainerParty.GetHealthyPocki();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{Trainer.Name} sends out {enemyPokemon.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = PlayerParty.GetHealthyPocki();
            if (playerPokemon == null)
            {
                yield return dialogBox.TypeDialog("You don't have any healthy pocki!");
                BattleOver(false);
                yield break;
            }
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
        }

        IsBattleOver = false;
        EscapeAttempts = 0;
        partyScreen.Init();
        StateMachine.ChangeState(ActionSelectionState.i);
    }

    public void BattleOver(bool won)
    {
        IsBattleOver = true;
        PlayerParty.Pockies.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }
    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public IEnumerator SwitchPokemon(Pocki newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        dialogBox.ClearMoveDetails();
    }

    public IEnumerator SendNextTrainerPokemon()
    {
        var nextPokemon = TrainerParty.GetHealthyPocki();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{Trainer.Name} send out {nextPokemon.Base.Name}!");
    }
    public IEnumerator ThrowCaptureball(CaptureBall captureBall)
    {

        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainers pokemon!");
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used {captureBall.Name.ToUpper()}!");

        var pokeballObj = Instantiate(pokeballSprites, playerUnit.transform.position, Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = captureBall.Icon;

        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2.5f, 1, 0.8f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon, captureBall);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            PlayerParty.AddPocki(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free");
            else
                yield return dialogBox.TypeDialog($"yahaha gadapet");

            Destroy(pokeball);
        }
    }

    int TryToCatchPokemon(Pocki pokemon, CaptureBall captureBall)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * captureBall.CatchRateModifier * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);//bulbapedia.bulbagarden.net

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711670 / a));//bulbapedia.bulbagarden.net

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }
    public BattleDialogBox DialogBox => dialogBox;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;
    public PartyScreen PartyScreen => partyScreen;
    public AudioClip BattleVictoryMusic => battleVictoryMusic;
}