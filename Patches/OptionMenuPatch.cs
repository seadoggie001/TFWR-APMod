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
        if (!ResourceManagerPatch.CustomOptions.Contains(optionName)) return;
        Plugin.Log.LogInfo("CustomOption Changed. Name: " + optionName);
        // Don't cause an infinite loop because we're setting the value in the "listener"
        // Note: a locking object fails here... perhaps it's related to BepinEx or Harmony?
        if (_settingValue) return;
        _settingValue = true;
        switch (optionName)
        {
            case "DEBUG":
                Plugin.Instance.LocationHelper.SubmitAchievement("PET_THE_PIGGY");
                break;
            case ResourceManagerPatch.ArchipelagoOptionToggle:
                try
                {
                    Plugin.Log.LogInfo("Opening Archipelago statistics gui");
                    StatisticsGUI.Show();
                    ArchipelagoSettingsGUI.Show();
                }
                catch (Exception e)
                {
                    Plugin.LogError("Failed to create the connection settings modifier GUI", e);
                }
                OptionHolder.SetOption(ResourceManagerPatch.ArchipelagoOptionToggle, ResourceManagerPatch.ArchipelagoOptionToggleValue);
                break;
        }
        _settingValue = false;
    }
}