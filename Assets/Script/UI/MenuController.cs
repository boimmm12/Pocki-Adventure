using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : SelectionUI<TextSlot>
{
    [SerializeField] Button openMenu;
    [SerializeField] Button closeMenu;

    public bool isopenmenu = false;
    private void Start()
    {
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }
    public void SetupMenuButtons()
    {
        var items = GetComponentsInChildren<TextSlot>().ToList();
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            items[i].OnClick = () =>
            {
                OnItemClicked(index);
                OnConfirmButton();
            };
        }

        openMenu.onClick.AddListener(() =>
        {
            isopenmenu = true;
        });
        closeMenu.onClick.AddListener(() =>
        {
            OnBackButton();
        });

        SetItems(items);
    }

    public override void UpdateSelectionInUI()
    {

    }
}