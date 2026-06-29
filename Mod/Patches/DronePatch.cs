using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Drone))]
public static class DronePatch
{
    [HarmonyPatch(nameof(Drone.PetThePiggy))]
    [HarmonyPrefix]
    public static void PetThePiggy()
    {
        if (!Plugin.Instance.Enabled) return;
        try
        {
            // Grant the achievement
            APManager.Instance?.APService?.UnlockAchievement(Achievement.PetThePiggy);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"Drone.{nameof(PetThePiggy)}", e);
        }
    }

    [HarmonyPatch(nameof(Drone.Harvest))]
    [HarmonyPrefix]
    public static void Harvest(Drone __instance)
    {
        try
        {
            FarmObject obj = __instance.EntityUnderDrone();
            if (obj?.objectSO?.dropItem != "hay") return;
            GameManager.Instance?.GameService.RaiseGrassSanity(__instance.pos);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"Drone.{nameof(Harvest)}", e);
        }
    }
}