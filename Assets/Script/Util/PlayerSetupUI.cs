using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetupUI : MonoBehaviour
{
    public static PlayerSetupUI i { get; private set; }

    [SerializeField] InputField nameInputField;
    [SerializeField] Image spritePreview;
    [SerializeField] Text spriteNameText;
    [SerializeField] Text specialNoteText;
    [SerializeField] private Text errorText;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button confirmButton;
    [SerializeField] Button backButton;

    [SerializeField] public List<CharacterPreset> characterPresets;
    public int currentPresetIndex = 0;
    public int CurrentPresetIndex => currentPresetIndex;

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);

        prevButton.onClick.AddListener(OnPrevClicked);
        nextButton.onClick.AddListener(OnNextClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void Start()
    {
        UpdateSpritePreview();
    }

    private void OnPrevClicked()
    {
        currentPresetIndex--;
        if (currentPresetIndex < 0)
            currentPresetIndex = characterPresets.Count - 1;

        UpdateSpritePreview();
    }

    private void OnNextClicked()
    {
        currentPresetIndex++;
        if (currentPresetIndex >= characterPresets.Count)
            currentPresetIndex = 0;

        UpdateSpritePreview();
    }

    private void OnConfirmClicked()
    {
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            ShowError($"{characterPresets[currentPresetIndex].specialErrorBlank}");
            return;
        }

        if (nameInputField.text.Length > 12)
        {
            ShowError($"{characterPresets[currentPresetIndex].specialErrorLong}");
            return;
        }

        var player = PlayerController.i;
        player.SetNameAndPreset(nameInputField.text.ToUpper(), characterPresets[currentPresetIndex]);
        AudioManager.i.StopMusic(fade: true);

        DontDestroyOnLoad(LoadingScreenController.i.gameObject);
        GameController.Instance.StateMachine.ChangeState(FreeRoamState.i);
        SavingSystem.i.Delete("autosave", SaveSlot.Auto);

        LoadingScreenController.i.StartLoadingScene(1);

        gameObject.SetActive(false);
    }
    void OnBackClicked()
    {
        gameObject.SetActive(false);
    }

    private void UpdateSpritePreview()
    {
        if (characterPresets.Count > 0)
            spritePreview.sprite = characterPresets[currentPresetIndex].idleDown;

        spriteNameText.text = characterPresets[currentPresetIndex].presetname.ToUpper();
        var note = characterPresets[currentPresetIndex].specialNote;
        if (!string.IsNullOrEmpty(note))
            specialNoteText.text = note;
        else
            specialNoteText.text = "";

        errorText.text = "";
    }

    public void SetPresetFromLoad(int presetIndex)
    {
        if (presetIndex >= 0 && presetIndex < characterPresets.Count)
        {
            currentPresetIndex = presetIndex;
            UpdateSpritePreview();
        }
    }
    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            CancelInvoke(nameof(ClearError));
            Invoke(nameof(ClearError), 3f);
        }
    }

    private void ClearError()
    {
        if (errorText != null)
            errorText.text = "";
    }

}
