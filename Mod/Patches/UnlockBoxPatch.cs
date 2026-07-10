using System.Diagnostics.CodeAnalysis;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(UnlockBox))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony forces the use of some variable names")]
public class UnlockBoxPatch
{
    /// <summary>
    /// This function is used to make unlocks cost an AP item and display it in the background
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="___image"></param>
    /// <param name="___codeText"></param>
    /// <param name="___currentCost"></param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnlockBox.SetupRec),
        [typeof(bool), typeof(HashSet<string>), typeof(ItemBlock), typeof(Dictionary<string, int>), typeof(bool)],
        [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out]
    )]
    public static void SetupRec(UnlockBox __instance, ref Image ___image, ref TextMeshProUGUI ___codeText, ref ItemBlock ___currentCost)
    {
        try
        {
            if (!Plugin.Instance.Enabled) return;
            ___image.gameObject.SetActive(true);
            ___image.sprite = Resources.Archipelago;
            ___image.color = new Color(1f, 1f, 1f, 0.5f);
            ___codeText.text = __instance.unlockSO.unlockName;
            ___codeText.color = new Color(1f, 1f, 1f, 1f);
            ___codeText.alignment = TextAlignmentOptions.Baseline;
            ___codeText.fontSize = 24f;
            ___codeText.verticalAlignment = VerticalAlignmentOptions.Baseline;
            ___currentCost = new ItemBlock(StringIdsPatch.ArchipelagoItem, 1);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(SetupRec)}", e);
        }
    }
}