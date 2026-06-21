using System.Diagnostics.CodeAnalysis;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Workspace))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony forces the use of some variable names")]
public class WorkspacePatch
{
    /// <summary>
    /// Prevent the world from zooming when hovering over the StatisticsGUI
    /// </summary>
    /// <param name="scroll"></param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Workspace.Scroll), typeof(float))]
    public static bool Scroll(float scroll)
    {
        // Ignore all of this if StatisticsGUI isn't around
        if (UIManager.Instance?.StatGuiOpen() ?? false) return !BepInExHelper.HarmonySkipFunction;
        // If the current mouse position overlaps the StatisticsGUI
        if (UIManager.Instance!.StatGuiBounds().Overlaps(new Rect(Input.mousePosition, new Vector2(1,1))))
        {
            // Skip the normal function. Our GUI handles the scroll
            return BepInExHelper.HarmonySkipFunction;
        }
        // Continue as normal
        return !BepInExHelper.HarmonySkipFunction;
    }
}