using UnityEngine;

public class GameAutoSaver : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        SavingSystem.i.Save("autosave", SaveSlot.Auto);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_ANDROID
        if (pauseStatus)
        {
            SavingSystem.i.Save("autosave", SaveSlot.Auto);
        }
#endif
    }
}
