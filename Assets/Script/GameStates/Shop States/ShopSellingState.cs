    using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSellingState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    public static ShopSellingState i { get; private set; }

    private void Awake()
    {
        i = this;
    }
    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }
    public List<ItemBase> AvailableItems { get; set; }
    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;

        StartCoroutine(StartSellingState());
    }

    IEnumerator StartSellingState()
    {
        yield return gc.StateMachine.PushAndWait(InventoryState.i);

        var selectedItem = InventoryState.i.SelectedItem;
        if (selectedItem != null)
        {
            yield return SellItem(selectedItem);
            StartCoroutine(StartSellingState());
        }
        else
        gc.StateMachine.Pop();
    }

    IEnumerator SellItem(ItemBase item)
    {

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("You can't sell this!");
            yield break;
        }

        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price * 70 / 100); //70%
        int countToSell = 1;

        int itemCount = inventory.GetItemCount(item);
        if (itemCount > 1)
        {
            yield return DialogManager.Instance.ShowDialogText($"Nak jual berapa banyak icik bos?",
                waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                (selectedCount) => countToSell = selectedCount);

            DialogManager.Instance.CloseDialog();
        }

        sellingPrice = sellingPrice * countToSell;

        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialogText($"Dapat {sellingPrice} ni icik bos, nak jual kah?",
        waitForInput: false,
        choices: new List<string>() { "Yes", "No" },
        onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Sell
            inventory.RemoveItem(item, countToSell);
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"Nih icik bos, ©{sellingPrice}");
        }

        walletUI.Close();
    }
}
