using System;
using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionState : State<GameController>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pockiImage;

    [SerializeField] AudioClip evoMusic;

    public static EvolutionState i { get; private set; }

    private void Awake()
    {
        i = this;
    }
    public IEnumerator Evolve(Pocki pokemon, Evolution evolution)
    {
        var gc = GameController.Instance;
        gc.StateMachine.Push(this);

        evolutionUI.SetActive(true);

        AudioManager.i.PlayMusic(evoMusic);

        pockiImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is evolving");

        var oldPokemon = pokemon.Base;
        pokemon.Evolve(evolution);

        pockiImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name} evolved into {pokemon.Base.Name}");

        evolutionUI.SetActive(false);
        
        gc.PartyScreen.SetPartyData();
        AudioManager.i.PlayMusic(gc.CurrentScene.SceneMusic, fade: true);

        gc.StateMachine.Pop();
    }
}
