using System;
using System.Collections.Generic;
using System.Linq;
using GDE.GenericSelectionUI;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] Text messageText;
    [SerializeField] Button[] partyButtons;
    [SerializeField] Button backPartyButton;

    PartyMemberUI[] memberSlots;
    List<Pocki> pokemons;
    PockiParty party;
    public bool CanInteract { get; set; } = true;


    public Pocki SelectedMember => pokemons[selectedItem];


    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        party = PockiParty.GetPlayerParty();

        party.OnUpdated += SetPartyData;
        SetPartyData();
        SetupButtons();
    }

    public void SetPartyData()
    {
        pokemons = party.Pockies;
        ClearItems();

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        var textSlots = memberSlots.Select(m => m.GetComponent<TextSlot>()).ToList();
        SetItems(textSlots.Take(pokemons.Count).ToList());

        selectedItem = -1;
        UpdateSelectionInUI();

        messageText.text = "Choose a Pocki";
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (!CanInteract) return;
        for (int i = 0; i < partyButtons.Length; i++)
        {
            int index = i;
            var button = partyButtons[i];
            var textSlot = memberSlots[i].GetComponent<TextSlot>();

            button.onClick.RemoveAllListeners();

            if (i < pokemons.Count)
            {
                button.interactable = true;
                button.onClick.AddListener(() => OnItemClicked(index));
            }
            else
            {
                button.interactable = false;
            }
        }

        backPartyButton.onClick.RemoveAllListeners();
        backPartyButton.onClick.AddListener(OnBackButton);
    }

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i]) ? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
    private void OnItemClicked(int index)
    {
        if (!CanInteract) return;
        selectedItem = index;
        UpdateSelectionInUI();

        OnConfirmButton();
    }
    public void ResetSelection()
    {
        selectedItem = -1;
        UpdateSelectionInUI();
    }

    public void ResetTmUsableMessages()
    {
        foreach (var slot in memberSlots)
            slot.SetMessage("");  // Atau null
    }
}
