using System.Collections.ObjectModel;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Model;

namespace com.seadoggie.TFWRArchipelago.Service;

/// <inheritdoc/>
public class LocationQueue(IEnumerable<APLocation> allLocations) : ILocationQueue
{
    [ModInject]
    private ILogService LogSource
    {
        set => Log = value.CreateLog("TFWRAP.LocQ");
    }
    private ILogger Log;
    private readonly HashSet<long> _locationQueue = [];

    public event EventHandler<APLocation> APLocationGiven;

    public void Process()
    {
        if (_locationQueue.Count == 0) return;
        long location = _locationQueue.ElementAt(0);
        try
        {
            if (GivePlayerLocation(location)) _locationQueue.Remove(location);
        }
        catch (Exception e)
        {
            Log.LogException($"Failed to process {location}", e);
        }
    }

    public void OnLocationsReceived(ReadOnlyCollection<long> locations) => _locationQueue.UnionWith(locations);

    private bool GivePlayerLocation(long location)
    {
        try
        {
            APLocation apLocation = allLocations.FirstOrDefault(m => m.id == location);

            if (apLocation is null)
            {
                Log.LogException($"Failed to find AP Location with ID: {location}");
                return false;
            }

            APLocationGiven?.Invoke(this, apLocation);

            return true;
        }
        catch (Exception e)
        {
            Log.LogError(e.Message);
            if (e.InnerException != null)
            {
                Log.LogError(e.InnerException.Message);
            }

            Log.LogInfo(e.StackTrace);
            return false;
        }
    }
}

/// <summary>
/// Queues received Archipelago locations until the mod is ready to process them
/// </summary>
public interface ILocationQueue
{
    /// <summary>
    /// Raised when a Location should be given to the player
    /// </summary>
    event EventHandler<APLocation> APLocationGiven;

    /// <summary>
    /// Attempt to process a single queued location
    /// </summary>
    void Process();

    /// <summary>
    /// Add locations to the queue
    /// </summary>
    /// <param name="locations"></param>
    void OnLocationsReceived(ReadOnlyCollection<long> locations);
}