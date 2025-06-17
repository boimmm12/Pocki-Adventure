using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;

    [SerializeField] Dialog startDialogue;
    [SerializeField] Dialog inProgressDialogue;
    [SerializeField] Dialog completedDialogue;

    [Header("QUEST ITEM")]
    [SerializeField] ItemBase startItem;
    [SerializeField] int startItemCount = 1;

    [Header("ITEM REQUIRED")]
    [SerializeField] List<ItemBase> requiredItems;
    [SerializeField] List<int> requiredItemCounts;

    [Header("REWARD")]
    [SerializeField] List<ItemBase> rewardItems;
    [SerializeField] List<int> rewardItemCounts;
    [SerializeField] int rewardMoney;
    [SerializeField] List<PockiBase> rewardPokemonChoices;
    [SerializeField] int rewardPokemonLevel = 5;


    private void OnValidate()
    {
        // Sync required items
        while (requiredItemCounts.Count < requiredItems.Count)
            requiredItemCounts.Add(1);
        while (requiredItemCounts.Count > requiredItems.Count)
            requiredItemCounts.RemoveAt(requiredItemCounts.Count - 1);

        // Sync reward items
        while (rewardItemCounts.Count < rewardItems.Count)
            rewardItemCounts.Add(1);
        while (rewardItemCounts.Count > rewardItems.Count)
            rewardItemCounts.RemoveAt(rewardItemCounts.Count - 1);
    }


    public string Name => name;
    public string Description => description;

    public Dialog StartDialogue => startDialogue;
    public Dialog InProgressDialogue => inProgressDialogue?.Lines?.Count > 0 ? inProgressDialogue : startDialogue;
    public Dialog CompletedDialogue => completedDialogue;

    public ItemBase StartItem => startItem;
    public int StartItemCount => startItemCount;

    public List<ItemBase> RequiredItems => requiredItems;
    public List<int> RequiredItemCounts => requiredItemCounts;

    public List<ItemBase> RewardItems => rewardItems;
    public List<int> RewardItemCounts => rewardItemCounts;
    public int RewardMoney => rewardMoney;
    public List<PockiBase> RewardPokemonChoices => rewardPokemonChoices;
    public int RewardPokemonLevel => rewardPokemonLevel;
}
