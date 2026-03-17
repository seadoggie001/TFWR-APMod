using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(ItemUI))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ItemUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ItemUI.Setup), typeof(int), typeof(double))]
    public static void Setup(ref Image ___image, int itemId, double c)
    {
        if (itemId == StringIdsPatch.ArchipelagoItem) ___image.sprite = Resources.Archipelago;
    }
}