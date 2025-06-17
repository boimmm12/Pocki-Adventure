using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BaitSelectionUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Text baitNameText;
    [SerializeField] private Image baitImage;
    [SerializeField] private Text baitCountText;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject selectionPanel;

    private List<BaitSlot> baitSlots;
    private int selectedIndex = 0;
    private Action<BaitItem> onBaitSelected;

    public static BaitSelectionUI i { get; private set; }

    private void Awake()
    {
        if (i == null)
            i = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);

        leftButton.onClick.AddListener(() => ChangeSelection(-1));
        rightButton.onClick.AddListener(() => ChangeSelection(1));
        confirmButton.onClick.AddListener(OnConfirm);
    }

    public void Show(List<BaitSlot> slots, Action<BaitItem> onSelected)
    {
        gameObject.SetActive(true);
        onBaitSelected = onSelected;

        baitSlots = new List<BaitSlot>();
        baitSlots.Add(new BaitSlot(null, 0)); // Slot "Tanpa Umpan"

        baitSlots.AddRange(slots);
        selectedIndex = 0;
        UpdateUI();
    }

    private void ChangeSelection(int direction)
    {
        if (baitSlots == null || baitSlots.Count == 0) return;

        selectedIndex = (selectedIndex + direction + baitSlots.Count) % baitSlots.Count;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (baitSlots == null || baitSlots.Count == 0) return;

        var slot = baitSlots[selectedIndex];
        var bait = slot.bait;

        if (bait == null)
        {
            baitNameText.text = "Tanpa Umpan";
            baitImage.sprite = null;
            baitImage.color = new Color(1, 1, 1, 0); // hide image
            baitCountText.text = "";
        }
        else
        {
            baitNameText.text = bait.Name;
            baitImage.sprite = bait.Icon;
            baitImage.color = new Color(1, 1, 1, 1); // show image
            baitCountText.text = $"x{slot.count}";
        }
    }

    private void OnConfirm()
    {
        var selectedSlot = baitSlots[selectedIndex];
        var selectedBait = selectedSlot.bait;

        if (selectedBait != null)
        {
            FishInventory.i.RemoveBait(selectedBait);
        }

        gameObject.SetActive(false);
        onBaitSelected?.Invoke(selectedBait);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(
            selectionPanel.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            gameObject.SetActive(false);
            onBaitSelected?.Invoke(null);
        }
    }
}
