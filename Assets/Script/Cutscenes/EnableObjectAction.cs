using System.Collections;
using UnityEngine;

public class EnableObjectAction : CutsceneAction
{
    [SerializeField] GameObject go;

    public override IEnumerator Play()
    {
        go.SetActive(true);
        yield break;
    }
}
