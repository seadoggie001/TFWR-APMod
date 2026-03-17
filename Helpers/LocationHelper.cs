using System.Collections.ObjectModel;
using com.seadoggie.TFWRArchipelago.Patches;

namespace com.seadoggie.TFWRArchipelago.Helpers;

public class LocationHelper
{
    private int _locationsReceived;
    private const string APItems = "aplocations";
    private readonly List<string> _locationQueue = [];

    public void Update()
    {
        if(_locationQueue.Count == 0 || Plugin.Instance.Session == null) return;
        TryGivePlayerLocation(_locationQueue[0]);
    }

    public void OnLocationsReceived(ReadOnlyCollection<long> locations)
    {
        foreach (long location in locations)
        {
            string locationName = Plugin.Instance.Session.Locations.GetLocationNameFromId(location, Plugin.GameName);
            
            _locationQueue.Add(locationName);
        }
    }
    
    public void TryGivePlayerLocation(string itemName)
    {
        if (GivePlayerLocation(itemName)) _locationQueue.Remove(itemName);
    }

    public void SubmitLocation(string achievement)
    {
        try
        {
            // Convert TFWR achievement into AP location name
            string locationName = Unlocks.AchievementToLocation(achievement);
            if (locationName == null)
            {
                Plugin.Log.LogError($"Achievement isn't tracked(!): {achievement}");
                return;
            }
            // AP Location name to ID
            long locationId = Plugin.Instance.Session.Locations.GetLocationIdFromName(Plugin.GameName, locationName);
            Plugin.Log.LogInfo(
                $"Submitting: Achievement: {achievement} Location: {locationName} LocationId: {locationId}");
            // Send the location to the server
            Plugin.Instance.Session.Locations.CompleteLocationChecks(locationId);
        }
        catch (Exception e)
        {
            Plugin.LogError("Submit location", e);
        }
    }
    
    private bool GivePlayerLocation(string locationName)
    {
        try
        {
            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null) return false;
            int apLocationCount = farm.NumUnlocked(APItems);
            Plugin.Log.LogInfo($"Unlocked AP Locations: {apLocationCount}");
            if (_locationsReceived < apLocationCount)
            {
                _locationsReceived++;
                return true;
            }
            string unlockName = Unlocks.LocationToAchievement(locationName);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                Plugin.Log.LogWarning($"Failed to find location: {locationName}");
                return true; // Don't keep it in the queue
            }
            
            // ToDo: Figure out how to give achievements
            
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