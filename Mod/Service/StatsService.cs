using System;
using System.Collections.Generic;
using System.Linq;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;

namespace com.seadoggie.TFWRArchipelago.Service;

/// <summary>
/// A Stat tracking implementation to enable custom statistics
/// </summary>
public class StatsService : IStatsService
{
    [ModInject]
    // ReSharper disable once UnusedMember.Local
    private ILogService LogService
    {
        set => Log = value.CreateLog("TFWRAP.StatsService");
    }

    [ModInject] private IEnabledService EnabledService { get; set; }

    private ILogger Log { get; set; }

    /// <summary>
    /// The statistics being tracked
    /// </summary>
    private readonly Dictionary<string, double> _stats = new();

    /// <summary>
    /// A flag to control the raising of events. If the APOptions haven't been determined yet, it cannot raise events.
    /// </summary>
    private bool _canRaiseStatisticEvents = false;

    /// <summary>
    /// The Milestones to unlock. Sorted by Target (low to high). Key is an ItemName
    /// </summary>
    private readonly Dictionary<string, List<Milestone>> _milestones = new();

    /// <summary>
    /// Used to access the dictionary safely (because of multiple threads?)
    /// </summary>
    private readonly object _lockObject = new();

    /// <summary>
    /// Raised when a statistic is completed
    /// </summary>
    public event EventHandler<GoalEvent> GoalEvent;

    /// <summary>
    /// Raised with updated stat totals 
    /// </summary>
    public event EventHandler<Stat> StatTotalEvent;

    /// <summary>
    /// Uses APLocations to determine which statistics to track
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
                BaseNumber = value,
                Target = Math.Abs(value - 1) < 1 ? 1 : null,
            };
            // Check if the list already has this key
            if (_milestones.TryGetValue(location.statistic.key, out List<Milestone> milestones))
            {
                // If there's not already a milestone with this target value, add it
                if (!milestones.Any(m => Math.Abs(m.BaseNumber - value) < 5)) milestones.Add(milestone);
            }
            else
            {
                // Add the key and milestone
                _milestones.Add(location.statistic.key, [milestone]);
            }

            _stats[location.statistic.key] = 0;
            // Uncomment to see all requested stats
            // Log.LogInfo($"Stat: {location.statistic.key} Value: {value}");
        }

        Log.LogInfo("Tracking stats for: " + string.Join(", ", _milestones.Keys));
    }

    public void Stop() => _canRaiseStatisticEvents = false;

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

        StatTotalEvent?.Invoke(null, new Stat(name, total));
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
    /// Load a new set of statistics
    /// </summary>
    /// <param name="newStats"></param>
    public void Load(List<Pair<string, double>> newStats)
    {
        lock (_lockObject)
        {
            _stats.Clear();
            if (newStats is null) return;
            foreach (Pair<string, double> newStat in newStats)
            {
                if (_stats.ContainsKey(newStat.key))
                    _stats.Add(newStat.key, newStat.value);
                else
                    _stats[newStat.key] = newStat.value;
                StatTotalEvent?.Invoke(null, new Stat(newStat.key, newStat.value));
            }
        }
    }

    public Dictionary<string, List<Milestone>> MilestoneCopy() =>
        new(_milestones.ToDictionary(m => m.Key, m => m.Value));

    public Dictionary<string, double> StatCopy()
    {
        lock (_lockObject)
        {
            return new Dictionary<string, double>(_stats.ToDictionary(m => m.Key, m => m.Value));
        }
    }

    /// <summary>
    /// Grants Stat-based Achievements similar to Steam
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="count"></param>
    private void GrantAchievements(string stat, double count)
    {
        if (!EnabledService.PluginIsEnabled())
        {
            Log.LogWarning("Not tracking stats, currently disabled");
            return;
        }

        if (!_canRaiseStatisticEvents) return;

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
        foreach (Milestone milestone in milestones
                     .Where(milestone => !milestone.Triggered && milestone.Target is not null)
                     .OrderBy(m => m.Target))
        {
            // If it's too much, stop checking
            if (milestone.Target > count) break;
            Log.LogInfo(
                $"Found achievement! Stat: {stat} Location: {milestone.Location} Achievement: {milestone.Achievement}");
            // Grant the achievement or location
            GoalEvent?.Invoke(this, !string.IsNullOrWhiteSpace(milestone.Achievement)
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

    /// <summary>
    /// Use the APOptions to set the Milestone's target values.
    /// </summary>
    /// <param name="apOptions"></param>
    public void LoadOptions(APOptions apOptions)
    {
        foreach (Milestone milestone in _milestones.SelectMany(milestoneGroup => milestoneGroup.Value))
            milestone.Target = apOptions.ModifiedValues.TryGetValue(milestone.BaseNumber, out double actualValue)
                ? actualValue
                : milestone.BaseNumber;

        _canRaiseStatisticEvents = true;
    }
}

/// <inheritdoc cref="StatsService" />
public interface IStatsService
{
    /// <inheritdoc cref="StatsService.GoalEvent" />
    event EventHandler<GoalEvent> GoalEvent;

    /// <inheritdoc cref="StatsService.StatTotalEvent" />
    event EventHandler<Stat> StatTotalEvent;

    /// <inheritdoc cref="StatsService.Initialize"/>
    void Initialize(IEnumerable<APLocation> locations);

    /// <inheritdoc cref="StatsService.Add"/>
    void Add(string name, double count);

    /// <inheritdoc cref="StatsService.Save"/>
    List<Pair<string, double>> Save();

    /// <inheritdoc cref="StatsService.Load"/>
    void Load(List<Pair<string, double>> newStats);

    /// <inheritdoc cref="StatsService.MilestoneCopy"/>
    Dictionary<string, List<Milestone>> MilestoneCopy();

    /// <inheritdoc cref="StatsService.StatCopy"/>
    Dictionary<string, double> StatCopy();

    /// <inheritdoc cref="StatsService.TryGetValue"/>
    bool TryGetValue(string stat, out double value);

    /// <inheritdoc cref="StatsService.LoadOptions"/>
    void LoadOptions(APOptions apOptions);

    /// <inheritdoc cref="StatsService.Stop"/>
    void Stop();
}