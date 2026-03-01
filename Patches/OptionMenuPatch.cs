using com.seadoggie.TFWRArchipelago.Configuration;
using HarmonyLib;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(OptionMenu))]
public class OptionMenuPatch
{
    private static bool _settingValue;
    /// <summary>
    /// Launch the GUI for Arch settings when a custom option is clicked
    /// </summary>
    /// <param name="optionName"></param>
    [HarmonyPrefix]
    [HarmonyPatch("OnOptionChanged", typeof(string))]
    public static void OnOptionChanged(string optionName)
    {
        // Don't cause an infinite loop because we're setting the value in the "listener"
        // Note: a locking object fails here... perhaps it's related to BepinEx or Harmony?
        if (_settingValue) return;
        _settingValue = true;
        Plugin.Log.LogInfo("OptionChanged: " + optionName);
        if (optionName != ResourceManagerPatch.Name) return;
        new GameObject().AddComponent<ArchipelagoSettings>();
        OptionHolder.SetOption(ResourceManagerPatch.Name, ResourceManagerPatch.DefaultValue);
        _settingValue = false;
    }
}