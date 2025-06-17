using System.Collections;
using UnityEngine;

[System.Serializable]
public class CutsceneAction
{
    [SerializeField] string name;
    [SerializeField] bool waitForCompletion = true;

    public virtual IEnumerator Play()
    {
        yield break;
    }
    public string Name {
        get => name;
        set => name = value;
    }

    public bool WaitForCompletion => waitForCompletion;
}
