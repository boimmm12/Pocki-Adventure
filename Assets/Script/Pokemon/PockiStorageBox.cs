using System.Collections.Generic;
using UnityEngine;

public class PockiStorageBox : MonoBehaviour, ISavable
{
    const int numberOfBoxes = 16;
    const int numberOfSlot = 16;

    public int NumOfBoxes => numberOfBoxes;
    public int NumOfSlots => numberOfSlot;
    Pocki[,] boxes = new Pocki[numberOfBoxes, numberOfSlot];

    public void AddPokemon(Pocki pokemon, int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = pokemon;
    }

    public void RemovePokemon(int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = null;
    }

    public Pocki GetPokemon(int boxIndex, int slotIndex)
    {
        return boxes[boxIndex, slotIndex];
    }

    public void AddPockiToEmptySlot(Pocki pokemon)
    {
        for (int boxIndex = 0; boxIndex < numberOfBoxes; boxIndex++)
        {
            for (int slotIndex = 0; slotIndex < numberOfSlot; slotIndex++)
            {
                if (boxes[boxIndex, slotIndex] == null)
                {
                    boxes[boxIndex, slotIndex] = pokemon;
                    return;
                }
            }
        }
    }

    public static PockiStorageBox GetPlayerStorageBox()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PockiStorageBox>();
    }

    public object CaptureState()
    {
        var saveData = new BoxSaveData()
        {
            boxSlots = new List<BoxSlotSaveData>()
        };

        for (int boxIndex = 0; boxIndex < numberOfBoxes; boxIndex++)
        {
            for (int slotIndex = 0; slotIndex < numberOfSlot; slotIndex++)
            {
                if (boxes[boxIndex, slotIndex] != null)
                {
                    var boxSlot = new BoxSlotSaveData()
                    {
                        pockiData = boxes[boxIndex, slotIndex].GetSaveData(),
                        boxIndex = boxIndex,
                        slotIndex = slotIndex
                    };

                    saveData.boxSlots.Add(boxSlot);
                }
            }
        }
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as BoxSaveData;

        for (int boxIndex = 0; boxIndex < numberOfBoxes; boxIndex++)
        {
            for (int slotIndex = 0; slotIndex < numberOfSlot; slotIndex++)
            {
                boxes[boxIndex,slotIndex] = null;
            }
        }

        foreach (var slot in saveData.boxSlots)
        {
            boxes[slot.boxIndex, slot.slotIndex] = new Pocki(slot.pockiData);
        }
    }
}
[System.Serializable]
public class BoxSaveData
{
    public List<BoxSlotSaveData> boxSlots;
}
[System.Serializable]
public class BoxSlotSaveData
{
    public PockiSaveData pockiData;
    public int boxIndex;
    public int slotIndex;
}
