using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(CodeWindow))]
public static class CodeWindowPatch
{
    [HarmonyPatch(nameof(CodeWindow.PromptDelete))]
    [HarmonyPrefix]
    public static bool PromptDelete(CodeWindow __instance)
    {
        if (__instance.isExecuting) return BepInExHelper.HarmonySkipFunction;
        if (__instance.CodeInput.text != string.Empty) return !BepInExHelper.HarmonySkipFunction;
        // There's no code, just delete it
        __instance.GetComponent<Window>().Close();
        return BepInExHelper.HarmonySkipFunction;
    }
}