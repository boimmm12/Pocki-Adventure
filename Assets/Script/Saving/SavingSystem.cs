using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SaveSlot
{
    Auto,
    Manual1,
    Manual2
}

[Serializable]
public class SaveMetadata
{
    public string sceneName;
    public string characterPresetName;
    public int presetIndex;
    public string saveTimeString;
    public string playerName;
}

public class SavingSystem : MonoBehaviour
{
    public static SavingSystem i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    Dictionary<string, object> gameState = new Dictionary<string, object>();

    public void CaptureEntityStates(List<SavableEntity> savableEntities)
    {
        foreach (SavableEntity savable in savableEntities)
        {
            gameState[savable.UniqueId] = savable.CaptureState();
        }
    }

    public void RestoreEntityStates(List<SavableEntity> savableEntities)
    {
        foreach (SavableEntity savable in savableEntities)
        {
            string id = savable.UniqueId;
            if (gameState.ContainsKey(id))
                savable.RestoreState(gameState[id]);
        }
    }

    public void Save(string saveFileName, SaveSlot slot)
    {
        CaptureState(gameState);
        SaveFile(saveFileName, gameState, slot);
    }

    public void Load(string saveFileName, SaveSlot slot)
    {
        gameState = LoadFile(saveFileName, slot);
        RestoreState(gameState);
    }

    public void Delete(string saveFileName, SaveSlot slot)
    {
        File.Delete(GetPath(saveFileName, slot));
    }

    public bool CheckIfSaveExists(string saveFileName, SaveSlot slot)
    {
        return File.Exists(GetPath(saveFileName, slot));
    }

    private void CaptureState(Dictionary<string, object> state)
    {
        foreach (SavableEntity savable in FindObjectsOfType<SavableEntity>())
        {
            state[savable.UniqueId] = savable.CaptureState();
        }

        var player = PlayerController.i;
        if (player != null)
        {
            var preset = GlobalCharacterPresets.i.GetPreset(player.PresetIndex);
            var currentAreaName = GameController.Instance?.CurrentScene?.gameObject?.name ?? "Unknown";

            var metadata = new SaveMetadata()
            {
                sceneName = currentAreaName,
                characterPresetName = preset?.name,
                presetIndex = player.PresetIndex,
                saveTimeString = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                playerName = player.Name
            };

            state["_meta"] = metadata;
            Debug.Log($"[SavingSystem] Metadata: Scene = {metadata.sceneName}, PresetIndex = {metadata.presetIndex}");

        }
    }

    private void RestoreState(Dictionary<string, object> state)
    {
        foreach (SavableEntity savable in FindObjectsOfType<SavableEntity>())
        {
            string id = savable.UniqueId;
            if (state.ContainsKey(id))
                savable.RestoreState(state[id]);
        }
    }

    public void RestoreEntity(SavableEntity entity)
    {
        if (gameState.ContainsKey(entity.UniqueId))
            entity.RestoreState(gameState[entity.UniqueId]);
    }

    void SaveFile(string saveFileName, Dictionary<string, object> state, SaveSlot slot)
    {
        string path = GetPath(saveFileName, slot);
        Debug.Log($"Saving to: {path}");

        using (FileStream fs = File.Open(path, FileMode.Create))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fs, state);
        }
    }

    Dictionary<string, object> LoadFile(string saveFileName, SaveSlot slot)
    {
        string path = GetPath(saveFileName, slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file found at: {path}");
            return new Dictionary<string, object>();
        }

        using (FileStream fs = File.Open(path, FileMode.Open))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            return (Dictionary<string, object>)binaryFormatter.Deserialize(fs);
        }
    }

    public string GetPath(string saveFileName, SaveSlot slot)
    {
        return Path.Combine(Application.persistentDataPath, $"{saveFileName}.sav");
    }

    // ✅ Fungsi yang kamu butuhkan untuk melihat preview save
    public SaveMetadata GetMetadata(string saveFileName, SaveSlot slot)
    {
        var path = GetPath(saveFileName, slot);
        if (!File.Exists(path)) return null;

        using (FileStream fs = File.Open(path, FileMode.Open))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            var state = (Dictionary<string, object>)binaryFormatter.Deserialize(fs);

            if (state != null && state.ContainsKey("_meta"))
            {
                var meta = state["_meta"] as SaveMetadata;
                Debug.Log($"[SavingSystem] Metadata: Scene = {meta?.sceneName}, PresetIndex = {meta?.presetIndex}");
                return meta;
            }
        }
        return null;
    }

}
