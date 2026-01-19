using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Farm))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FarmUnlockPatch
{
    [HarmonyPatch(nameof(Farm.UnlockOrUpgrade), typeof(UnlockSO), typeof(bool))]
    [HarmonyPostfix]
    public static void UnlockOrUpgrade(UnlockSO unlockSO, bool requireParent, ref bool __result)
    {
        if(!Plugin.Instance.Enabled) return;
        if (!__result) return;
        // Something was unlocked or upgraded
        Plugin.Log.LogInfo($"Unlocked or upgraded: [UnlockName: {unlockSO.unlockName}, Description: {unlockSO.description}, Parent: {unlockSO.parentUnlock}]");
        Plugin.Log.LogInfo($"RequireParent: {requireParent}");
    }

    public static void AddUnlock()
    {
        
        // MainSim.Inst.storedSim.farm.GetUnlockCost()
    }
}
