using GDEUtils.StateMachine;
using System.Collections;
using UnityEngine;

public class GameMenuState : State<GameController>
{
    [SerializeField] private MenuController menuController;
    [SerializeField] private QuestJournalUI questJournal;
    [SerializeField] private FishStorageUI fishStorageUI;
    [SerializeField] private SaveSlotUI saveSlotUI;

    public static GameMenuState i { get; private set; }

    private bool backPressedOnce = false;
    private float backPressedTimer = 0f;
    private const float backPressDelay = 4f;

    private GameController gc;

    private void Awake()
    {
        i = this;
    }

    public override void Enter(GameController owner)
    {
        gc = owner;

        menuController.gameObject.SetActive(true);
        menuController.OnSelected += OnMenuItemSelected;
        menuController.OnBack += OnBack;
    }

    public override void Execute()
    {
        menuController.HandleUpdate();
        UpdateBackPress();
    }

    public override void Exit()
    {
        menuController.gameObject.SetActive(false);
        menuController.OnSelected -= OnMenuItemSelected;
        menuController.OnBack -= OnBack;
    }

    private void OnMenuItemSelected(int selection)
    {
        switch (selection)
        {
            case 0: // Bag
                gc.StateMachine.Push(InventoryState.i);
                break;
            case 1: // Party
                gc.StateMachine.Push(PartyState.i);
                break;
            case 2: // Storage
                gc.StateMachine.Push(StorageState.i);
                break;
            case 3: // Fish Storage
                fishStorageUI.Show();
                break;
            case 4: // Journal
                questJournal.gameObject.SetActive(true);
                break;
            case 5: // Save
                saveSlotUI.Show(false, slot => StartCoroutine(SaveToSlot(slot)));
                break;
            case 6: // Load
                saveSlotUI.Show(isLoading: true, onSlotSelected: (selectedSlot) =>
                {
                    StartCoroutine(LoadFromSlot(selectedSlot));
                });
                break;
            case 7: // Quit
                QuitGame();
                break;
        }
    }

    private IEnumerator SaveToSlot(SaveSlot slot)
    {
        yield return Fader.i.FadeIn(0.5f);
        SavingSystem.i.Save(GetSaveFileName(slot), slot);
        yield return Fader.i.FadeOut(0.5f);
    }

    private IEnumerator LoadFromSlot(SaveSlot slot)
    {
        yield return Fader.i.FadeIn(0.5f);
        SavingSystem.i.Load(GetSaveFileName(slot), slot);
        yield return Fader.i.FadeOut(0.5f);
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

    private void UpdateBackPress()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (backPressedOnce)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                backPressedOnce = true;
                backPressedTimer = 0f;
                ShowAndroidToast("Press back again to exit");
            }
        }

        if (backPressedOnce)
        {
            backPressedTimer += Time.deltaTime;
            if (backPressedTimer > backPressDelay)
            {
                backPressedOnce = false;
            }
        }
    }

    private void ShowAndroidToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (activity != null)
        {
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", context, message, 0);
                toast.Call("show");
            }));
        }
    }
#endif
    }

    private void QuitGame()
    {
        Debug.Log("Quit game triggered");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For testing in editor
#endif
    }
    private void OnBack()
    {
        gc.StateMachine.Pop();
    }
}
