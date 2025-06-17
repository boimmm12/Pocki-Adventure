using UnityEngine;

public interface IPlayerTriggerable
{
    void onPlayerTriggered(PlayerController player);

    bool TriggerRepeatedly {get;}
}
