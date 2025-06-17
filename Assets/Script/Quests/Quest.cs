using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }
    PockiParty party;

    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectByName(saveData.name);
        Status = saveData.status;
    }

    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = Base.name,
            status = Status
        };
        return saveData;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        if (Base.StartItem != null)
        {
            Inventory.GetInventory().AddItem(Base.StartItem, Base.StartItemCount);
            string playerName = GameController.Instance.PlayerController.Name;
            yield return DialogManager.Instance.ShowDialogText($"{playerName} received {Base.StartItem.Name}");
        }

        yield return DialogManager.Instance.ShowDialog(Base.StartDialogue);

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public IEnumerator CompleteQuest(Transform player)
    {
        if (Status == QuestStatus.None)
        {
            // Tambahkan ke daftar quest walau tidak pernah di-start
            QuestList.GetQuestList().AddQuest(this);
        }

        Status = QuestStatus.Completed;
        yield return DialogManager.Instance.ShowDialog(Base.CompletedDialogue);

        var inventory = Inventory.GetInventory();

        // Hapus item yang diminta
        for (int i = 0; i < Base.RequiredItems.Count; i++)
        {
            inventory.RemoveItem(Base.RequiredItems[i], Base.RequiredItemCounts[i]);
        }

        // Berikan item hadiah
        for (int i = 0; i < Base.RewardItems.Count; i++)
        {
            inventory.AddItem(Base.RewardItems[i], Base.RewardItemCounts[i]);
            yield return DialogManager.Instance.ShowDialogText($"{player.GetComponent<PlayerController>().Name} received {Base.RewardItemCounts[i]}x {Base.RewardItems[i].Name}!");
        }

        // Berikan uang jika ada
        if (Base.RewardMoney > 0)
        {
            Wallet.i.AddMoney(Base.RewardMoney);
            yield return DialogManager.Instance.ShowDialogText($"{player.GetComponent<PlayerController>().Name} received 🪙{Base.RewardMoney}!");
        }

        // Hadiah berupa pilihan Pokémon
        if (Base.RewardPokemonChoices != null && Base.RewardPokemonChoices.Count > 0)
        {
            int selectedChoice = 0;

            yield return DialogManager.Instance.ShowDialogText("Choose a Pocki:",
                choices: Base.RewardPokemonChoices.Select(p => p.Name).ToList(),
                onChoiceSelected: (choice) => selectedChoice = choice);

            var selectedBase = Base.RewardPokemonChoices[selectedChoice];
            var newPokemon = new Pocki(selectedBase, Base.RewardPokemonLevel);

            var party = player.GetComponent<PlayerController>().Party;
            party.AddPocki(newPokemon);

            yield return DialogManager.Instance.ShowDialogText($"{player.GetComponent<PlayerController>().Name} received {selectedBase.Name}!");
        }


        // Tambahkan ke daftar quest (jika belum)
        QuestList.GetQuestList().AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();

        for (int i = 0; i < Base.RequiredItems.Count; i++)
        {
            var item = Base.RequiredItems[i];
            var requiredCount = Base.RequiredItemCounts[i];

            if (inventory.GetItemCount(item) < requiredCount)
                return false;
        }

        // Quest bisa diselesaikan meskipun statusnya None, selama item lengkap
        return Status == QuestStatus.Started || Status == QuestStatus.None;
    }


}

[System.Serializable]
public class QuestSaveData
{
    public string name;
    public QuestStatus status;
}

public enum QuestStatus { None, Started, Completed }
