using System.Collections;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    [SerializeField] int rewardMoney = 500;
    [SerializeField] Dialog moneyRewardDialog;


    [SerializeField] AudioClip trainerAppears;
    ItemGiver itemGiver;
    bool battleLost = false;
    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookToward(initiator.position);

        if (!battleLost)
        {
            AudioManager.i.PlayMusic(trainerAppears);

            yield return DialogManager.Instance.ShowDialog(dialog);
            GameController.Instance.StartTrainerBattle(this);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }
    }


    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        GameController.Instance.StateMachine.Push(CutsceneState.i);

        AudioManager.i.PlayMusic(trainerAppears);

        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        yield return DialogManager.Instance.ShowDialog(dialog);

        GameController.Instance.StateMachine.Pop();

        GameController.Instance.StartTrainerBattle(this);
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public IEnumerator HandleBattleEnd()
    {
        BattleLost();

        if (itemGiver != null && itemGiver.CanBeGiven())
            yield return GiveReward();

        if (rewardMoney > 0)
            yield return GiveMoney();
    }

    private IEnumerator GiveReward()
    {
        var player = GameObject.FindObjectOfType<PlayerController>();
        yield return itemGiver.GiveItem(player);
    }

    private IEnumerator GiveMoney()
    {
        var player = GameObject.FindObjectOfType<PlayerController>();

        if (moneyRewardDialog != null)
            yield return DialogManager.Instance.ShowDialog(moneyRewardDialog);

        Wallet.i.AddMoney(rewardMoney);

        yield return DialogManager.Instance.ShowDialogText($"{player.Name} received 🪙{rewardMoney}!");
    }


    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;
        if (battleLost)
            fov.gameObject.SetActive(false);
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
