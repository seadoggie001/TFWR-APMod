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
        GameManager.Instance?.GameService.RaiseMenuOpen(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Menu.Open))]
    public static void Open()
    {
        GameManager.Instance?.GameService.RaiseMenuOpen(true);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Menu.LoadSave))]
    public static void LoadSave()
    {
        GameManager.Instance?.GameService.RaiseMenuOpen(false);
    }
}