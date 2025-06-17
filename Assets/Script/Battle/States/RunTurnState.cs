using GDEUtils.StateMachine;
using UnityEngine;
using System.Linq;
using System.Collections;
public class RunTurnState : State<BattleSystem>
{
    public static RunTurnState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    BattleUnit playerUnit;
    BattleUnit enemyUnit;
    BattleDialogBox dialogBox;
    PartyScreen partyScreen;
    bool isTrainerBattle;
    PockiParty playerParty;
    PockiParty trainerParty;

    BattleSystem bs;

    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        playerUnit = bs.PlayerUnit;
        enemyUnit = bs.EnemyUnit;
        dialogBox = bs.DialogBox;
        partyScreen = bs.PartyScreen;
        isTrainerBattle = bs.IsTrainerBattle;
        playerParty = bs.PlayerParty;
        trainerParty = bs.TrainerParty;

        StartCoroutine(RunTurns(bs.SelectedAction));
    }
    IEnumerator RunTurns(BattleAction playerAction)
    {

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[bs.SelectedMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
            var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (bs.IsBattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (bs.IsBattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                yield return bs.SwitchPokemon(bs.SelectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                if (bs.SelectedItem is CaptureBall)
                {
                    yield return bs.ThrowCaptureball(bs.SelectedItem as CaptureBall);
                    if (bs.IsBattleOver) yield break;
                }
                else
                {

                }
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (bs.IsBattleOver) yield break;
        }
        if (!bs.IsBattleOver)
            bs.StateMachine.ChangeState(ActionSelectionState.i);
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");


        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            DamageDetails damageDetails = null;

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Effects != null && move.Base.Effects.DrainHP && damageDetails != null)
            {
                int healAmount = Mathf.FloorToInt(damageDetails.DamageDealt * move.Base.Effects.DrainPercentage);
                if (healAmount > 0)
                {
                    sourceUnit.Pokemon.IncreaseHP(healAmount);
                    yield return sourceUnit.Hud.WaitForHPUpdate();
                    yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} absorbed health!");
                }
            }



            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffect effects, Pocki source, Pocki target, MoveTarget moveTarget)
    {

        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (bs.IsBattleOver) yield break;

        // Statuses like burn or psn will hurt the pokemon after the turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
        }
    }
    bool CheckIfMoveHits(Move move, Pocki source, Pocki target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValue = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValue[accuracy];
        else
            moveAccuracy /= boostValue[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValue[evasion];
        else
            moveAccuracy *= boostValue[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pocki pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} fainted!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
                battleWon = trainerParty.GetHealthyPocki() == null;

            if (battleWon)
                AudioManager.i.PlayMusic(bs.BattleVictoryMusic);

            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = isTrainerBattle ? 1.5f : 1f;

            int totalExp = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus) / 7;
            var playerParty = PlayerController.i.GetComponent<PockiParty>().Pockies;

            foreach (var pkmn in playerParty)
            {
                if (pkmn.HP <= 0) continue;

                int expGain = (pkmn == playerUnit.Pokemon) ? totalExp : Mathf.FloorToInt(totalExp * 0.4f);
                pkmn.Exp += expGain;

                if (pkmn == playerUnit.Pokemon)
                {
                    yield return dialogBox.TypeDialog($"{pkmn.Base.Name} gained {expGain} exp");
                    yield return playerUnit.Hud.SetExpSmooth();
                }

                int oldLevel = pkmn.Level;
                float speedMultiplier = (pkmn == playerUnit.Pokemon) ? 3f : 1f;

                while (pkmn.CheckForLevelUp())
                {
                    if (pkmn == playerUnit.Pokemon)
                    {
                        playerUnit.Hud.SetLevel();
                        yield return dialogBox.TypeDialog($"{pkmn.Base.Name} leveled up!");
                        yield return playerUnit.Hud.SetExpSmooth(true, speedMultiplier);
                    }
                }

                if (pkmn.Level > oldLevel && pkmn == playerUnit.Pokemon)
                {
                    yield return dialogBox.TypeDialog($"{pkmn.Base.Name} now Lvl.{pkmn.Level}!");
                }

                if (pkmn == playerUnit.Pokemon)
                {
                    var newMove = pkmn.GetLearnableMoveAtCurrLevel();

                    if (newMove != null)
                    {
                        if (pkmn.Moves.Count < PockiBase.MaxNumOfMoves)
                        {
                            pkmn.LearnMove(newMove.Base);
                            yield return dialogBox.TypeDialog($"Your {pkmn.Base.Name} learned {newMove.Base.Name}");
                            dialogBox.SetMoveNames(pkmn.Moves);
                        }
                        else
                        {
                            yield return dialogBox.TypeDialog($"{pkmn.Base.Name} is trying to learn {newMove.Base.Name}");
                            yield return dialogBox.TypeDialog($"But it can't learn more than {PockiBase.MaxNumOfMoves} moves");
                            yield return dialogBox.TypeDialog("Choose a move to forget");

                            MoveToForgetState.i.CurrentMoves = pkmn.Moves.Select(x => x.Base).ToList();
                            MoveToForgetState.i.NewMove = newMove.Base;
                            yield return GameController.Instance.StateMachine.PushAndWait(MoveToForgetState.i);

                            var moveIndex = MoveToForgetState.i.Selection;
                            if (moveIndex == PockiBase.MaxNumOfMoves || moveIndex == -1)
                            {
                                yield return dialogBox.TypeDialog($"{pkmn.Base.Name} did not learn {newMove.Base.Name}");
                                pkmn.DeclineMove(newMove.Base);
                            }
                            else
                            {
                                var selectedMove = pkmn.Moves[moveIndex].Base;
                                yield return dialogBox.TypeDialog($"{pkmn.Base.Name} forgot {selectedMove.Name} and learned {newMove.Base.Name}");
                                pkmn.Moves[moveIndex] = new Move(newMove.Base);
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(1f);
        }
        yield return CheckForBattleOver(faintedUnit);
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPocki();
            if (nextPokemon != null)
            {
                yield return GameController.Instance.StateMachine.PushAndWait(PartyState.i);
                yield return bs.SwitchPokemon(PartyState.i.SelectedPokemon);
            }
            else
            {
                bs.BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                bs.BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPocki();
                if (nextPokemon != null)
                {
                    AboutToUseState.i.NewPokemon = nextPokemon;
                    yield return bs.StateMachine.PushAndWait(AboutToUseState.i);
                }
                else
                {
                    bs.BattleOver(true);
                }
            }
        }
    }
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");
        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's Super Effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's Not Very Effective!");
    }

    IEnumerator TryToEscape()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You cant run!");
            yield break;
        }

        ++bs.EscapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            dialogBox.EnableActionSelector(false);
            yield return dialogBox.TypeDialog($"Nigerondayoo Smokeey!");

            bs.BattleOver(true);
        }
        else
        {
            float f = playerSpeed * 128 / enemySpeed + 30 * bs.EscapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                dialogBox.EnableActionSelector(false);
                yield return dialogBox.TypeDialog($"Ran away safely!");

                bs.BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
            }
        }
    }
}
