using com.seadoggie.TFWRArchipelago.Constants;
using com.seadoggie.TFWRArchipelago.Patches;

namespace com.seadoggie.TFWRArchipelago;

/// <summary>
/// I think the idea here was to implement my own tracking of stats to enable custom statistics
/// </summary>
public static class UserStats
{
    private static readonly Dictionary<string, double> Stats = new();

    /// <summary>
    /// The Milestones to unlock. Sorted by Target (low to high)
    /// </summary>
    /// <remarks>Performing most actions once is handled elsewhere</remarks>
    private static Dictionary<string, List<Milestone>> Milestones = new() { };

    private static readonly object LockObject = new();

    public static double Add(string name, double count)
    {
        try
        {
            lock (LockObject)
            {
                if (Stats.ContainsKey(name))
                {
                    Stats[name] += count;
                }
                else
                {
                    Stats.Add(name, count);
                }

                Plugin.Log.LogInfo("UserStats.Add " + name + " - " + count);
                GrantAchievements(name, Stats[name]);
                return Stats[name];
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("UserStats.Add Error", e);
            return 0;
        }
    }

    /// <summary>
    /// Grants Stat-based Achievements similar to Steam
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="count"></param>
    public static void GrantAchievements(string stat, double count)
    {
        if (!Plugin.Instance.Enabled)
        {
            Plugin.Log.LogWarning("Not tracking stats, currently disabled");
            return;
        }

        // Find the milestone
        if (!Milestones.TryGetValue(stat, out List<Milestone> milestones))
        {
            Plugin.Log.LogWarning($"Tracking Stats, but found nothing for {stat}!");
            return;
        }

        // ignore if it's empty
        if (milestones is null || !milestones.Any())
        {
            Plugin.LogError($"Found a really weird milestone! Stat for {stat} but the value is null or empty list?");
            return;
        }

        foreach (Milestone milestone in milestones.Where(milestone => milestone.Triggered))
        {
            Plugin.Log.LogInfo($"Already triggered. Stat: {stat} Achievement: {milestone.Achievement}");
        }

        // Loop through possible achievements
        foreach (Milestone milestone in milestones.Where(milestone => !milestone.Triggered))
        {
            // If it's too much, stop checking
            if (milestone.Target > count) continue;
            Plugin.Log.LogInfo($"Found achievement! Stat: {stat} Achievement: {milestone.Achievement}");
            // Grant the achievement
            AchievementsPatch.UnlockAchievement(milestone.Achievement);
            milestone.Triggered = true;
        }
    }

    public static List<Pair<string, double>> Save()
    {
        lock (LockObject)
        {
            IEnumerable<Pair<string, double>> result =
                Stats.Select(entry => new Pair<string, double>(entry.Key, entry.Value));
            return result.ToList();
        }
    }

    public static void Load(List<Pair<string, double>> newStats)
    {
        lock (LockObject)
        {
            Stats.Clear();
            foreach (Pair<string, double> newStat in newStats)
            {
                if (Stats.ContainsKey(newStat.key))
                {
                    Stats.Add(newStat.key, newStat.value);
                }
                else
                {
                    Stats[newStat.key] = newStat.value;
                }
            }
        }
    }

    public static void InitializeStatistics()
    {
        Plugin.Log.LogInfo("Initializing statistics...");
        IEnumerable<APLocation> statisticLocations = APLocation.APLocations.Where(m => m.statistic != null);
        foreach (APLocation location in statisticLocations)
        {
            // Convert the "number" into a real number
            double value = APLocation.Parse(location.statistic!.value);
            Milestone milestone = new() { Achievement = location.achievement, Target = value };
            // Check if the list already has this key
            if (Milestones.TryGetValue(location.statistic!.key, out List<Milestone> milestones))
            {
                // If there's not already a milestone with this target value, add it
                if (!milestones.Any(m => Math.Abs(m.Target - value) < 50)) milestones.Add(milestone);
            }
            else
            {
                // Add the key and milestone
                Milestones.Add(location.statistic!.key, [milestone]);

                Plugin.Log.LogInfo(" - " + location.statistic!.key);
            }
        }
    }
}

public record Milestone
{
    public double Target;
    public bool Triggered = false;
    public string Achievement;
}