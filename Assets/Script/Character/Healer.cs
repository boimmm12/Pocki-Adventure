using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [SerializeField] Dialog dialogYes;
    [SerializeField] Dialog dialogNo;
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("damn",
        choices : new List<string>() { "Yes", "No"},
        onChoiceSelected :   (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Yes
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<PockiParty>();
            playerParty.Pockies.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.Instance.ShowDialog(dialogYes);
        }
        else if (selectedChoice == 1)
        {
            //No
            yield return DialogManager.Instance.ShowDialog(dialogNo);
        }
    }
}