using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Achievements))]
public class AchievementsPatch
{
    [HarmonyPatch(nameof(Achievements.UnlockAchievement), typeof(string))]
    [HarmonyPrefix]
    public static void UnlockAchievement(string achievement)
    {
        try
        {
            if (!Plugin.Instance.Enabled) return;
            APManager.Instance?.APService.UnlockAchievement(achievement);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(UnlockAchievement)}", e);
        }
    }

    /// <summary>
    /// Only used for doing a flip (!)
    /// </summary>
    /// <param name="statName"></param>
    /// <param name="increment"></param>
    [HarmonyPatch(nameof(Achievements.IncrementStat), typeof(string), typeof(int))]
    [HarmonyPrefix]
    public static void IncrementStat(string statName, int increment)
    {
        try
        {
            if (!Plugin.Instance.Enabled) return;
            GoalManager.Instance?.RaiseStatEvent(statName, increment);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(IncrementStat)}", e);
        }
    }

    /// <summary>
    /// Override the CollectItems method, it will skip because Achievements are disabled.
    /// Use the internal method for tracking items and compare against the custom statistics
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="number"></param>
    /// <param name="___total_stats"></param>
    [HarmonyPatch(nameof(Achievements.CollectItem), typeof(int), typeof(double), typeof(Duration), typeof(Duration))]
    [HarmonyPrefix]
    public static void CollectItem(int itemId, double number, ItemBlock ___total_stats)
    {
        try
        {
            ___total_stats.AddItem(itemId, number);
            if (!Plugin.Instance.Enabled) return;
            GoalManager.Instance?.RaiseStatEvent(StringIds.GetItemName(itemId), number);
        }
        catch (Exception e)
        {
            Plugin.Log.LogException($"{nameof(CollectItem)}", e);
        }
    }
}