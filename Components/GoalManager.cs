using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;

namespace com.seadoggie.TFWRArchipelago.Components;

public class GoalManager : BaseComponent, IGoalManagerDelegates
{
    public static GoalManager Instance;
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.GoalMgr");
    
    /// <summary>
    /// Raised when a statistic is completed
    /// </summary>
    public event EventHandler<GoalEvent> GoalEvent;
    public event EventHandler<Stat> StatEvent;
    public event EventHandler<Stat> StatTotalEvent;
    
    private IStatsService _statsService;
    
    protected override void OnEnable()
    {
        Instance = this;
        _statsService = new StatsService();
        
        base.OnEnable();
    }
    
    private void Start()
    {
        _statsService.Initialize(APManager.Instance?.GetLocations());
        
        StatEvent += OnStatEvent;
        OnDisabled += () => StatEvent -= OnStatEvent;
        
        GameManager.Instance?.GameService.GameLoaded += OnGameLoaded;
        OnDisabled += () => GameManager.Instance?.GameService.GameLoaded -= OnGameLoaded;
    }

    public void RaiseGoalEvent(GoalEvent goalEvent)
    {
        GoalEvent?.Invoke(this, goalEvent);
    }

    public void RaiseStatEvent(string statName, double value)
    {
        StatEvent?.Invoke(null, new Stat(statName, value));
    }

    public void RaiseStatTotalEvent(string statName, double value)
    {
        StatTotalEvent?.Invoke(null, new Stat(statName, value));
    }
    
    public List<Pair<string, double>> UserStatsSave()
    {
        return _statsService.Save();
    }
    
    private void OnGameLoaded(object sender, ModSaveGame e)
    {
        Log.LogInfo("Game was loaded");
        _statsService.Load(e.Statistics);
    }

    private void OnStatEvent(object sender, Stat e)
    {
        _statsService.Add(e.Name, e.Value);
    }

    public bool TryGetValue(string stat, out double value)
    {
        return _statsService.TryGetValue(stat, out value);
    }

    public IEnumerable<KeyValuePair<string, List<Milestone>>> MilestoneCopy()
    {
        return _statsService.MilestoneCopy();
    }
}

public interface IGoalManagerDelegates
{
    event EventHandler<GoalEvent> GoalEvent;
    /// <summary>
    /// Raised to add a value to a stat
    /// </summary>
    event EventHandler<Stat> StatEvent;
    /// <summary>
    /// Raised with updated stat totals 
    /// </summary>
    event EventHandler<Stat> StatTotalEvent;
}