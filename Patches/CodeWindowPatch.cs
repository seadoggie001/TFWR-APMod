using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(CodeWindow))]
public static class CodeWindowPatch
{
    [HarmonyPatch(nameof(CodeWindow.PromptDelete))]
    [HarmonyPrefix]
    public static bool PromptDelete(CodeWindow __instance)
    {
        if (__instance.isExecuting) return Plugin.HarmonySkipFunction;
        if (__instance.CodeInput.text != string.Empty) return !Plugin.HarmonySkipFunction;
        // There's no code, just delete it
        __instance.GetComponent<Window>().Close();
        return Plugin.HarmonySkipFunction;
    }
}