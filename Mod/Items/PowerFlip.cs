using System;
using com.seadoggie.TFWRArchipelago.Patches;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Utils;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Items;

[Item("PowerFlip")]
public class PowerFlip : BaseItem, IInjectable
{
    private const string ItemName = "PowerFlip";

    [Log] private readonly ILogger<PowerFlip> _log = null!;

    public void OnInject()
    {
        try
        {
            _log.LogInformation("Attempting to unlock {itemName}", ItemName);
            string unlockName = Unlocks.ItemToUnlock(ItemName);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                _log.LogError("Failed to find unlock item: {ItemName}", ItemName);
                return;
            }

            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                _log.LogError("Failed to find Farm.");
                return;
            }

            int count = farm.NumUnlocked(unlockName);
            _log.LogInformation("Found {Count} unlocked {UnlockName}", count, unlockName);

            // Hopefully we do not allow for "too many" items... but I think the game handles that internally
            farm.Unlock(unlockName, count + 1);
            UnlockSO unlock = farm.GetUnlockOf(unlockName);
            // foreach (string unlockItemName in unlock.unlocks) Log.LogInfo("  - and unlocks " + unlockItemName);

            farm.UnlockAllIn(unlock);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to unlock item");
        }
    }
}