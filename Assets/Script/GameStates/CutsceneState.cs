using GDEUtils.StateMachine;
using UnityEngine;

public class CutsceneState : State<GameController>
{
    public static CutsceneState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public override void Execute()
    {
        PlayerController.i.Character.HandleUpdate();
    }
}
