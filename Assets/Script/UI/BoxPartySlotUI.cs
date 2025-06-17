using UnityEngine;
using UnityEngine.UI;

public class BoxPartySlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text lvltext;
    [SerializeField] Image image;

    public void SetData(Pocki pokemon)
    {
        nameText.text = pokemon.Base.Name;
        lvltext.text = "Lv." + pokemon.Level;
        image.sprite = pokemon.Base.FrontSprite;
        image.color = new Color(255, 255, 255, 100);
    }

    public void ClearData()
    {
        nameText.text = "";
        lvltext.text = "";
        image.sprite = null;
        image.color = new Color(255, 255, 255, 0);
    }
}
