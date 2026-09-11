using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;

namespace com.seadoggie.TFWRArchipelago.Service;

/// <summary>
/// Queues received Archipelago items until the mod is ready to process them  
/// </summary>
/// <param name="processItemCallback"></param>
public class ItemQueue(Func<string, int, bool> processItemCallback) : IItemQueue
{
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
        _itemQueue.Add(itemReceivedName);
        helper.DequeueItem();
    }

    public void Reset()
    {
        _itemsReceived = 0;
        _itemQueue.Clear();
    }
}

public interface IItemQueue
{
    void Process();
    void OnItemReceived(IReceivedItemsHelper helper);
    void Reset();
}