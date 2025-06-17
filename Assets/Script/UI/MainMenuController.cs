using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : SelectionUI<TextSlot>
{
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] GameObject playerSetup;
    [SerializeField] GameObject credit;
    [SerializeField] private SaveSlotUI saveSlotUI;


    private void Start()
    {
        var textSlots = GetComponentsInChildren<TextSlot>().ToList();
        SetupMenuButtons();

        if (!SavingSystem.i.CheckIfSaveExists("autosave", SaveSlot.Auto))

        {
            var firstSlot = textSlots[0];
            var textComponent = firstSlot.GetComponentInChildren<Text>();
            var buttonComponent = firstSlot.GetComponentInChildren<Button>();

            if (textComponent != null)
                textComponent.color = Color.gray;

            if (buttonComponent != null)
                buttonComponent.interactable = false;
        }
        SetItems(textSlots);
        if (mainMenuMusic != null)
            AudioManager.i.PlayMusic(mainMenuMusic, fade: true);

        OnSelected += OnItemSelected;
    }
    public void SetupMenuButtons()
    {
        var items = GetComponentsInChildren<TextSlot>().ToList();
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            items[i].OnClick = () =>
            {
                Debug.Log($"[TextSlot] Klik: {index}");
                OnItemClicked(index);
                OnConfirmButton();
            };
        }

        SetItems(items);
    }
    private void Update()
    {
        HandleUpdate();
    }
    public override void UpdateSelectionInUI()
    {

    }

    void OnItemSelected(int selection)
    {
        if (selection == 0)
        {
            string fileName = "autosave";
            SaveSlot slot = SaveSlot.Auto;

            if (!SavingSystem.i.CheckIfSaveExists(fileName, slot))
            {
                Debug.Log("[MainMenu] Tidak ada autosave. Tidak bisa Continue.");
                return;
            }

            AudioManager.i.StopMusic(fade: true);
            DontDestroyOnLoad(LoadingScreenController.i.gameObject);

            LoadingScreenController.i.StartLoadingScene(1, () =>
            {
                SavingSystem.i.Load(fileName, slot);
                GameController.Instance.StateMachine.ChangeState(FreeRoamState.i);
            });

            Destroy(gameObject);
        }

        else if (selection == 1)
        {
            playerSetup.gameObject.SetActive(true);
        }
        else if (selection == 2)
        {
            saveSlotUI.Show(true, selectedSlot =>
            {
                string fileName = selectedSlot switch
                {
                    SaveSlot.Auto => "autosave",
                    SaveSlot.Manual1 => "manual1",
                    SaveSlot.Manual2 => "manual2",
                    _ => "save"
                };

                if (!SavingSystem.i.CheckIfSaveExists(fileName, selectedSlot))
                {
                    Debug.Log($"[MainMenu] Tidak ada save di slot: {selectedSlot}");
                    return;
                }

                AudioManager.i.StopMusic(fade: true);
                DontDestroyOnLoad(LoadingScreenController.i.gameObject);

                LoadingScreenController.i.StartLoadingScene(1, () =>
                {
                    SavingSystem.i.Load(fileName, selectedSlot);
                    GameController.Instance.StateMachine.ChangeState(FreeRoamState.i);
                });

                Destroy(gameObject);
            });
        }
        else if(selection == 3)
        {
            credit.gameObject.SetActive(true);
        }
    }
}
