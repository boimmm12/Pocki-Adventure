using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreen;

    int selectedPage = 0;
    public static SummaryState i { get; private set; }

    public int SelectedPokemonIndex { get; set; }
    private void Awake()
    {
        i = this;
    }

    List<Pocki> playerParty;

    void Start()
    {
        playerParty = PlayerController.i.GetComponent<PockiParty>().Pockies;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        playerParty = PlayerController.i.GetComponent<PockiParty>().Pockies;

        selectedPage = 0;
        if (SelectedPokemonIndex >= playerParty.Count)
            SelectedPokemonIndex = 0;
        summaryScreen.gameObject.SetActive(true);
        summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
        summaryScreen.ShowPage(selectedPage);
        summaryScreen.SetButton();
    }

    public override void Execute()
    {
        playerParty = PlayerController.i.GetComponent<PockiParty>().Pockies;

        if (playerParty == null || playerParty.Count == 0)
        {
            Debug.LogWarning("[SummaryState] Party kosong! Keluar dari Summary.");
            gc.StateMachine.Pop();
            return;
        }

        if (SelectedPokemonIndex >= playerParty.Count)
        {
            Debug.LogWarning("[SummaryState] Selected index out of range. Resetting to 0.");
            SelectedPokemonIndex = 0;
        }

        if (!summaryScreen.InMoveSelection)
        {
            //Page Selection
            int prevPage = selectedPage;
            if (summaryScreen.isStatPage)
            {
                summaryScreen.isStatPage = false;
                selectedPage = Mathf.Abs((selectedPage - 1) % 2);
            }
            else if (summaryScreen.isMovePage)
            {
                summaryScreen.isMovePage = false;
                selectedPage = Mathf.Abs((selectedPage + 1) % 2);
            }

            if (selectedPage != prevPage)
            {
                summaryScreen.ShowPage(selectedPage);
            }

            //Pocki Selection
            int prevSelection = SelectedPokemonIndex;
            if (summaryScreen.nextButtonright)
            {
                SelectedPokemonIndex += 1;
                summaryScreen.nextButtonright = false;

                if (SelectedPokemonIndex >= playerParty.Count)
                    SelectedPokemonIndex = 0;
            }
            else if (summaryScreen.nextButtonleft)
            {
                SelectedPokemonIndex -= 1;
                summaryScreen.nextButtonleft = false;

                if (SelectedPokemonIndex < 0)
                    SelectedPokemonIndex = playerParty.Count - 1;
            }

            if (SelectedPokemonIndex != prevSelection)
            {
                if (SelectedPokemonIndex >= playerParty.Count)
                    SelectedPokemonIndex = 0;

                summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
                summaryScreen.ShowPage(selectedPage);
            }
        }

        if (summaryScreen.isDetailsOpen)
        {
            summaryScreen.isDetailsOpen = false;
            summaryScreen.detailsbackButton.gameObject.SetActive(true);
            summaryScreen.detailsButton.gameObject.SetActive(false);

            if (selectedPage == 1 && !summaryScreen.InMoveSelection)
            {
                summaryScreen.InMoveSelection = true;
            }
        }
        else if (summaryScreen.isDetailsClose)
        {
            summaryScreen.isDetailsClose = false;
            summaryScreen.InMoveSelection = false;
            summaryScreen.detailsButton.gameObject.SetActive(true);
            summaryScreen.detailsbackButton.gameObject.SetActive(false);
        }
        else if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape) || summaryScreen.backButtonclicked)
        {
            if (summaryScreen.InMoveSelection)
            {
                summaryScreen.backButtonclicked = false;
                summaryScreen.InMoveSelection = false;
                summaryScreen.detailsButton.gameObject.SetActive(true);
                summaryScreen.detailsbackButton.gameObject.SetActive(false);
            }
            else
            {
                summaryScreen.backButtonclicked = false;
                gc.StateMachine.Pop();
                return;
            }
        }

        summaryScreen.HandleUpdate();
    }

    public override void Exit()
    {
        summaryScreen.gameObject.SetActive(false);
        PartyState.i.ResetSelectionInParty();
    }
}