using System;
using System.Collections;
using System.Reflection.Emit;
using GDEUtils.StateMachine;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MenuController menuController;

    public StateMachine<GameController> StateMachine { get; private set; }
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }
    public static GameController Instance { get; set; }

    private void Awake()
    {
        Instance = this;
        ConditionsDB.Init();
        PockiDB.Init();
        MoveDB.Init();
        ItemDB.Init();
        FishDB.Init();
        QuestDB.Init();
        BaitDB.Init();
    }

    private void Start()
    {
        StateMachine = new StateMachine<GameController>(this);
        StateMachine.ChangeState(PauseState.i);

        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            StateMachine.Push(DialogueState.i);
        };

        DialogManager.Instance.OnDialogFinished += () =>
        {
            StateMachine.Pop();
        };
        menuController.SetupMenuButtons();
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            StateMachine.Push(PauseState.i);
        }
        else
        {
            StateMachine.Pop();
        }
    }
    public void StartBattle(BattleTrigger trigger)
    {
        BattleSystemState.i.trigger = trigger;
        StateMachine.Push(BattleSystemState.i);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        BattleSystemState.i.trainer = trainer;
        StateMachine.Push(BattleSystemState.i);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }
    public void EnterCameraState()
    {
        StateMachine.Push(CameraState.i);
    }

    public void StartClassifiedBattle(string klasifikasi)
    {
        var playerParty = playerController.GetComponent<PockiParty>();

        PockiBase targetBase = null;
        int level = GetRandomLevel();

        switch (klasifikasi.ToLower())
        {
            case "bengal":
                targetBase = PockiDB.GetObjectByName("Bengal");
                level = GetRandomLevel();
                break;

            case "maine coon":
                targetBase = PockiDB.GetObjectByName("MaineCoon");
                level = GetRandomLevel();
                break;

            case "himalayan":
                targetBase = PockiDB.GetObjectByName("Himalayan");
                level = GetRandomLevel();
                break;

            case "british short hair":
                targetBase = PockiDB.GetObjectByName("BSH");
                level = GetRandomLevel();
                break;

            case "egyptian mau":
                targetBase = PockiDB.GetObjectByName("E-Mau");
                level = GetRandomLevel();
                break;

            case "domestic":
                targetBase = PockiDB.GetObjectByName("Domestic");
                level = GetRandomLevel();
                break;

            case "persian":
                targetBase = PockiDB.GetObjectByName("Persian");
                level = GetRandomLevel();
                break;

            case "sphynx":
                targetBase = PockiDB.GetObjectByName("Sphynx");
                level = GetRandomLevel();
                break;

            case "ragdoll":
                targetBase = PockiDB.GetObjectByName("Ragdoll");
                level = GetRandomLevel();
                break;

            case "scottish fold":
                targetBase = PockiDB.GetObjectByName("ScottishFold");
                level = GetRandomLevel();
                break;

            case "bukan kucing":
                Debug.LogWarning($"Klasifikasi '{klasifikasi}' tidak dikenali. Tidak memulai battle.");
                return;

            default:
                Debug.LogWarning($"Klasifikasi '{klasifikasi}' tidak dikenali. Tidak memulai battle.");
                return;
        }

        Pocki classifiedPokemon = new Pocki(targetBase, level);
        classifiedPokemon.Init();

        BattleSystemState.i.trainer = null;
        BattleSystemState.i.overrideWildPokemon = classifiedPokemon;

        StateMachine.Push(BattleSystemState.i);
    }
    int GetRandomLevel()
    {
        float roll = UnityEngine.Random.value;

        if (roll < 0.4f)
            return UnityEngine.Random.Range(1, 6);
        else if (roll < 0.8f)
            return UnityEngine.Random.Range(6, 10);
        else
            return UnityEngine.Random.Range(10, 13);
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            StartCoroutine(trainer.HandleBattleEnd());
            trainer = null;
        }

        partyScreen.SetPartyData();

        playerController.EnableInput();
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<PockiParty>();
        if (trainer == null)
        {
            bool hasEvolutions = playerParty.CheckForEvolutions();

            if (hasEvolutions)
                StartCoroutine(playerParty.RunEvolutions());
            else
                AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        }

    }
    private void Update()
    {
        StateMachine.Execute();
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut = false)
    {
        yield return Fader.i.FadeIn(0.5f);

        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        if (waitForFadeOut)
            yield return Fader.i.FadeOut(0.5f);
        else
            StartCoroutine(Fader.i.FadeOut(0.5f));
    }

    public PlayerController PlayerController => playerController;
    public Camera WorldCamera => worldCamera;
    public PartyScreen PartyScreen => partyScreen;
}
