using com.seadoggie.TFWRArchipelago.Constants;
using com.seadoggie.TFWRArchipelago.Patches;
using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago;

/// <summary>
/// A Stat tracking implementation to enable custom statistics
/// </summary>
public static class UserStats
{
    private static readonly Dictionary<string, double> Stats = new();

    /// <summary>
    /// The Milestones to unlock. Sorted by Target (low to high)
    /// </summary>
    /// <remarks>Performing most actions once is handled elsewhere</remarks>
    private static readonly Dictionary<string, List<Milestone>> Milestones = new();

    private static readonly object LockObject = new();

    public static event Action<string, double> OnStatChange;

    /// <summary>
    ///     Uses APLocations to determine which statistics to track
    /// </summary>
    public static void Initialize()
    {
        Plugin.Log.LogInfo("Initializing statistics...");
        // For each location with a statistic
        foreach (APLocation location in APLocation.APLocations.Where(m => m.statistic != null))
        {
            // Convert the "number" into a real number
            double value = APLocation.Parse(location.statistic!.value);
            // Create the milestone to track
            Milestone milestone = new()
            {
                APLocation = location,
                Achievement = location.achievement,
                Location = location.name,
                Target = value
            };
            // Check if the list already has this key
            if (Milestones.TryGetValue(location.statistic.key, out List<Milestone> milestones))
            {
                // If there's not already a milestone with this target value, add it
                if (!milestones.Any(m => Math.Abs(m.Target - value) < 50)) milestones.Add(milestone);
            }
            else
            {
                // Add the key and milestone
                Milestones.Add(location.statistic.key, [milestone]);
            }

            Stats[location.statistic.key] = 0;
        }

        Plugin.Log.LogInfo("Tracking stats for: " + string.Join(", ", Milestones.Keys));

        OnStatChange += GrantAchievements;
    }

    public static void Add(string name, double count)
    {
        try
        {
            lock (LockObject)
            {
                if (Stats.ContainsKey(name))
                    Stats[name] += count;
                else
                    Stats.Add(name, count);

                OnStatChange?.Invoke(name, Stats[name]);
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("UserStats.Add Error", e);
        }
    }

    /// <summary>
    ///     Gets a list of KeyValuePairs to save
    /// </summary>
    /// <returns></returns>
    public static List<Pair<string, double>> Save()
    {
        lock (LockObject)
        {
            IEnumerable<Pair<string, double>> result =
                Stats.Select(entry => new Pair<string, double>(entry.Key, entry.Value));
            return result.ToList();
        }
    }

    /// <summary>
    ///     Load a new set of statistics
    /// </summary>
    /// <param name="newStats"></param>
    public static void Load(List<Pair<string, double>> newStats)
    {
        lock (LockObject)
        {
            Stats.Clear();
            foreach (Pair<string, double> newStat in newStats)
                if (Stats.ContainsKey(newStat.key))
                    Stats.Add(newStat.key, newStat.value);
                else
                    Stats[newStat.key] = newStat.value;
        }
    }

    public static Dictionary<string, List<Milestone>> MilestoneCopy()
    {
        return new Dictionary<string, List<Milestone>>(Milestones.ToDictionary(m => m.Key, m => m.Value));
    }

    public static bool TryGetValue(string stat, out double value)
    {
        lock (LockObject)
        {
            return Stats.TryGetValue(stat, out value);
        }
    }

    /// <summary>
    ///     Grants Stat-based Achievements similar to Steam
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="count"></param>
    private static void GrantAchievements(string stat, double count)
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

        // Loop through possible achievements
        foreach (Milestone milestone in milestones.Where(milestone => !milestone.Triggered).OrderBy(m => m.Target))
        {
            // If it's too much, stop checking
            if (milestone.Target > count) break;
            Plugin.Log.LogInfo(
                $"Found achievement! Stat: {stat} Location: {milestone.Location} Achievement: {milestone.Achievement}");
            // Grant the achievement or location
            if (!string.IsNullOrWhiteSpace(milestone.Achievement))
                AchievementsPatch.UnlockAchievement(milestone.Achievement);
            else Plugin.Instance.LocationHelper.SubmitLocation(milestone.Location);
            milestone.Triggered = true;
        }
    }
}

public record Milestone
{
    public string Location;

    [CanBeNull] public string Achievement;

    public APLocation APLocation;

    /// <summary>
    ///     The required number
    /// </summary>
    public double Target;

    /// <summary>
    ///     Has the target been reached yet?
    /// </summary>
    public bool Triggered;
}