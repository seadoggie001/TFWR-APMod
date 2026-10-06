using System;
using com.seadoggie.TFWRArchipelago.Patches;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Utils;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Items;

// Do NOT add an ItemAttribute to this class. Unlocks are manually registered in the ItemService 
public class UnlockItem : BaseItem, IInjectable
{
    protected override string ItemName => "UnlockItem";
    public string ItemNameToUnlock { get; set; }
    private bool _ready;
    [Log] private readonly ILogger<UnlockItem> _log = null!;

    public void SetUnlock(string item)
    {
        ItemNameToUnlock = item;
        if (_log != null) _ready = true;
    }

    public void OnInject() => _ready = !string.IsNullOrWhiteSpace(ItemNameToUnlock);

    public void Update()
    {
        if (!_ready) return;
        if (Unlocked()) Completed();
    }

    private bool Unlocked()
    {
        try
        {
            _log.LogInformation("Attempting to unlock {itemName}", ItemNameToUnlock);
            string unlockName = Unlocks.ItemToUnlock(ItemNameToUnlock);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                _log.LogError("Failed to find unlock item: {ItemName}", ItemNameToUnlock);
                return true;
            }

            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                _log.LogError("Failed to find Farm.");
                return false;
            }

            int count = farm.NumUnlocked(unlockName);
            _log.LogInformation("Found {Count} unlocked {UnlockName}", count, unlockName);

            // Hopefully we do not allow for "too many" items... but I think the game handles that internally
            farm.Unlock(unlockName, count + 1);
            UnlockSO unlock = farm.GetUnlockOf(unlockName);
            // foreach (string unlockItemName in unlock.unlocks) Log.LogInfo("  - and unlocks " + unlockItemName);

            farm.UnlockAllIn(unlock);
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to unlock item");
            return false;
        }
    }
}