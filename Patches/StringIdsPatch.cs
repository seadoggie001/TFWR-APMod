using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(StringIds))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class StringIdsPatch
{
    public const string ArchipelagoItemName = "Archipelago Item";
    public static int ArchipelagoItem { get; private set; }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StringIds.SetItemIds), typeof(IEnumerable<string>))]
    public static void SetItemIds(IEnumerable<string> items, Dictionary<string, int> ___itemIds, ref string[] ___itemNames)
    {
        // If my name is already there, quit
        if(___itemIds.ContainsKey(ArchipelagoItemName)) return;
        
        // Find the next ID
        int index = items.Count();
        // Save the ID
        ___itemIds[ArchipelagoItemName] = index;
        // Save the name
        ___itemNames = ___itemNames.AddItem(ArchipelagoItemName).ToArray();
        // Steal the ID
        ArchipelagoItem = index;
    }
}