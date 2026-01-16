using System.Reflection;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Farm))]
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
        
        // ToDo: Compare the startUnlocks to the unlocks param to determine if it's a new game?
        // No, you likely will create a new game then connect to the server...
        // I might-could compare the list of unlocked items against the list of "vanilla" unlock items though?
        // I really don't remember why I needed all of this though... just use the vanilla unlocks and call it a day?
        
        // Combine the startUnlocks with the unlocks param
        List<string> distinctUnlocks = startUnlocks.ToList();
        distinctUnlocks.AddRange(unlocks);
        distinctUnlocks = distinctUnlocks.Distinct().ToList();
        
        startUnlocks = [];
        unlocks = [];
        
        // ToDo: Check if Archipelago thinks the upgrades are valid
        foreach (string unlock in distinctUnlocks)
        {
            Plugin.Log.LogInfo("Requested unlock: " + unlock);
            // startUnlocks.Add(unlock);
        }
        startUnlocksField.SetValue(null, startUnlocks);
    }
    
    // /// <summary>
    // /// Can use this to override more the of the unlocking of things in the farm
    // /// </summary>
    // /// <param name="__instance"></param>
    // /// <param name="s"></param>
    // /// <param name="__result"></param>
    // /// <returns></returns>
    // [HarmonyPatch(nameof(Farm.IsUnlocked), typeof(string))]
    // [HarmonyPrefix]
    // public static bool IsUnlocked_Prefix(Farm __instance, string s, ref bool __result)
    // {
    //     
    //     return true;
    // }
}