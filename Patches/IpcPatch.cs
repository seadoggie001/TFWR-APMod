using System.Collections.Generic.Integrations;
using System.Reflection;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Ipc))]
public static class IpcPatch
{
    [HarmonyPatch(typeof(Ipc), nameof(Ipc.Start))]
    [HarmonyPrefix]
    public static bool Start()
    {
        return BepInExHelper.HarmonySkipFunction;
    }
    
    /// <summary>
    /// Try to tell the IPC to start/stop running
    /// </summary>
    /// <param name="running"></param>
    public static void SetRunning(bool running)
    {
        if(!(GameManager.Instance?.TfwrConfig.DisableIpc ?? false)) return;
        try
        {
            Type type = typeof(Ipc);
            FieldInfo fieldInfo = type.GetField("_isRunning", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo is null)
            {
                Plugin.Log.LogWarning("Failed to deactivate Ipc, the log file may be swarmed with errors on Linux");
            }
            else
            {
                Plugin.Log.LogWarning("Ipc should be disabled");
                fieldInfo.SetValue(Ipc.Instance, running);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogException("Failed to get MainSim.sim", e);
        }
    }
}