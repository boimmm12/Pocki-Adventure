using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text messageText;
    [SerializeField] Image pokemonSprite;

    Pocki _pokemon;

    public void Init(Pocki pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        SetMessage("");
        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
        pokemonSprite.sprite = _pokemon.Base.FrontSprite;
    }
    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = Color.blue;
        else
            nameText.color = Color.black;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
