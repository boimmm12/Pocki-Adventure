using GDEUtils.StateMachine;
using UnityEngine;

public class StorageState : State<GameController>
{
    [SerializeField] PockiStorageUI storageUI;
    bool isMovingPokemon = false;
    int selectedSlotToMove = 0;
    Pocki selectedPokemonToMove = null;

    PockiParty party;
    public static StorageState i { get; private set; }

    private void Awake()
    {
        i = this;
        party = PockiParty.GetPlayerParty();
    }

    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;

        storageUI.SelectedBox = 0;
        storageUI.gameObject.SetActive(true);
        storageUI.SetDataInPartySlots();
        storageUI.SetDataInStorageSlots();
        storageUI.SetupActionButtons();
        storageUI.UpdateBoxLabel();

        storageUI.OnSelected += OnSlotSelected;
        storageUI.OnBack += OnBack;
    }
    public override void Execute()
    {
        storageUI.HandleUpdate();
    }

    public override void Exit()
    {
        storageUI.gameObject.SetActive(false);
        storageUI.OnSelected -= OnSlotSelected;
        storageUI.OnBack -= OnBack;
    }

    void OnSlotSelected(int slotIndex)
    {
        if (!isMovingPokemon)
        {
            var pokemon = storageUI.TakePockiFromSlot(slotIndex);
            if (pokemon != null)
            {
                isMovingPokemon = true;
                selectedSlotToMove = slotIndex;
                selectedPokemonToMove = pokemon;
            }
        }
        else
        {
            isMovingPokemon = false;

            int firstSlotIndex = selectedSlotToMove;
            int secondSlotIndex = slotIndex;

            var secondPokemon = storageUI.TakePockiFromSlot(slotIndex);

            if (secondPokemon == null && storageUI.IsPartySlot(firstSlotIndex) && storageUI.IsPartySlot(secondSlotIndex))
            {
                storageUI.PutPockiIntoSlot(selectedPokemonToMove, selectedSlotToMove);
                storageUI.SetDataInStorageSlots();
                storageUI.SetDataInPartySlots();
                storageUI.HideMovingImage();
                storageUI.ResetIndex();
                return;
            }

            storageUI.PutPockiIntoSlot(selectedPokemonToMove, secondSlotIndex);

            if (secondPokemon != null)
                storageUI.PutPockiIntoSlot(secondPokemon, firstSlotIndex);

            party.Pockies.RemoveAll(p => p == null);
            party.PartyUpdated();

            storageUI.SetDataInStorageSlots();
            storageUI.SetDataInPartySlots();
            storageUI.HideMovingImage();
            storageUI.ResetIndex();
        }
    }

    void OnBack()
    {
        if (isMovingPokemon)
        {
            isMovingPokemon = false;
            storageUI.PutPockiIntoSlot(selectedPokemonToMove, selectedSlotToMove);

            storageUI.SetDataInStorageSlots();
            storageUI.SetDataInPartySlots();
            storageUI.ResetIndex();
        }
        else
        {
            gc.StateMachine.Pop();
        }
    }
}