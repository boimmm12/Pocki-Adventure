using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPreset", menuName = "Player/CharacterPreset")]
public class CharacterPreset : ScriptableObject
{
    public Sprite idleDown;  // Untuk preview icon
    public string presetname;
    public string specialNote;
    public string specialErrorBlank = "Give us your name, so we know how to call you please";
    public string specialErrorLong = "Not that long, what do you think i am ? Eminem ?";
    public List<Sprite> walkDownSprites;
    public List<Sprite> walkUpSprites;
    public List<Sprite> walkRightSprites;
    public List<Sprite> walkLeftSprites;
    public Sprite battleSprite;
}
