using UnityEngine;

public class SandDune : MonoBehaviour, IPlayerTriggerable
{
    public void onPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.isMoving = false;
            GameController.Instance.StartBattle(BattleTrigger.Sand);
        }
    }

    public bool TriggerRepeatedly => true;
}
