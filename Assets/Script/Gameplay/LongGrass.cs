using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public void onPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.isMoving = false;
            GameController.Instance.StartBattle(BattleTrigger.LongGrass);
        }
    }

    public bool TriggerRepeatedly => true;
}
