using System.Collections.Generic;
using UnityEngine;

public class GlobalCharacterPresets : MonoBehaviour
{
    public static GlobalCharacterPresets i;

    [SerializeField] public List<CharacterPreset> presets;

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);
    }

    public CharacterPreset GetPreset(int index)
    {
        if (index >= 0 && index < presets.Count)
            return presets[index];

        Debug.LogWarning($"[GlobalCharacterPresets] Invalid preset index: {index}");
        return null;
    }
}

