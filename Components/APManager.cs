using System.Reflection;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace com.seadoggie.TFWRArchipelago.Components;

public class APManager : BaseComponent, IAPManager
{
    [CanBeNull] public static APManager Instance { get; private set; }
    private static readonly ManualLogSource Log = new ("TFWRAP.APManager"); 

    public readonly IAPService APService = new APService(Instance);
    public readonly ILocationQueue LocationQueue;
    public readonly IItemQueue ItemQueue;

    private readonly IEnumerable<APLocation> _apLocations;

    public APManager()
    {
        Instance = this;
        LocationQueue = new LocationQueue(this);
        ItemQueue = new ItemQueue((itemName, itemsReceived) =>
            GameManager.Instance?.GiveItem(itemName, itemsReceived) ?? false);
        _apLocations = InitializeLocations();
    }

    private void Start()
    {
        GameManager.Instance?.GameService.PreLoadGame += APService.Disconnect;
        OnDisabled += () => GameManager.Instance?.GameService.PreLoadGame -= APService.Disconnect;
        
        GameManager.Instance?.GameService.GameLoaded += APService.OnGameLoaded;
        OnDisabled += () => GameManager.Instance?.GameService.GameLoaded -= APService.OnGameLoaded;

        UIManager.Instance?.ConnectionAttemptEvent += OnConnectionAttemptEvent;
        OnDisabled += () => UIManager.Instance?.ConnectionAttemptEvent -= OnConnectionAttemptEvent;
    }

    private void Update()
    {
        try
        {
            ItemQueue.Process();
            LocationQueue.Process();
        }
        catch (Exception e)
        {
            Log.LogException("Failed to load update locations/items", e);
        }
    }

    private void OnConnectionAttemptEvent(object sender, ConnectionInfo e) =>
        APService.TryEnableAsync(e, LocationQueue, ItemQueue);

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
            return locationData;
        }
        catch (Exception e)
        {
            Log.LogException("Failed to load APLocation data", e);
            return [];
        }
    }
}

public interface IAPManager
{
    IEnumerable<APLocation> GetLocations();
}