using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyableObject : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.Instance.ShowDialogText("This thing looks like it can be destroyed");

        var pokemonWithCut = initiator.GetComponent<PockiParty>().Pockies.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Punch"));

        if(pokemonWithCut != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Should {pokemonWithCut.Base.Name} use punch?",
                choices: new List<string>() {"Yes", "No"},
                onChoiceSelected: (selection) => selectedChoice = selection);

            if(selectedChoice == 0)
            {
                //Yes
                yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name} use punch!");
                gameObject.SetActive(false);
            }

        }
    }
}
