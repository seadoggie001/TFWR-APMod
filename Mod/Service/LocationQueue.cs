using System.Collections.ObjectModel;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;

namespace com.seadoggie.TFWRArchipelago.Service;

public class LocationQueue(IAPManager apManager) : ILocationQueue
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.LocQ");
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
            APLocation apLocation = apManager.GetLocations().FirstOrDefault(m => m.id == location);

            if (apLocation is null)
            {
                Log.LogException($"Failed to find AP Location with ID: {location}");
                return false;
            }

            Log.LogInfo($"Giving player location: {apLocation.name}");

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

public interface ILocationQueue
{
    event EventHandler<APLocation> APLocationGiven;
    void Process();
    void OnLocationsReceived(ReadOnlyCollection<long> locations);
}