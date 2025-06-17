using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Merchant : MonoBehaviour
{
    [SerializeField] bool isFishVendor;

    [SerializeField] List<ItemBase> availableItems;
    public IEnumerator Trade()
    {
        ShopMenuState.i.IsFishVendor = isFishVendor;

        ShopMenuState.i.AvailableItems = availableItems
            .OrderBy(item => GetDetailedCategoryOrder(item))
            .ThenBy(item => item.Name)
            .ToList();

        yield return GameController.Instance.StateMachine.PushAndWait(ShopMenuState.i);
    }

    private int GetDetailedCategoryOrder(ItemBase item)
    {
        int baseCategory = (int)GetCategoryIndex(item);

        if (item is RecoveryItem recovery)
        {
            if (recovery.restoreMaxHP || recovery.hpAmount > 0)
                return baseCategory * 100 + 0;
            if (recovery.restoreMaxPP || recovery.ppAmount > 0)
                return baseCategory * 100 + 1;
            if (recovery.revive || recovery.maxRevive)
                return baseCategory * 100 + 2;
            if (recovery.recoverAllStatus || recovery.status != ConditionID.none)
                return baseCategory * 100 + 3;
            return baseCategory * 100 + 4;
        }

        if (item is CaptureBall ball)
        {
            return baseCategory * 100 + Mathf.RoundToInt(ball.catchRateModifier * 100);
        }

        if (item is BaitItem bait)
        {
            return baseCategory * 100 + bait.Price;
        }
        return baseCategory * 100 + 99;
    }

    ItemCategory GetCategoryIndex(ItemBase item)
    {
        if (item is RecoveryItem || item is XpScrollItem || item is EvolutionItem)
            return ItemCategory.Items;
        else if (item is CaptureBall)
            return ItemCategory.CaptureBalls;
        else if (item is TmItem)
            return ItemCategory.Tms;
        else
            return ItemCategory.QuestItems;
    }
}
