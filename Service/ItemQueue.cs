using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;

namespace com.seadoggie.TFWRArchipelago.Service;

public class ItemQueue(Func<string, int, bool> processItemCallback) : IItemQueue
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.ItemQ");
    private int _itemsReceived;
    private readonly List<string> _itemQueue = [];

    public void Process()
    {
        if (_itemQueue.Count == 0) return;
        string item = _itemQueue.First();
        if (!processItemCallback(item, _itemsReceived)) return;
        _itemQueue.Remove(item);
        _itemsReceived++;
    }

    public void OnItemReceived(IReceivedItemsHelper helper)
    {
        ItemInfo itemInfo = helper.PeekItem();
        string itemReceivedName = itemInfo.ItemDisplayName;
        Log.LogInfo($"Added item to queue- ID: {itemInfo.ItemId} Name: {itemReceivedName}");
        _itemQueue.Add(itemReceivedName);
        helper.DequeueItem();
    }
}

public interface IItemQueue
{
    void Process();
    void OnItemReceived(IReceivedItemsHelper helper);
}