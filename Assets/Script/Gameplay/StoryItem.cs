using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialog dialog;

    public void onPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.isMoving = false;
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }

    public bool TriggerRepeatedly => false;
}
