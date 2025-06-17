using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;

    [Header("Quest")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    [Header("Movement")]
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;

    Character character;
    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;
    Healer healer;
    Merchant merchant;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (state != NPCState.Idle)
            yield break;

        state = NPCState.Dialog;
        character.LookToward(initiator.position);

        var questList = QuestList.GetQuestList();

        // Cek penyelesaian quest dulu
        if (questToComplete != null)
        {
            var quest = questList.GetQuest(questToComplete);

            // Jika quest sudah ada dan bisa diselesaikan
            if (quest != null && quest.CanBeCompleted())
            {
                yield return quest.CompleteQuest(initiator);
                Debug.Log($"{quest.Base.Name} completed!");
                state = NPCState.Idle;
                yield break;
            }

            // Jika quest belum pernah dimulai tapi item sudah cukup → langsung selesaikan
            if (quest == null)
            {
                var tempQuest = new Quest(questToComplete);
                if (tempQuest.CanBeCompleted())
                {
                    yield return tempQuest.CompleteQuest(initiator);
                    Debug.Log($"{tempQuest.Base.Name} completed directly!");
                    state = NPCState.Idle;
                    yield break;
                }
            }
        }

        // Cek pemberian item atau pokemon
        if (itemGiver != null && itemGiver.CanBeGiven())
        {
            yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
        }
        else if (pokemonGiver != null && pokemonGiver.CanBeGiven())
        {
            yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
        }

        // Cek apakah quest baru bisa dimulai
        else if (questToStart != null)
        {
            if (!questList.IsStarted(questToStart.Name))
            {
                var newQuest = new Quest(questToStart);
                yield return newQuest.StartQuest();
            }
            else
            {
                // Hanya tampilkan dialog in progress, jangan pernah menyelesaikan quest dari sini
                yield return DialogManager.Instance.ShowDialog(questToStart.InProgressDialogue);
            }
        }

        // Cek quest yang sedang aktif
        else if (activeQuest != null)
        {
            if (activeQuest.CanBeCompleted())
            {
                // Hanya selesaikan jika NPC juga adalah questToComplete
                if (questToComplete == activeQuest.Base)
                    yield return activeQuest.CompleteQuest(initiator);
                else
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
            }
        }

        // Healer dan lainnya
        else if (healer != null)
        {
            yield return healer.Heal(initiator, dialog);
        }
        else if (merchant != null)
        {
            yield return merchant.Trade();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
        }

        state = NPCState.Idle;
        idleTimer = 0f;
    }


    private void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;


        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if (questToStart != null)
            saveData.questToStart = new Quest(questToStart).GetSaveData();

        if (questToComplete != null)
            saveData.questToComplete = new Quest(questToComplete).GetSaveData();

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;

        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}
[System.Serializable]

public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Walking, Dialog }