using System.Collections.Generic;
using System.Linq;
using GDE.GenericSelectionUI;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("Basic Details")]
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Image image;
    [SerializeField] Image Background;

    [Header("Pages")]
    [SerializeField] Text pageNameText;
    [SerializeField] GameObject skillsPage;
    [SerializeField] GameObject movesPage;

    [Header("Pocki Skills")]
    [SerializeField] Text hpText;
    [SerializeField] Text attackText;
    [SerializeField] Text defenseText;
    [SerializeField] Text spAttackText;
    [SerializeField] Text spDefenseText;
    [SerializeField] Text speedText;
    [SerializeField] Text expPointsText;
    [SerializeField] Text nextLevelExpText;
    [SerializeField] Transform expBar;

    [Header("Pocki Moves")]
    [SerializeField] List<Text> moveTypes;
    [SerializeField] List<Text> moveNames;
    [SerializeField] List<Text> movePPs;
    [SerializeField] Text movePowerText;
    [SerializeField] Text moveAccuracyText;
    [SerializeField] Text moveCategoryText;
    [SerializeField] Text moveDescText;

    [Header("Button")]
    [SerializeField] Button back;
    [SerializeField] Button nextrightButton;
    [SerializeField] Button nextleftButton;
    [SerializeField] Button statPage;
    [SerializeField] Button movePage;
    [SerializeField] public Button detailsButton;
    [SerializeField] public Button detailsbackButton;

    List<TextSlot> moveSlots;

    private static readonly Dictionary<PockiType, Color> typeColors = new()
    {
        { PockiType.Strength, HexToColor("A5668B") },
        { PockiType.Vitality, HexToColor("4DA167") },
        { PockiType.Agility, HexToColor("462255") }
    };

    private void Start()
    {
        moveSlots = moveNames.Select(m => m.GetComponent<TextSlot>()).ToList();
    }

    bool inMoveSelection;
    public bool InMoveSelection
    {
        get => inMoveSelection;
        set
        {
            inMoveSelection = value;

            if (inMoveSelection)
            {
                SetItems(moveSlots.Take(pokemon.Moves.Count).ToList());
                for (int i = 0; i < moveSlots.Count; i++)
                {
                    int index = i;
                    moveSlots[i].OnClick = () =>
                    {
                        Debug.Log($"[TextSlot] Klik: {index}");
                        OnItemClicked(index);
                        OnConfirmButton();
                    };
                }
                selectedItem = 0;
                UpdateSelectionInUI();
            }
            else
            {
                movePowerText.text = "";
                moveAccuracyText.text = "";
                moveCategoryText.text = "";
                moveDescText.text = "";
                ClearItems();
            }
        }
    }

    Pocki pokemon;
    public bool backButtonclicked = false;
    public bool nextButtonright = false;
    public bool nextButtonleft = false;
    public bool isStatPage = false;
    public bool isMovePage = false;
    public bool isDetailsOpen = false;
    public bool isDetailsClose = false;
    public void SetBasicDetails(Pocki pokemon)
    {
        this.pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lv." + pokemon.Level;
        image.sprite = pokemon.Base.FrontSprite;
    }
    public void ShowPage(int pageNum)
    {
        if (pageNum == 0)
        {
            pageNameText.text = "POCKI SKILLS";
            skillsPage.SetActive(true);
            movesPage.SetActive(false);

            SetSkills();
        }
        else if (pageNum == 1)
        {
            pageNameText.text = "POCKI MOVES";
            skillsPage.SetActive(false);
            movesPage.SetActive(true);

            SetMoves();
        }
    }

    public void SetSkills()
    {
        hpText.text = $"{pokemon.HP}/{pokemon.MaxHp}";
        attackText.text = "" + pokemon.Attack;
        defenseText.text = "" + pokemon.Defense;
        spAttackText.text = "" + pokemon.SpAttack;
        spDefenseText.text = "" + pokemon.SpDefense;
        speedText.text = "" + pokemon.Speed;
        if (typeColors.TryGetValue(pokemon.Base.Type1, out var color))
            Background.color = color;
        else
            Background.color = Color.gray;


        expPointsText.text = "" + pokemon.Exp;
        nextLevelExpText.text = "" + (pokemon.Base.GetExpForLevel(pokemon.Level + 1) - pokemon.Exp);
        expBar.localScale = new Vector2(pokemon.GetNormalizedExp(), 1);
    }
    public void SetMoves()
    {
        for (int i = 0; i < moveNames.Count; i++)
        {
            if (i < pokemon.Moves.Count)
            {
                var move = pokemon.Moves[i];

                moveTypes[i].text = move.Base.Type.ToString().ToUpper();
                moveNames[i].text = move.Base.Name.ToUpper();
                movePPs[i].text = $"PP {move.PP}/{move.Base.PP}";
            }
            else
            {
                moveTypes[i].text = "-";
                moveNames[i].text = "-";
                movePPs[i].text = "-";
            }
        }
    }
    public void SetButton()
    {
        back.onClick.AddListener(() => backButtonclicked = true);
        nextrightButton.onClick.AddListener(() => nextButtonright = true);
        nextleftButton.onClick.AddListener(() => nextButtonleft = true);
        statPage.onClick.AddListener(() => isStatPage = true);
        movePage.onClick.AddListener(() => isMovePage = true);
        detailsButton.onClick.AddListener(() => isDetailsOpen = true);
        detailsbackButton.onClick.AddListener(() => isDetailsClose = true);
    }

    public override void HandleUpdate()
    {
        if (InMoveSelection)
            base.HandleUpdate();
    }
    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var move = pokemon.Moves[selectedItem];

        movePowerText.text = "" + move.Base.Power;
        moveAccuracyText.text = "" + move.Base.Accuracy;
        moveCategoryText.text = "" + move.Base.Category;
        moveDescText.text = move.Base.Description;
    }
    public static Color HexToColor(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString($"#{hex}", out color))
            return color;
        else
            return Color.magenta; // fallback kalau gagal
    }

}
