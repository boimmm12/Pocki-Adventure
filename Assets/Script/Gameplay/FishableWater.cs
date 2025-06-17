using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishableWater : MonoBehaviour, Interactable
{
    [SerializeField] private List<FishItem> possibleFish;

    public IEnumerator Interact(Transform initiator)
    {
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator == null || animator.IsFishing) yield break;

        PlayerController.i.DisableInput();

        // Tampilkan UI pemilihan umpan
        if (FishInventory.i == null)
        {
            Debug.LogError("❌ FishInventory belum siap!");
            yield break;
        }

        var allBaits = FishInventory.i.GetAllBaits();
        if (allBaits == null || allBaits.Count == 0)
        {
            Debug.LogWarning("⚠️ Tidak ada umpan tersedia!");
        }

        BaitItem selectedBait = null;
        bool selectionDone = false;
        if (BaitSelectionUI.i == null)
        {
            Debug.LogError("❌ BaitSelectionUI belum ada di scene atau belum aktif!");
            PlayerController.i.EnableInput();
            yield break;
        }

        BaitSelectionUI.i.Show(allBaits, bait =>
        {
            selectedBait = bait;
            selectionDone = true;
        });

        yield return new WaitUntil(() => selectionDone);

        float successRate = selectedBait?.successRateMultiplier ?? 0.5f;

        yield return DialogManager.Instance.ShowDialogText("You cast your line...");

        animator.IsFishing = true;
        animator.SetFishingAnimation(true);
        yield return new WaitForSeconds(2f);

        bool success = UnityEngine.Random.value < successRate;

        if (success)
        {
            // Gunakan rarity distribusi dari bait jika ada
            FishRarity rarity = selectedBait != null
            ? selectedBait.GetRandomRarity()
            : BaitItem.GetDefaultRandomRarity(); // <-- buat static default fallback


            var candidates = possibleFish.FindAll(f => f.rarity == rarity);
            if (candidates.Count == 0)
            {
                candidates = possibleFish; // fallback jika tidak ada
            }

            var fishItem = candidates[Random.Range(0, candidates.Count)];

            float weight = fishItem.GetRandomWeight();
            var caughtFish = new CaughtFishData(fishItem, weight, rarity);
            FishInventory.i.AddFish(caughtFish);

            yield return DialogManager.Instance.ShowDialogText(
                $"You caught a {fishItem.FishName}!\n{weight:0.0} KG - {rarity}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("No bites this time...");
        }

        animator.SetFishingAnimation(false);
        animator.IsFishing = false;
        PlayerController.i.EnableInput();
    }
}
