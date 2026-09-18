using System;
using com.seadoggie.TFWRArchipelago.Components;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Menu))]
public class MenuPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Menu.Play))]
    public static void Play()
    {
        try
        {
            GameManager.Instance?.GameService.RaiseMenuOpen(false);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(Play)}", e);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Menu.Open))]
    public static void Open()
    {
        try
        {
            GameManager.Instance?.GameService.RaiseMenuOpen(true);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(Open)}", e);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Menu.LoadSave))]
    public static void LoadSave()
    {
        try
        {
            GameManager.Instance?.GameService.RaiseMenuOpen(false);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(LoadSave)}", e);
        }
    }
}