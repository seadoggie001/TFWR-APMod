using System.Diagnostics.CodeAnalysis;
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
        if (StatisticsGUI.Instance is null) return !Plugin.HarmonySkipFunction;
        // If the current mouse position overlaps the StatisticsGUI
        if (StatisticsGUI.Instance.RootElement.worldBound.Overlaps(new Rect((Vector2)Input.mousePosition, new Vector2(1,1))))
        {
            // Skip the normal function. Our GUI handles the scroll
            return Plugin.HarmonySkipFunction;
        }
        // Continue as normal
        return !Plugin.HarmonySkipFunction;
    }
}