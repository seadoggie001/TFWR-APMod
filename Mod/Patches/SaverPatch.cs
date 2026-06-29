using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Saver))]
public static class SaverPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Saver.Load), typeof(MainSim))]
    public static void Load()
    {
        try
        {
            GameManager.Instance?.Load();
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(Load)}", e);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Saver.SaveProgress), typeof(MainSim))]
    public static void SaveProgress()
    {
        try
        {
            if(!Plugin.Instance.Enabled) return;
            GameManager.Instance?.SaveProgress();
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(SaveProgress)}", e);
        }
    }
}