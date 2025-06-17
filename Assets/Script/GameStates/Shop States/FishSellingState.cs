using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSellingState : State<GameController>
{
    [SerializeField] FishStorageUI fishStorageUI;
    [SerializeField] WalletUI walletUI;

    public static FishSellingState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;
        StartCoroutine(SellAllFish());
    }

    IEnumerator SellAllFish()
    {
        var fishList = FishInventory.i.GetFishList();

        if (fishList.Count == 0)
        {
            yield return DialogManager.Instance.ShowDialogText("You have no fish to sell.");
            gc.StateMachine.Pop();
            yield break;
        }

        int totalPrice = 0;
        foreach (var fish in fishList)
        {
            totalPrice += fish.GetPrice();
        }

        int choice = 0;
        yield return DialogManager.Instance.ShowDialogText(
            $"Sell all fish for {totalPrice}₵?",
            waitForInput: false,
            choices: new List<string>() { "Yes", "No" },
            onChoiceSelected: (i) => choice = i
        );

        if (choice == 0)
        {
            Wallet.i.AddMoney(totalPrice);
            FishInventory.i.ClearFish();
            yield return DialogManager.Instance.ShowDialogText($"All fish sold! You earned {totalPrice}₵.");
            totalPrice = 0;
        }

        gc.StateMachine.Pop();
    }
}
