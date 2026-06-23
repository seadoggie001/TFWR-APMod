using System.Reflection;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace com.seadoggie.TFWRArchipelago.Components;

public class APManager : BaseComponent
{
    [CanBeNull] public static APManager Instance { get; private set; }
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.APMgr");

    public IAPService APService;
    public ILocationQueue LocationQueue;
    private IItemQueue _itemQueue;

    private IEnumerable<APLocation> _apLocations;

    protected override void OnEnable()
    {
        Instance = this;
        _apLocations = InitializeLocations();
        List<APLocation> locations = _apLocations.ToList();
        APService = new APService(locations);
        LocationQueue = new LocationQueue(locations);
        _itemQueue = new ItemQueue((itemName, itemsReceived) =>
            GameManager.Instance?.GiveItem(itemName, itemsReceived) ?? false);

        base.OnEnable();
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
    }

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
    private static IEnumerable<APLocation> InitializeLocations()
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