using com.seadoggie.TFWRArchipelago.Configuration;
using HarmonyLib;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Input))]
public class InputPatch
{
    /// <summary>
    /// This prevents mouse clicks when the IMGUI connection screen is visible
    /// </summary>
    /// <param name="__result"></param>
    /// <returns></returns>
    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown))]
    [HarmonyPrefix]
    public static bool GetMouseButtonDown(ref bool __result) {
        // If the connection screen is not visible or the mouse is outside the window, process normally
        if (!(ArchipelagoSettingsGUI.Instance?.DisplayingWindow ?? false) || !ArchipelagoSettingsGUI.Instance.IsMouseOverWindow()) return !Plugin.HarmonySkipFunction;
        // the mouse click was handled. Skip the rest of the function.
        __result = false;
        return Plugin.HarmonySkipFunction;
    }
}