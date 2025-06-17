using UnityEngine;
using UnityEngine.UI;

public class FishSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text weightText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text rarityText;

    public void SetData(CaughtFishData data)
    {
        Debug.Log("FishSlotUI.SetData dipanggil untuk: " + data.FishItem.FishName);

        if (data == null || data.FishItem == null) return;

        iconImage.sprite = data.FishItem.Icon;
        weightText.text = $"{data.weight:0.0} KG";
        nameText.text = data.FishItem.FishName;
        rarityText.text = "" + data.FishItem.rarity;
    }
    public void ClearData()
    {
        iconImage.sprite = null;
        weightText.text = "";
        nameText.text = "";
        rarityText.text = "";
    }

}
