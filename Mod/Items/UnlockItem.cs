using System;
using com.seadoggie.TFWRArchipelago.Patches;
using com.seadoggie.TFWRArchipelago.Utils;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Items;

// Do NOT add an ItemAttribute to this class. Unlocks are manually registered in the ItemService 
public class UnlockItem : BaseItem
{
    protected override string ItemName => "UnlockItem";
    public string ItemNameToUnlock { get; set; }

    public void SetUnlock(string item) => ItemNameToUnlock = item;

    public void Update()
    {
        if (string.IsNullOrEmpty(ItemNameToUnlock)) return;
        if(Unlocked()) Completed();
    }

    private bool Unlocked()
    {
        try
        {

            string unlockName = Unlocks.ItemToUnlock(ItemNameToUnlock);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                Log.LogError($"Failed to find unlock item: {ItemNameToUnlock}");
                return true;
            }

            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                Log.LogError("Failed to find Farm.");
                return false;
            }

            int count = farm.NumUnlocked(unlockName);
            Log.LogInfo($"Found {count} unlocked {unlockName}");

            // Hopefully we do not allow for "too many" items... but I think the game handles that internally
            farm.Unlock(unlockName, count + 1);
            UnlockSO unlock = farm.GetUnlockOf(unlockName);
            // foreach (string unlockItemName in unlock.unlocks) Log.LogInfo("  - and unlocks " + unlockItemName);

            farm.UnlockAllIn(unlock);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogException("Failed to unlock item", ex);
            return false;
        }
    }
}