using com.seadoggie.TFWRArchipelago.Components;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Saver))]
public static class SaverPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Saver.Load), typeof(MainSim))]
    public static void Load()
    {
        GameManager.Instance?.Load();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Saver.SaveProgress), typeof(MainSim))]
    public static void SaveProgress()
    {
        if(!Plugin.Instance.Enabled) return;
        GameManager.Instance?.SaveProgress();
    }
}