using com.seadoggie.TFWRArchipelago.Constants;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Achievements))]
public class AchievementsPatch
{
    private static readonly object LockObject = new ();
    private static readonly HashSet<string> UnlockedAchievements = [];
    
    [HarmonyPatch(nameof(Achievements.UnlockAchievement), typeof(string))]
    [HarmonyPrefix]
    public static void UnlockAchievement(string achievement)
    {
        if (!Plugin.Instance.Enabled) return;
        Achievements.enabled = false;
        // ToDo: Lock object might only be needed when dealing with Steam Achievements... need to check on this. 
        lock (LockObject)
        {
            if(!UnlockedAchievements.Add(achievement)) return;
            switch (achievement)
            {
                case Achievement.CauseARuntimeError:
                    MainSim.Inst.UnlockHat(ResourceManager.GetHat(Constants.Hat.TrafficCone.Resource));
                    break;
                case Achievement.StackOverflow:
                    MainSim.Inst.UnlockHat(ResourceManager.GetHat(Constants.Hat.TrafficConeStack.Resource));
                    break;
                case Achievement.HigherOrderProgramming:
                    MainSim.Inst.UnlockHat(ResourceManager.GetHat(Constants.Hat.Wizard.Resource));
                    break;
            }
            Plugin.Instance.LocationHelper.SubmitLocation(achievement);
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
        if (!Plugin.Instance.Enabled) return;
        UserStats.Add(statName, increment);
    }

    /// <summary>
    /// Override the CollectItems method, it will skip because Achievements are disabled.
    /// Use the internal method for tracking items and compare against the custom statistics
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="number"></param>
    /// <param name="_"></param>
    /// <param name="__"></param>
    /// <param name="___total_stats"></param>
    [HarmonyPatch(nameof(Achievements.CollectItem), typeof(int), typeof(double), typeof(Duration), typeof(Duration))]
    [HarmonyPrefix]
    public static void CollectItem(int itemId,
        double number,
        ItemBlock ___total_stats)
    {
        ___total_stats.AddItem(itemId, number);
        UserStats.Add(StringIds.GetItemName(itemId), number);
    }
}