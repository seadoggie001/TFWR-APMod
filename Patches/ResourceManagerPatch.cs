using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(ResourceManager))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony forces the use of some variable names")]
public class ResourceManagerPatch
{
    public const string ArchipelagoOptionToggle = "Archipelago";
    public const string ArchipelagoOptionToggleValue = "Click to Open";
    public static string[] CustomOptions = [];

    private static CycleOptionSO _openArchipelagoOption;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResourceManager.LoadAll))]
    public static void LoadAll(ref ItemSO[] ___items)
    {
        // Install the Archipelago item
        ItemSO item = ScriptableObject.CreateInstance<ItemSO>();
        item.itemId = StringIdsPatch.ArchipelagoItem;
        item.itemName = StringIdsPatch.ArchipelagoItemName;
        // The rest of this is all nonsense?
        item.description = "Archipelago Stuff";
        item.docs = "I got some docs!?";
        item.enabled = true;
        item.name = "Some other Archipelago name?";
        item.trackStats = true;
        // Now tell the game about it
        ___items = ___items.AddItem(item).ToArray();

        // Track stats for all items
        foreach (ItemSO itemSo in ___items)
        {
            itemSo.trackStats = true;
        }
    }

    /// <summary>
    /// Load custom options
    /// </summary>
    /// <param name="__result"></param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResourceManager.GetAllOptions))]
    // ReSharper disable once InconsistentNaming
    public static void GetAllOptions(ref IEnumerable<OptionSO> __result)
    {
        if (!Plugin.Instance.Loaded) return;
        List<OptionSO> options = __result.ToList();
        _openArchipelagoOption ??= AddOption(ArchipelagoOptionToggle, "Open Archipelago settings", "general", 0f,
            [ArchipelagoOptionToggleValue, "Closed"], ArchipelagoOptionToggleValue);
        options.Add(_openArchipelagoOption);
        options.Add(AddOption("DEBUG", "Cause debugging is hard", "general", 0f, ["Send location", "done"],
            "Send location"));
        CustomOptions = [ArchipelagoOptionToggle, "DEBUG"];
        __result = options;
    }

    /// <summary>
    /// Build a custom CycleOptionSO
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tooltip"></param>
    /// <param name="category"></param>
    /// <param name="importance"></param>
    /// <param name="options"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static CycleOptionSO AddOption(string name, string tooltip, string category, float importance,
        List<string> options, string defaultValue)
    {
        CycleOptionSO option = ScriptableObject.CreateInstance<CycleOptionSO>();
        option.name = name;
        option.optionName = name;
        option.tooltip = tooltip;
        option.category = category;
        option.importance = importance;
        option.options = options;
        option.defaultValue = defaultValue;
        // ReSharper disable once Unity.UnknownResource -- This will load from TFWR's resources
        OptionSO[] existingOptions = UnityEngine.Resources.LoadAll<OptionSO>("Options/");
        OptionSO cycleOption = existingOptions.FirstOrDefault(m => m is CycleOptionSO);
        if (cycleOption != null && cycleOption.optionUI != null) option.optionUI = cycleOption.optionUI;
        if (OptionHolder.GetOption(name) == null) OptionHolder.SetOption(name, defaultValue);
        return option;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResourceManager.GetUnlock))]
    public static void GetUnlock(string name, ref UnlockSO __result)
    {
        if (Plugin.Instance.Enabled && __result is not null && __result)
        {
            // Everything costs an Archipelago item
            __result.unlockCost = new ItemBlock(StringIdsPatch.ArchipelagoItem, 1);
            // ToDo: Someday, check if there's a hint for this item. If there is a hint, display it here...
            // __result.description = "Text describing a hint for this item";
        }
    }
}