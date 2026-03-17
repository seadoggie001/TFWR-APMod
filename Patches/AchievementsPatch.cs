using com.seadoggie.TFWRArchipelago.Constants;
using HarmonyLib;

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

    [HarmonyPatch(nameof(Achievements.IncrementStat), typeof(string), typeof(int))]
    [HarmonyPrefix]
    public static void IncrementStat(string statName, int increment)
    {
        if (!Plugin.Instance.Enabled) return;
        UserStats.Add(statName, increment);
    }
}