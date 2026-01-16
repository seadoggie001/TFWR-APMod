using HarmonyLib;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(ResourceManager))]
public class ResourceManagerPatch
{
    public const string Name = "Archipelago";
    public const string DefaultValue = "Click to Open";
    private static readonly string[] Options = ["Click to Open", "Closed"];
    
    private static CycleOptionSO _openArchipelagoOption = null;
    
    /// <summary>
    /// Load custom options
    /// </summary>
    /// <param name="__result"></param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ResourceManager.GetAllOptions))]
    // ReSharper disable once InconsistentNaming
    public static void GetAllOptions(ref IEnumerable<OptionSO> __result)
    {
        _openArchipelagoOption ??= AddOption(Name, "Open Archipelago settings", "general", 0f, Options.ToList(), DefaultValue);
        List<OptionSO> options = __result.ToList();
        Plugin.Log.LogInfo(string.Join(", ", options.Select(m => m.name)));
        options.Add(_openArchipelagoOption);
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
        OptionSO[] existingOptions = Resources.LoadAll<OptionSO>("Options/");
        OptionSO cycleOption = existingOptions.FirstOrDefault(m => m is CycleOptionSO);
        if(cycleOption != null && cycleOption.optionUI != null) option.optionUI = cycleOption.optionUI;
        if(OptionHolder.GetOption(name, null) == null) OptionHolder.SetOption(name, defaultValue);
        return option;
    }
}