using GDEUtils.StateMachine;
using UnityEngine;

public class FreeRoamState : State<GameController>
{
    public static FreeRoamState i { get; private set; }
    [SerializeField] MenuController mc;
    [SerializeField] GameObject UI;
    void Awake()
    {
        i = this;
    }
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        UI.gameObject.SetActive(true);
    }
    public override void Execute()
    {
        PlayerController.i.HandleUpdate();
        PlayerController.i.UpdateCameraButton();
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape) || mc.isopenmenu)
        {
            Debug.Log($"[FreeRoamState] isopenmenu: {mc?.isopenmenu}");

            gc.StateMachine.Push(GameMenuState.i);
            mc.isopenmenu = false;
        }
    }

}
