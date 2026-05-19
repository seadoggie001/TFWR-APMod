using com.seadoggie.TFWRArchipelago.Service;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

// ReSharper disable InconsistentNaming

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(SaveOption))]
public class SaveOptionPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SaveOption.Setup), typeof(string), typeof(SaveChooser))]
    public static void Setup(SaveOption __instance, string fileName)
    {
        // Check if the custom mod file exists
        string filePath = GameService.GetFilePath(fileName);
        if(!File.Exists(filePath)) return;
        
        // Create an image to show it exists
        GameObject icon = new ("AP_Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        
        Transform transform = __instance.transform.Find("PlayButton");
        icon.transform.SetParent(transform, false);
        // Move in front of Green panel
        icon.transform.SetSiblingIndex(1);
        
        // Load the image
        Image image = icon.GetComponent<Image>();
        image.sprite = Resources.Archipelago;
        image.enabled = true;
        
        // Align it to the Middle-Left positioned 10 pixels from the left
        RectTransform rect = icon.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.sizeDelta = new Vector2(40, 40);
        rect.anchoredPosition = new Vector2(10, 0);
    } 
}