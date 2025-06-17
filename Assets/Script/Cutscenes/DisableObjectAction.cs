using System.Collections;
using UnityEngine;

public class DisableObjectAction : CutsceneAction
{
    [SerializeField] GameObject go;

    public override IEnumerator Play()
    {
        go.SetActive(false);
        yield break;
    }
}
