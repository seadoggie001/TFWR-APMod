using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

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
    public static bool GetMouseButtonDown(ref bool __result)
    {
        try
        {
            // If the mouse is outside all windows, process normally
            if (!UIManager.Instance?.MouseOverAnyWindow() ?? true) return !BepInExHelper.HarmonySkipFunction;
            // the mouse click was handled. Skip the rest of the function.
            __result = false;
            return BepInExHelper.HarmonySkipFunction;
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(GetMouseButtonDown)}", e);
            return !BepInExHelper.HarmonySkipFunction;
        }
    }
}