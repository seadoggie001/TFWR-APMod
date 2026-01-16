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
        lock (LockObject)
        {
            if(!UnlockedAchievements.Add(achievement)) return;
            switch (achievement)
            {
                case "CAUSE_A_RUNTIME_ERROR":
                    MainSim.Inst.UnlockHat(ResourceManager.GetHat(TFWRTypes.Hat.TrafficCone.Resource));
                    break;
                case "STACK_OVERFLOW":
                    MainSim.Inst.UnlockHat(ResourceManager.GetHat(TFWRTypes.Hat.TrafficConeStack.Resource));
                    break;
                case "HIGHER_ORDER_PROGRAMMING":
                    MainSim.Inst.UnlockHat(ResourceManager.GetHat(TFWRTypes.Hat.Wizard.Resource));
                    break;
            }
            // Do archipelago stuff here
            if(!Plugin.Instance.Enabled) return;
        }
    }

    [HarmonyPatch(nameof(Achievements.IncrementStat), typeof(string), typeof(int))]
    [HarmonyPrefix]
    public static void IncrementStat(string statName, int increment)
    {
        UserStats.Add(statName, increment);
    }
}