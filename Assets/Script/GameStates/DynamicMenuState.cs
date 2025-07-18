using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

public class DynamicMenuState : State<GameController>
{
    [SerializeField] DynamicMenuUI dynamicMenuUI;
    [SerializeField] TextSlot itemTextPrefab;
    public List<string> MenuItems { get; set; }

    public int? SelectedItem { get; private set; }

    public static DynamicMenuState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        foreach (Transform child in dynamicMenuUI.transform)  // Jangan lupa OptionContainer
            Destroy(child.gameObject);

        var itemTextSlots = new List<TextSlot>();

        foreach (var menuItem in MenuItems)
        {
            var itemTextSlot = Instantiate(itemTextPrefab, dynamicMenuUI.transform);  // <- ke container
            itemTextSlot.SetText(menuItem);
            itemTextSlots.Add(itemTextSlot);
        }

        dynamicMenuUI.SetItems(itemTextSlots);          // ⬅ Panggil SetItems dulu
        dynamicMenuUI.SetupDynamicMenuButtons(itemTextSlots);         // ⬅ Lalu baru pasang listener

        dynamicMenuUI.gameObject.SetActive(true);
        dynamicMenuUI.OnSelected += OnItemSelected;
        dynamicMenuUI.OnBack += OnBack;
    }


    public override void Execute()
    {
        dynamicMenuUI.HandleUpdate();
    }
    public override void Exit()
    {
        dynamicMenuUI.ClearItems();

        dynamicMenuUI.gameObject.SetActive(false);
        dynamicMenuUI.OnSelected -= OnItemSelected;
        dynamicMenuUI.OnBack -= OnBack;
    }
    void OnItemSelected(int selectedItem)
    {
        SelectedItem = selectedItem;
        gc.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedItem = null;
        gc.StateMachine.Pop();
    }

}
