using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Farm))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FarmPatch
{
    /// <summary>
    /// Used to patch the starting unlocks on the farm and the unlocks on startup 
    /// </summary>
    /// <param name="unlocks">The requested unlocks</param>
    [HarmonyPatch(MethodType.Constructor, typeof(Simulation), typeof(IEnumerable<string>), typeof(ItemBlock),
        typeof(List<SFO>), typeof(List<SFO>), typeof(bool))]
    [HarmonyPrefix]
    public static void Constructor_Prefix(ref IEnumerable<string> unlocks)
    {
        if(!Plugin.Instance.Enabled) return;
        // Get the startUnlocks field via reflection
        FieldInfo startUnlocksField = typeof(Farm).GetField("startUnlocks", 
            BindingFlags.Public | BindingFlags.Static);
        List<string> startUnlocks = (List<string>)startUnlocksField?.GetValue(null);
        if (startUnlocks is null) return;
        
        // Combine the startUnlocks with the unlocks param
        List<string> distinctUnlocks = startUnlocks.ToList();
        distinctUnlocks.AddRange(unlocks);
        distinctUnlocks = distinctUnlocks.Distinct().ToList();
        
        startUnlocks = [];
        unlocks = [];
        
        // ToDo: Check if Archipelago thinks the upgrades are valid
        foreach (string unlock in distinctUnlocks)
        {
            // Plugin.Log.LogInfo("Requested unlock: " + unlock);
            startUnlocks.Add(unlock);
        }
        startUnlocksField.SetValue(null, startUnlocks);
    }

    /// <summary>
    /// Used to make everything cost an Archipelago Item
    /// </summary>
    /// <param name="unlockSO"></param>
    /// <param name="numUnlocked"></param>
    /// <param name="__result"></param>
    /// <returns></returns>
    [HarmonyPatch(nameof(Farm.GetUnlockCost), typeof(UnlockSO), typeof(int))]
    [HarmonyPrefix]
    public static bool GetUnlockCost(UnlockSO unlockSO, int numUnlocked, ref ItemBlock __result)
    {
        if (!Plugin.Instance.Enabled) return !BepInExHelper.HarmonySkipFunction;
        __result = new ItemBlock(StringIdsPatch.ArchipelagoItem, 1);
        return BepInExHelper.HarmonySkipFunction;
    }
    
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
}