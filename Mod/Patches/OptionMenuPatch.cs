using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;

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
        try
        {
            if (!(GameManager.Instance?.TfwrConfig?.Debug ?? false) ||
                !ResourceManagerPatch.CustomOptions.Contains(optionName))
                return;

            Plugin.Log.LogInfo("CustomOption Changed. Name: " + optionName);
            // Don't cause an infinite loop because we're setting the value in the "listener"
            // Note: a locking object fails here... perhaps it's related to BepinEx or Harmony?
            if (_settingValue) return;
            _settingValue = true;
            switch (optionName)
            {
                case ResourceManagerPatch.ArchipelagoOptionToggle:
                    try
                    {
                        UIManager.Instance?.OpenConnectionSettings();
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogException("Failed to create the connection settings modifier GUI", e);
                    }

                    OptionHolder.SetOption(ResourceManagerPatch.ArchipelagoOptionToggle,
                        ResourceManagerPatch.ArchipelagoOptionToggleValue);
                    break;
            }

            _settingValue = false;
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(OnOptionChanged)}", e);
        }
    }
}