using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using com.seadoggie.TFWRArchipelago.Patches;

namespace com.seadoggie.TFWRArchipelago.Helpers;

public class ItemHelper
{
    private int _itemsReceived;
    private const string APItems = "apitems";
    private readonly List<string> _itemQueue = [];

    public void Update()
    {
        if(_itemQueue.Count == 0 || Plugin.Instance.Session == null) return;
        foreach (string item in _itemQueue)
        {
            TryGivePlayerItem(item);
        }
    }

    public void OnItemReceived(ReceivedItemsHelper helper)
    {
        ItemInfo itemInfo = helper.PeekItem();
        string itemReceivedName = itemInfo.ItemDisplayName;
        Plugin.Log.LogInfo($"Found item- ID: {itemInfo.ItemId} Name: {itemReceivedName}");
        _itemQueue.Add(itemReceivedName);
        helper.DequeueItem();
    }
    
    public void TryGivePlayerItem(string itemName)
    {
        if (GivePlayerItem(itemName)) _itemQueue.Remove(itemName);
    }
    
    private bool GivePlayerItem(string itemName)
    {
        try
        {
            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null) return false;
            int apItemCount = farm.NumUnlocked(APItems);
            Plugin.Log.LogInfo($"Unlocked AP Items: {apItemCount}");
            if (_itemsReceived < apItemCount)
            {
                _itemsReceived++;
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
            farm.Unlock(APItems, ++_itemsReceived);
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