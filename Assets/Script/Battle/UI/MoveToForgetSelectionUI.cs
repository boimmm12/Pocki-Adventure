using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDE.GenericSelectionUI;
using UnityEngine;
using UnityEngine.UI;

public class MoveToForgetSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<Text> moveTexts;
    [SerializeField] Button[] moveButtons;
    [SerializeField] private Button confirmButton;


    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; ++i)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        if (currentMoves.Count < moveTexts.Count)
            moveTexts[currentMoves.Count].text = newMove.Name;

        SetItems(moveTexts.Select(m => m.GetComponent<TextSlot>()).ToList());
        SetupMoveButtons();
    }

    private void SetupMoveButtons()
    {
        for (int i = 0; i < moveButtons.Length; i++)
        {
            int index = i;
            var button = moveButtons[i];

            button.onClick.RemoveAllListeners();

            if (i < moveTexts.Count)
            {
                button.interactable = true;
                button.onClick.AddListener(() =>
                {
                    Debug.Log($"[Button] Klik: {index}");
                    OnItemClicked(index);
                });
            }
            else
            {
                button.interactable = false;
            }
        }

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmButton);
    }
}