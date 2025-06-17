using UnityEngine;
using UnityEngine.UI;

public class BaitSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText;

    public void SetData(BaitSlot baitSlot)
    {
        if (baitSlot == null || baitSlot.bait == null)
        {
            iconImage.enabled = false;
            countText.text = "";
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = baitSlot.bait.Icon;
        countText.text = $"x{baitSlot.count}";
    }
}
