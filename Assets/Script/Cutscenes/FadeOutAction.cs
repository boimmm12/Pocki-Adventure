using System.Collections;
using UnityEngine;

public class FadeOutAction : CutsceneAction
{
    [SerializeField] float duration = 0.5f;

    public override IEnumerator Play()
    {
        yield return Fader.i.FadeOut(duration);
    }
}
