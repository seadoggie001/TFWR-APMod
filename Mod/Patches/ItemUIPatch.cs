using System.Diagnostics.CodeAnalysis;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using UnityEngine.UI;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(ItemUI))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ItemUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemUI.Setup), typeof(int), typeof(double))]
    public static void Setup(ref Image ___image, int itemId, double c)
    {
        try
        {
            if (itemId == StringIdsPatch.ArchipelagoItem) ___image.sprite = Resources.Archipelago;
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(Setup)}", e);
        }
    }
}