using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;

namespace com.seadoggie.TFWRArchipelago.Components;

// ToDo: Rename this to StatManager. Goal Manager sounds like it listens for AP-Goals.
public class GoalManager : BaseComponent
{
    public static GoalManager Instance;
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.GoalMgr");


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

    # region Events
    
    /// <summary>
    /// Raised when a statistic is completed
    /// </summary>
    public event EventHandler<GoalEvent> GoalEvent;

    /// <summary>
    /// Raised to add a value to a stat
    /// </summary>
    public event EventHandler<Stat> StatEvent;
    
    /// <summary>
    /// Raised with updated stat totals 
    /// </summary>
    public event EventHandler<Stat> StatTotalEvent;
    
    public void RaiseGoalEvent(GoalEvent goalEvent) => GoalEvent?.Invoke(this, goalEvent);

    public void RaiseStatEvent(string statName, double value) => StatEvent?.Invoke(null, new Stat(statName, value));

    public void RaiseStatTotalEvent(string statName, double value) =>
        StatTotalEvent?.Invoke(null, new Stat(statName, value));

    public List<Pair<string, double>> UserStatsSave() => _statsService.Save();
    
    private void OnGameLoaded(object sender, ModSaveGame e) => _statsService.Load(e.Statistics);

    private void OnStatEvent(object sender, Stat e) => _statsService.Add(e.Name, e.Value);
    
    # endregion
    
    public bool TryGetValue(string stat, out double value) => _statsService.TryGetValue(stat, out value);

    public IEnumerable<KeyValuePair<string, List<Milestone>>> MilestoneCopy() => _statsService.MilestoneCopy();
}