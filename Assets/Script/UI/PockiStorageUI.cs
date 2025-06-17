using System.Collections.Generic;
using GDE.GenericSelectionUI;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PockiStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> boxSlots;
    [SerializeField] Button backB;
    [SerializeField] Button Rbox;
    [SerializeField] Button Lbox;
    [SerializeField] Image movingPockiImage;
    [SerializeField] Text boxNameText;

    bool isRbox = false;
    bool isLbox = false;

    List<BoxPartySlotUI> partySlots = new List<BoxPartySlotUI>();
    List<BoxStorageSlotUI> storageSlots = new List<BoxStorageSlotUI>();

    List<Image> boxSlotImage = new List<Image>();
    PockiParty party;
    PockiStorageBox storageBoxes;

    public int SelectedBox { get; set; } = 0;

    private void Awake()
    {
        foreach (var boxSlot in boxSlots)
        {
            var storageSlot = boxSlot.GetComponent<BoxStorageSlotUI>();
            if (storageSlot != null)
            {
                storageSlots.Add(storageSlot);
            }
            else
            {
                partySlots.Add(boxSlot.GetComponent<BoxPartySlotUI>());
            }
        }
        party = PockiParty.GetPlayerParty();
        storageBoxes = PockiStorageBox.GetPlayerStorageBox();

        boxSlotImage = boxSlots.Select(b => b.transform.GetChild(0).GetComponent<Image>()).ToList();
        movingPockiImage.gameObject.SetActive(false);
    }
    private void Start()
    {
        SetItems(boxSlots);
        ResetIndex();
    }
    public void SetupActionButtons()
    {

        var items = GetComponentsInChildren<ImageSlot>().ToList();
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            items[i].OnClick = () =>
            {
                OnItemClicked(index);
                OnConfirmButton();
            };
        }

        backB.onClick.AddListener(() => OnBackButton());
        Rbox.onClick.AddListener(() => isRbox = true);
        Lbox.onClick.AddListener(() => isLbox = true);
    }
    public void SetDataInPartySlots()
    {
        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < party.Pockies.Count)
                partySlots[i].SetData(party.Pockies[i]);
            else
                partySlots[i].ClearData();
        }
    }

    public void SetDataInStorageSlots()
    {
        for (int i = 0; i < storageSlots.Count; i++)
        {
            var pokemon = storageBoxes.GetPokemon(SelectedBox, i);
            if (pokemon != null)
                storageSlots[i].SetData(pokemon);
            else
                storageSlots[i].ClearData();
        }
    }
    public override void HandleUpdate()
    {
        int prevSelectedBox = SelectedBox;

        if (isLbox)
        {
            isLbox = false;
            SelectedBox = SelectedBox > 0 ? SelectedBox - 1 : storageBoxes.NumOfBoxes - 1;
        }
        else if (isRbox)
        {
            isRbox = false;
            SelectedBox = (SelectedBox + 1) % storageBoxes.NumOfBoxes;
        }

        if (prevSelectedBox != SelectedBox)
        {
            SetDataInStorageSlots();
            UpdateSelectionInUI();
            return;
        }

        base.HandleUpdate();
    }
    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        boxNameText.text = "Box " + (SelectedBox + 1);

        if (movingPockiImage.gameObject.activeSelf)
            movingPockiImage.transform.position = boxSlotImage[selectedItem].transform.position + Vector3.up * 50f;
    }
    public bool IsPartySlot(int slotIndex)
    {
        return slotIndex < 6;
    }

    public Pocki TakePockiFromSlot(int slotIndex)
    {
        Pocki pokemon;
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex;

            if (partyIndex >= party.Pockies.Count)
                return null;

            pokemon = party.Pockies[partyIndex];
            if (pokemon == null) return null;

            party.Pockies[partyIndex] = null;
        }
        else
        {
            int boxIndex = slotIndex - partySlots.Count;
            pokemon = storageBoxes.GetPokemon(SelectedBox, boxIndex);
            if (pokemon == null) return null;

            storageBoxes.RemovePokemon(SelectedBox, boxIndex);
        }
        movingPockiImage.sprite = boxSlotImage[slotIndex].sprite;
        movingPockiImage.transform.position = boxSlotImage[slotIndex].transform.position + Vector3.up * 50f;
        boxSlotImage[slotIndex].color = new Color(1, 1, 1, 0);
        movingPockiImage.gameObject.SetActive(true);

        return pokemon;
    }

    public void PutPockiIntoSlot(Pocki pokemon, int slotIndex)
    {
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex;

            if (partyIndex >= party.Pockies.Count)
                party.AddPocki(pokemon);
            else
                party.Pockies[partyIndex] = pokemon;
        }
        else
        {
            int boxIndex = slotIndex - partySlots.Count;
            storageBoxes.AddPokemon(pokemon, SelectedBox, boxIndex);
        }
    }
    public void ResetIndex()
    {
        selectedItem = -1;
        UpdateSelectionInUI();
        UpdateBoxLabel();
    }
    public void HideMovingImage()
    {
        movingPockiImage.gameObject.SetActive(false);
    }
    public void UpdateBoxLabel()
    {
        boxNameText.text = $"Box {SelectedBox + 1}";
    }
}
