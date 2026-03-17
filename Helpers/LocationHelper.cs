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
        foreach (string item in _locationQueue)
        {
            TryGivePlayerLocation(item);
        }
    }

    public void OnLocationsReceived(ReadOnlyCollection<long> locations)
    {
        foreach (long location in locations)
        {
            string locationName = Plugin.Instance.Session.Locations.GetLocationNameFromId(location, Plugin.GameName);
            
            // Give the player the location... somehow...
            // Maybe I'll log it for now to see what's here
            Plugin.Log.LogInfo($"Found location- ID: {location} Name: {locationName}");
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
    
    private bool GivePlayerLocation(string itemName)
    {
        try
        {
            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null) return false;
            int apItemCount = farm.NumUnlocked(APItems);
            Plugin.Log.LogInfo($"Unlocked AP Items: {apItemCount}");
            if (_locationsReceived < apItemCount)
            {
                _locationsReceived++;
                return true;
            }
            string unlockName = Unlocks.ItemToUnlock(itemName);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                Plugin.Log.LogWarning($"Failed to find unlock item: {itemName}");
                return true; // Don't keep it in the queue
            }
            
            int count = farm.NumUnlocked(unlockName);
            Plugin.Log.LogInfo($"Found {count} unlocked {unlockName}");
            
            // Hopefully we do not allow for "too many" items... but I think the game handles that internally
            farm.Unlock(unlockName, count + 1);
            farm.Unlock(APItems, ++_locationsReceived);
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