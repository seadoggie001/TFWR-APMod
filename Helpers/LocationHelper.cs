using System.Collections.ObjectModel;
using com.seadoggie.TFWRArchipelago.Constants;
using com.seadoggie.TFWRArchipelago.Patches;

namespace com.seadoggie.TFWRArchipelago.Helpers;

public class LocationHelper
{
    private int _locationsReceived;
    private const string APItems = "aplocations";
    private readonly List<long> _locationQueue = [];

    public void Update()
    {
        if (_locationQueue.Count == 0 || Plugin.Instance.Session == null) return;
        TryGivePlayerLocation(_locationQueue[0]);
    }

    public void OnLocationsReceived(ReadOnlyCollection<long> locations) => _locationQueue.AddRange(locations);

    public void TryGivePlayerLocation(long item)
    {
        if (GivePlayerLocation(item)) _locationQueue.Remove(item);
    }

    public void SubmitAchievement(string achievement)
    {
        try
        {
            if (APLocation.APLocations == null)
            {
                Plugin.LogError("APLocations is null, not submitting location");
                return;
            }

            // Find the first location with that achievement mentioned
            APLocation apLocation = APLocation.APLocations.FirstOrDefault(m => m.achievement == achievement);
            if (apLocation is null)
            {
                Plugin.LogError($"Failed to find AP Location with achievement of {achievement}");
                return;
            }

            // Send the location to the server
            Plugin.Instance.Session.Locations.CompleteLocationChecks(apLocation.id);
            
            StatisticsGUI.Instance.MarkCompleted(apLocation.name);
        }
        catch (Exception e)
        {
            Plugin.LogError("Submit achievement", e);
        }
    }

    public void SubmitLocation(string location)
    {
        try
        {
            if (APLocation.APLocations == null)
            {
                Plugin.LogError("APLocations is null, not submitting location");
                return;
            }

            // Find the first location with that achievement mentioned
            APLocation apLocation = APLocation.APLocations.FirstOrDefault(m => m.name == location);
            if (apLocation is null)
            {
                Plugin.LogError($"Failed to find AP Location: {location}");
                return;
            }

            // Send the location to the server
            Plugin.Instance.Session.Locations.CompleteLocationChecks(apLocation.id);
        }
        catch (Exception e)
        {
            Plugin.LogError("Submit location", e);
        }
    }
    
    private bool GivePlayerLocation(long location)
    {
        try
        {
            APLocation apLocation = APLocation.APLocations.FirstOrDefault(m => m.id == location);
            if (apLocation is null)
            {
                Plugin.LogError($"Failed to find AP Location with ID: {location}");
                return false;
            }

            // Find the farm
            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                Plugin.LogError($"Failed to find Farm. Location ID: {location}");
                return false;
            }

            // Check if we've received too many locations already
            // int apLocationCount = farm.NumUnlocked(APItems);
            // Plugin.Log.LogInfo($"Unlocked AP Locations: {apLocationCount}");
            // if (_locationsReceived < apLocationCount)
            // {
            //     _locationsReceived++;
            //     return true;
            // }
            
            if (apLocation.statistic != null)
            {
                // find the statistic and mark it as triggered
                StatisticsGUI.Instance.MarkCompleted(apLocation.statistic.key, APLocation.Parse(apLocation.statistic.value));
            }
            else if (apLocation.timed != null)
            {
                // find the timed statistic and mark it as triggered
                StatisticsGUI.Instance.MarkCompleted(apLocation.timed.key, APLocation.Parse(apLocation.timed.value));
            }
            else if (apLocation.achievement != null)
            {
                int count = farm.NumUnlocked(apLocation.achievement);
                farm.Unlock(apLocation.achievement, count + 1);
                StatisticsGUI.Instance.MarkCompleted(apLocation.name);
            }
            else
            {
                Plugin.LogError($"Invalid APLocation. Not an achievement, statistic, or timed statistic."
                                + $" Name: {apLocation.name} ID: {apLocation.id}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError(e.Message);
            if (e.InnerException != null)
            {
                Plugin.Log.LogError(e.InnerException.Message);
            }

            Plugin.Log.LogInfo(e.StackTrace);
            return false;
        }
    }
}