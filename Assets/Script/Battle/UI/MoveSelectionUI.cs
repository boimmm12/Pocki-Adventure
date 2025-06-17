using System.Collections.Generic;
using System.Linq;
using GDE.GenericSelectionUI;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TextSlot> moveTexts;
    [SerializeField] Button[] moveButtons;
    [SerializeField] Button confirmButton;


    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    [SerializeField] Text effectivenessText;
    Pocki targetPokemon;

    public void SetTarget(Pocki target) => targetPokemon = target;

    List<Move> _moves;
    public void SetMoves(List<Move> moves)
    {
        _moves = moves;

        selectedItem = 0;

        SetItems(moveTexts.Take(moves.Count).ToList());

        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                moveTexts[i].SetText(moves[i].Base.Name);
            }
            else
            {
                moveTexts[i].SetText("-");
            }
        }
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
                    confirmButton.gameObject.SetActive(true);
                });
            }
            else
            {
                button.interactable = false;
            }
        }

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            OnConfirmButton();
            confirmButton.gameObject.SetActive(false);
        });
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var move = _moves[selectedItem];

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
            ppText.color = Color.red;
        else if (move.PP <= move.Base.PP / 2)
            ppText.color = new Color(1f, 0.647f, 0f);
        else
            ppText.color = Color.black;

        if (targetPokemon != null)
        {
            var moveType = move.Base.Type;
            var type1 = targetPokemon.Base.Type1;
            var type2 = targetPokemon.Base.Type2;

            float effectiveness = TypeChart.GetEffectiveness(moveType, type1);

            if (type2 != PockiType.None)
                effectiveness *= TypeChart.GetEffectiveness(moveType, type2);

            if (effectiveness == 0f)
            {
                effectivenessText.text = "No Effect";
                effectivenessText.color = Color.gray;
            }
            else if (effectiveness < 1f)
            {
                effectivenessText.text = "Not\nVery Effective";
                effectivenessText.color = Color.red;
            }
            else if (effectiveness > 1f)
            {
                effectivenessText.text = "Super Effective!";
                effectivenessText.color = Color.green;
            }
            else
            {
                effectivenessText.text = "Normal";
                effectivenessText.color = Color.black;
            }

        }
        else
        {
            effectivenessText.text = "";
        }


    }
}
