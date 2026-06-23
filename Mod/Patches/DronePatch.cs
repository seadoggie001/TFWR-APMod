using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
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
        // Grant the achievement
        APManager.Instance?.APService?.UnlockAchievement(Achievement.PetThePiggy);
    }

    [HarmonyPatch(nameof(Drone.Harvest))]
    [HarmonyPrefix]
    public static void Harvest(Drone __instance)
    {
        FarmObject obj = __instance.EntityUnderDrone();
        if (obj?.objectSO?.dropItem != "hay") return;
        GameManager.Instance?.GameService.RaiseGrassSanity(__instance.pos);
    }
}