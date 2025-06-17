using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] ChoiceBox choiceBox;
    [SerializeField] Text dialogText;
    [SerializeField] int letterPerSecond;
    [SerializeField] Button nextButton;

    public event Action OnShowDialog;
    public event Action OnDialogFinished;
    public static DialogManager Instance { get; private set; }
    private bool isButtonClicked = false;


    private void Awake()
    {
        Instance = this;
        nextButton.onClick.AddListener(() => isButtonClicked = true);
    }

    public bool IsShowing { get; set; }

    public IEnumerator ShowDialogText(string text, bool waitForInput = true, bool autoClose = true, List<string> choices = null, Action<int> onChoiceSelected = null, Action onCancel = null)
    {
        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        AudioManager.i.PlaySfx(AudioId.UISelect);
        yield return TypeDialog(text);
        if (waitForInput)
        {
            yield return new WaitUntil(() =>
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) ||
                Input.GetMouseButtonDown(0) // Left mouse click
            );
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected, onCancel);
        }

        if (autoClose)
        {
            CloseDialog();
        }

        OnDialogFinished?.Invoke();
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        IsShowing = false;
    }
    public IEnumerator ShowDialog(Dialog dialog, List<string> choices = null, Action<int> onChoiceSelected = null, Action onCancel = null)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {
            AudioManager.i.PlaySfx(AudioId.UISelect);
            yield return TypeDialog(line);
            isButtonClicked = false;
            yield return new WaitUntil(() => isButtonClicked);
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected, onCancel);
        }

        dialogBox.SetActive(false);
        IsShowing = false;
        OnDialogFinished?.Invoke();
    }

    public void HandleUpdate()
    {

    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }
    }
}
