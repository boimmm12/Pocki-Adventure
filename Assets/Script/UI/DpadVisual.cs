using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DpadVisual : MonoBehaviour
{
    [Header("UI Components")]
    public Image dpadImage;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    public void OnUpPressed() => dpadImage.sprite = upSprite;
    public void OnDownPressed() => dpadImage.sprite = downSprite;
    public void OnLeftPressed() => dpadImage.sprite = leftSprite;
    public void OnRightPressed() => dpadImage.sprite = rightSprite;

    public void OnReleased() => dpadImage.sprite = idleSprite;
}
