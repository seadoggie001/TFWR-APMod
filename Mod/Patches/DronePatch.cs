using System;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Drone))]
public static class DronePatch
{
    public static bool IsFrozen = false;
    public static Vector3? ModifiedSize = null;
    public static bool Hatless = false;

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

    [HarmonyPatch(nameof(Drone.Move))]
    [HarmonyPrefix]
    public static bool Move()
    {
        return !IsFrozen;
    }

    [HarmonyPatch(nameof(Drone.ChangeHat))]
    [HarmonyPrefix]
    public static bool ChangeHat()
    {
        // Skip changing hats if you're hatless
        return !Hatless;
    }
    
    [HarmonyPatch(nameof(Drone.GetTransform))]
    [HarmonyPostfix]
    public static void GetTransform(Drone __instance, ref Matrix4x4 __result)
    {
        if (ModifiedSize is null) return;
        __result = Matrix4x4.TRS(__result.GetPosition(), __result.rotation, ModifiedSize ?? Vector3.one);
    }
}