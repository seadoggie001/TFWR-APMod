using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine.UI;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(UnlockBox))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony forces the use of some variable names")]
public class UnlockBoxPatch
{
    /// <summary>
    /// Used to hide the text of not-yet-unlocked items. Could store hint-like information here.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnlockBox.GetTooltipInfo))]
    public static void GetTooltipInfo(UnlockBox __instance, ref TooltipInfo __result)
    {
        if(!Plugin.Instance.Enabled) return;
        // Check if the unlock name is currently allowed by archipelago
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnlockBox.SetupRec),
        [typeof(bool), typeof(HashSet<string>), typeof(ItemBlock), typeof(Dictionary<string, int>), typeof(bool)],
        [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out]
    )]
    public static void SetupRec(UnlockBox __instance, ref Image ___image)
    {
        if (__instance.unlockSO.mesh)
        {
            ___image.sprite = Resources.Archipelago;
        }
    }
}