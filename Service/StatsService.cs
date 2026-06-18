using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;

namespace com.seadoggie.TFWRArchipelago.Service;

/// <summary>
/// A Stat tracking implementation to enable custom statistics
/// </summary>
public class StatsService : IStatsService
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.StatsService");

    private readonly Dictionary<string, double> _stats = new();

    /// <summary>
    /// The Milestones to unlock. Sorted by Target (low to high). Key is an ItemName
    /// </summary>
    private readonly Dictionary<string, List<Milestone>> _milestones = new();

    private readonly object _lockObject = new();

    /// <summary>
    ///     Uses APLocations to determine which statistics to track
    /// </summary>
    public void Initialize(IEnumerable<APLocation> locations)
    {
        Log.LogInfo("Initializing statistics...");
        // For each location with a statistic
        foreach (APLocation location in locations?.Where(m => m.statistic != null) ?? [])
        {
            // Convert the "number" into a real number
            double value = Parse.FormattedDouble(location.statistic!.value);
            // Create the milestone to track
            Milestone milestone = new()
            {
                APLocation = location,
                Achievement = location.achievement,
                Location = location.name,
                Target = value
            };
            // Check if the list already has this key
            if (_milestones.TryGetValue(location.statistic.key, out List<Milestone> milestones))
            {
                // If there's not already a milestone with this target value, add it
                if (!milestones.Any(m => Math.Abs(m.Target - value) < 50)) milestones.Add(milestone);
            }
            else
            {
                // Add the key and milestone
                _milestones.Add(location.statistic.key, [milestone]);
            }

            _stats[location.statistic.key] = 0;
        }

        Log.LogInfo("Tracking stats for: " + string.Join(", ", _milestones.Keys));
    }

    public void Add(string name, double count)
    {
        double total;
        try
        {
            lock (_lockObject)
            {
                if (_stats.ContainsKey(name))
                    _stats[name] += count;
                else
                    _stats.Add(name, count);
                total = _stats[name];
            }
        }
        catch (Exception e)
        {
            Log.LogException("UserStats.Add Error", e);
            return;
        }

        GrantAchievements(name, total);

        GoalManager.Instance.RaiseStatTotalEvent(name, total);
    }

    /// <summary>
    ///     Gets a list of KeyValuePairs to save
    /// </summary>
    /// <returns></returns>
    public List<Pair<string, double>> Save()
    {
        lock (_lockObject)
        {
            IEnumerable<Pair<string, double>> result =
                _stats.Select(entry => new Pair<string, double>(entry.Key, entry.Value));
            return result.ToList();
        }
    }

    /// <summary>
    ///     Load a new set of statistics
    /// </summary>
    /// <param name="newStats"></param>
    public void Load(List<Pair<string, double>> newStats)
    {
        lock (_lockObject)
        {
            _stats.Clear();
            foreach (Pair<string, double> newStat in newStats)
                if (_stats.ContainsKey(newStat.key))
                    _stats.Add(newStat.key, newStat.value);
                else
                    _stats[newStat.key] = newStat.value;
        }
    }

    public Dictionary<string, List<Milestone>> MilestoneCopy()
    {
        return new Dictionary<string, List<Milestone>>(_milestones.ToDictionary(m => m.Key, m => m.Value));
    }

    /// <summary>
    ///     Grants Stat-based Achievements similar to Steam
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="count"></param>
    private void GrantAchievements(string stat, double count)
    {
        if (!Plugin.Instance.Enabled)
        {
            Log.LogWarning("Not tracking stats, currently disabled");
            return;
        }

        // Find the milestone
        if (!_milestones.TryGetValue(stat, out List<Milestone> milestones))
        {
            Log.LogWarning($"Tracking Stats, but found nothing for {stat}!");
            return;
        }

        // ignore if it's empty
        if (milestones is null || !milestones.Any())
        {
            Log.LogException($"Found a really weird milestone! Stat for {stat} but the value is null or empty list?");
            return;
        }

        // Loop through possible achievements
        foreach (Milestone milestone in milestones.Where(milestone => !milestone.Triggered).OrderBy(m => m.Target))
        {
            // If it's too much, stop checking
            if (milestone.Target > count) break;
            Log.LogInfo(
                $"Found achievement! Stat: {stat} Location: {milestone.Location} Achievement: {milestone.Achievement}");
            // Grant the achievement or location
            GoalManager.Instance.RaiseGoalEvent(!string.IsNullOrWhiteSpace(milestone.Achievement)
                ? new GoalEvent(milestone.APLocation.id, milestone.Achievement, GoalType.Achievement)
                : new GoalEvent(milestone.APLocation.id, milestone.Location, GoalType.Statistic));
            milestone.Triggered = true;
        }
    }

    public bool TryGetValue(string stat, out double value)
    {
        lock (_lockObject)
        {
            return _stats.TryGetValue(stat, out value);
        }
    }
}

public interface IStatsService
{
    /// <summary>
    ///     Uses APLocations to determine which statistics to track
    /// </summary>
    void Initialize(IEnumerable<APLocation> locations);

    void Add(string name, double count);

    /// <summary>
    ///     Gets a list of KeyValuePairs to save
    /// </summary>
    /// <returns></returns>
    List<Pair<string, double>> Save();

    /// <summary>
    ///     Load a new set of statistics
    /// </summary>
    /// <param name="newStats"></param>
    void Load(List<Pair<string, double>> newStats);

    Dictionary<string, List<Milestone>> MilestoneCopy();
    bool TryGetValue(string stat, out double value);
}