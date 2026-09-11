using System.Reflection;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace com.seadoggie.TFWRArchipelago.Components;

public class APManager : BaseComponent
{
    [CanBeNull] public static APManager Instance { get; private set; }
    [ModInject]
    public ILogService LogService
    {
        set => Log = value.CreateLog("TFWRAP.APMgr");
    }
    private ILogger Log;

    public IAPService APService;
    public ILocationQueue LocationQueue;
    private IItemQueue _itemQueue;

    private IEnumerable<APLocation> _apLocations;

    protected override void OnEnable()
    {
        base.OnEnable();
        Instance = this;
        _apLocations = InitializeLocations();
        List<APLocation> locations = _apLocations.ToList();
        APService = InjectionService.Inject(new APService(locations));
        LocationQueue = InjectionService.Inject(new LocationQueue(locations));
        _itemQueue = new ItemQueue((itemName, itemsReceived) =>
            GameManager.Instance?.GiveItem(itemName, itemsReceived) ?? false);
        InjectionService.Inject(_itemQueue);
    }

    private void Start()
    {
        GameManager.Instance?.GameService.PreLoadGame += APService.Disconnect;
        OnDisabled += () => GameManager.Instance?.GameService.PreLoadGame -= APService.Disconnect;

        GameManager.Instance?.GameService.GameLoaded += APService.ResetAchievementCache;
        OnDisabled += () => GameManager.Instance?.GameService.GameLoaded -= APService.ResetAchievementCache;

        GameManager.Instance?.GameService.GrassSanity += OnGrassSanity;
        OnDisabled += () => GameManager.Instance?.GameService.GrassSanity -= OnGrassSanity;

        UIManager.Instance?.settingsGUI.ConnectionAttemptEvent += OnConnectionAttemptEvent;
        OnDisabled += () => UIManager.Instance?.settingsGUI.ConnectionAttemptEvent -= OnConnectionAttemptEvent;
        
        UIManager.Instance?.settingsGUI.DisconnectRequestEvent += APService.Disconnect;
        OnDisabled += () => UIManager.Instance?.settingsGUI.DisconnectRequestEvent -= APService.Disconnect;
        
        APService.APDisconnected += OnAPDisconnected;
        OnDisabled += () => APService.APDisconnected -= OnAPDisconnected;
    }

    private void OnAPDisconnected(object sender, string e) => _itemQueue.Reset();

    private void Update()
    {
        try
        {
            _itemQueue.Process();
            LocationQueue.Process();
        }
        catch (Exception e)
        {
            Log.LogException("Failed to load update locations/items", e);
        }
    }

    private void OnGrassSanity(object sender, string grassCoords) => APService.SubmitGrass(grassCoords);

    private void OnConnectionAttemptEvent(object sender, ConnectionInfo e) =>
        APService.TryEnableAsync(e, LocationQueue, _itemQueue);

    public IEnumerable<APLocation> GetLocations() => _apLocations;

    /// <summary>
    /// Loads all Locations from data.yaml
    /// </summary>
    private IEnumerable<APLocation> InitializeLocations()
    {
        try
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            string locationText = File.ReadAllText(Path.Combine(folderPath, "locations.json"));
            List<APLocation> locationData = JsonConvert.DeserializeObject<List<APLocation>>(locationText);
            Log.LogInfo($"Loaded {locationData.Count} locations");
            return locationData;
        }
        catch (Exception e)
        {
            Log.LogException("Failed to load APLocation data", e);
            return [];
        }
    }
}