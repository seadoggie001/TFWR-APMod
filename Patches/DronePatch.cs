using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
using HarmonyLib;

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
}