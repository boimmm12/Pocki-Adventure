using GDEUtils.StateMachine;
using UnityEngine;

public class DialogueState : State<GameController>
{
    public static DialogueState i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}
