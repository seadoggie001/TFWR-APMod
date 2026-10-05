using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Components;

// ToDo: Rename this to StatManager. Goal Manager sounds like it listens for AP-Goals.
public class GoalManager : BaseComponent
{
    [CanBeNull] public static GoalManager Instance;

    [Log] private readonly ILogger<GoalManager> _log = null!;
    [ModInject] public readonly IStatsService StatsService = null!;

    protected override void OnEnable()
    {
        base.OnEnable();
        Instance = this;
    }

    public override void OnInject()
    {}

    public override void Initialize()
    {
        try
        {
            StatsService.Initialize(APManager.Instance?.GetLocations());
        
            StatEvent += OnStatEvent;
            OnDisabled += () => StatEvent -= OnStatEvent;

            APManager.Instance?.APService.OptionsLoaded += OnOptionsLoaded;
            OnDisabled += () => APManager.Instance?.APService.OptionsLoaded -= OnOptionsLoaded;

            APManager.Instance?.APService.APDisconnected += OnAPDisconnected;
            OnDisabled += () => APManager.Instance?.APService.APDisconnected -= OnAPDisconnected;

            GameManager.Instance?.GameService.GameLoaded += OnGameLoaded;
            OnDisabled += () => GameManager.Instance?.GameService.GameLoaded -= OnGameLoaded;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to startup {name}", nameof(GoalManager));
        }
    }

    private void OnAPDisconnected(object sender, string e) => StatsService.Stop();

    private void OnOptionsLoaded(object _, APOptions apOptions) => StatsService.LoadOptions(apOptions);

    /// <summary>
    /// Raised to add a value to a stat
    /// </summary>
    public event EventHandler<Stat> StatEvent;

    public void RaiseStatEvent(string statName, double value)
    {
        _ = Task.Run(() =>
        {
            try
            {
                StatEvent?.Invoke(null, new Stat(statName, value));
            }
            catch (Exception ex)
            {
                _log.LogException(nameof(RaiseStatEvent), ex);
            }
        });
    }

    public List<KeyValuePair<string, double>> UserStatsSave() => StatsService.Save();

    private void OnGameLoaded(object _, ModSaveGame modSaveGame)
    {
        StatsService.Stop();
        StatsService.Load(modSaveGame?.Statistics);
    }

    private void OnStatEvent(object _, Stat e) => StatsService.Add(e.Name, e.Value);
}