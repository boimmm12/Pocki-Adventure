using System;
using UnityEngine;
using UnityEngine.UI;


public class SaveSlotUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button autoSlotButton;
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button backButton;

    [Header("Auto Slot UI")]
    [SerializeField] private Image autoPreview;
    [SerializeField] private Text autoSceneText;
    [SerializeField] private Text autoTimeText;
    [SerializeField] private Text autoNameText;

    [Header("Slot 1 UI")]
    [SerializeField] private Image slot1Preview;
    [SerializeField] private Text slot1SceneText;
    [SerializeField] private Text slot1TimeText;
    [SerializeField] private Text slot1NameText;

    [Header("Slot 2 UI")]
    [SerializeField] private Image slot2Preview;
    [SerializeField] private Text slot2SceneText;
    [SerializeField] private Text slot2TimeText;
    [SerializeField] private Text slot2NameText;



    private Action<SaveSlot> onSlotSelected;

    private void Awake()
    {
        slot1Button.onClick.AddListener(() => OnSlotClicked(SaveSlot.Manual1));
        slot2Button.onClick.AddListener(() => OnSlotClicked(SaveSlot.Manual2));
        autoSlotButton.onClick.AddListener(() => OnSlotClicked(SaveSlot.Auto));
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void Show(bool isLoading, Action<SaveSlot> onSlotSelected)
    {
        Debug.Log("[SaveSlotUI] Show dipanggil");

        this.onSlotSelected = onSlotSelected;
        gameObject.SetActive(true);

        autoSlotButton.gameObject.SetActive(isLoading);

        SetSlotUI(SaveSlot.Manual1, slot1Preview, slot1SceneText, slot1TimeText, slot1NameText);
        SetSlotUI(SaveSlot.Manual2, slot2Preview, slot2SceneText, slot2TimeText, slot2NameText);
        SetSlotUI(SaveSlot.Auto, autoPreview, autoSceneText, autoTimeText, autoNameText);
    }

    private void SetSlotUI(SaveSlot slot, Image previewImage, Text sceneText, Text timeText, Text nameText)
    {
        Debug.Log("SetSlotUI dipanggil");
        string fileName = GetSaveFileName(slot);
        bool hasSave = SavingSystem.i.CheckIfSaveExists(fileName, slot);

        previewImage.enabled = false;
        sceneText.text = "-";
        nameText.text = "-";
        timeText.text = "";


        if (hasSave)
        {
            Debug.Log("has save");
            var meta = SavingSystem.i.GetMetadata(fileName, slot);
            Debug.Log($"[SaveSlotUI] Slot: {slot}, Scene: {meta?.sceneName}, PresetIndex: {meta?.presetIndex}");
            Debug.Log($"[SaveSlotUI] preset index: {meta?.presetIndex}");
            Debug.Log($"[SaveSlotUI] preset count: {GlobalCharacterPresets.i?.presets.Count}");


            var preset = GlobalCharacterPresets.i?.GetPreset(meta?.presetIndex ?? 0);

            if (preset == null)
                Debug.LogWarning($"[SaveSlotUI] Preset null untuk index {meta?.presetIndex}");

            previewImage.sprite = preset?.idleDown;
            previewImage.enabled = (preset?.idleDown != null);

            if (preset?.idleDown == null)
                Debug.LogWarning("[SaveSlotUI] idleDown sprite null!");

            sceneText.text = meta?.sceneName ?? "???";
            timeText.text = meta?.saveTimeString ?? "";
            nameText.text = meta?.playerName ?? "Unknown";
        }
    }

    private string GetSaveFileName(SaveSlot slot)
    {
        return slot switch
        {
            SaveSlot.Auto => "autosave",
            SaveSlot.Manual1 => "manual1",
            SaveSlot.Manual2 => "manual2",
            _ => "save"
        };
    }

    private void OnSlotClicked(SaveSlot slot)
    {
        gameObject.SetActive(false);
        onSlotSelected?.Invoke(slot);
    }
}
