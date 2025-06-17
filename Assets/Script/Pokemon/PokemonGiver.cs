using System.Collections;
using UnityEngine;


public class PokemonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Pocki pokemonToGive;
    [SerializeField] Dialog dialog;

    bool used = false;
    public IEnumerator GivePokemon(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        pokemonToGive.Init();
        player.GetComponent<PockiParty>().AddPocki(pokemonToGive);

        used = true;

        AudioManager.i.PlaySfx(AudioId.PockiObtained, pauseMusic: true);

        string dialogText = $"{player.Name} received {pokemonToGive.Base.Name}";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return pokemonToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}