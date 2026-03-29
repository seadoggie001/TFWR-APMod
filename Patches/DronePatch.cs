using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Drone))]
public static class DronePatch
{
    [HarmonyPatch(nameof(Drone.PetThePiggy))]
    [HarmonyPrefix]
    public static void PetThePiggy(Hat ___hat)
    {
        if (!Plugin.Instance.Enabled) return;
        // I don't know what this line does, but it's copied from the base method
        if (___hat.hatSO.rotateDroneToMove) return;
        // Grant the achievement now without checking if the leaderboard is enabled
        Achievements.UnlockAchievement("PET_THE_PIGGY");
    }
}