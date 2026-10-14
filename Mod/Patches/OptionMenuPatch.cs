using System;
using System.Linq;
using com.seadoggie.TFWRArchipelago.Components;
using HarmonyLib;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(OptionMenu))]
public class OptionMenuPatch
{
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
            if (!ResourceManagerPatch.CustomOptions.ToList().Contains(optionName)) return;

            Plugin.Log.LogInformation("CustomOption Changed. Name: {option} Value: {value}", optionName,
                OptionHolder.GetString(optionName));
            switch (optionName)
            {
                case ResourceManagerPatch.OptionNames.OpenConnectionWindow:
                    try
                    {
                        UIManager.Instance?.OpenConnectionSettings();
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogError(e, "Failed to create the connection settings modifier GUI");
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogError(e, $"{nameof(OnOptionChanged)}");
        }
    }
}