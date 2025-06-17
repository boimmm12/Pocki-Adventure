using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PockiParty : MonoBehaviour
{
    [SerializeField] List<Pocki> pockies;

    public event Action OnUpdated;

    public List<Pocki> Pockies
    {
        get
        {
            return pockies;
        }
        set
        {
            pockies = value;
            OnUpdated?.Invoke();
        }
    }

    PockiStorageBox storageBoxes;
    private void Awake()
    {
        storageBoxes = GetComponent<PockiStorageBox>();
        foreach (var pocki in pockies)
        {
            pocki.Init();
        }
    }
    public Pocki GetHealthyPocki()
    {
        return pockies.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPocki(Pocki newPocki)
    {
        if (pockies.Count < 6)
        {
            pockies.Add(newPocki);
            OnUpdated?.Invoke();
        }
        else
        {
            storageBoxes.AddPockiToEmptySlot(newPocki);
        }
    }

    public bool CheckForEvolutions()
    {
        return pockies.Any(p => p.CheckForEvolutionBasedOnMoves(p.Level) != null || p.CheckForEvolution(p.Level) != null);
    }

    public IEnumerator RunEvolutions()
    {
        foreach (var pocki in pockies)
        {
            Debug.Log($"[EVOLUTION] Cek {pocki.Base.Name}");

            var moveTypeEvolution = pocki.CheckForEvolutionBasedOnMoves(pocki.Level);

            var evolution = moveTypeEvolution ?? pocki.CheckForEvolution(pocki.Level);

            if (evolution != null)
            {
                yield return EvolutionState.i.Evolve(pocki, evolution);
            }
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }
    public static PockiParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PockiParty>();
    }
}
