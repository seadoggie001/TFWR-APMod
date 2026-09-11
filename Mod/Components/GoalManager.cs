using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Utils;
using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago.Components;

// ToDo: Rename this to StatManager. Goal Manager sounds like it listens for AP-Goals.
public class GoalManager : BaseComponent
{
    [CanBeNull] public static GoalManager Instance;
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.GoalMgr");
    
    public IStatsService StatsService;

    protected override void OnEnable()
    {
        Instance = this;
        StatsService = new StatsService();
        InjectionService.Inject(StatsService);
        base.OnEnable();
    }

    private void Start()
    {
        StatsService.Initialize(APManager.Instance?.GetLocations());

        StatEvent += OnStatEvent;
        OnDisabled += () => StatEvent -= OnStatEvent;
        
        APManager.Instance?.APService.OptionsLoaded += OnConnectionResult;
        OnDisabled += () => APManager.Instance?.APService.OptionsLoaded -= OnConnectionResult;
        
        APManager.Instance?.APService.APDisconnected += OnAPDisconnected;
        OnDisabled += () => APManager.Instance?.APService.APDisconnected -= OnAPDisconnected;

        GameManager.Instance?.GameService.GameLoaded += OnGameLoaded;
        OnDisabled += () => GameManager.Instance?.GameService.GameLoaded -= OnGameLoaded;
    }

    private void OnAPDisconnected(object sender, string e) => StatsService.Stop();

    private void OnConnectionResult(object _, APOptions apOptions) => StatsService.LoadOptions(apOptions);

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
                Log.LogException(nameof(RaiseStatEvent), ex);
            }
        });
    }

    public List<Pair<string, double>> UserStatsSave() => StatsService.Save();

    private void OnGameLoaded(object _, ModSaveGame e)
    {
        StatsService.Stop();
        StatsService.Load(e?.Statistics);
    }

    private void OnStatEvent(object _, Stat e) => StatsService.Add(e.Name, e.Value);
}