using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Service;

/// <summary>
/// Queues received Archipelago items until the mod is ready to process them  
/// </summary>
[Injectable(typeof(IItemQueue))]
public class ItemQueue : IItemQueue
{
    [Log] private readonly ILogger<ItemQueue> _log = null!;

    /// <summary># of items received from AP</summary>
    private int _itemsReceived;

    /// <summary># of items already processed</summary>
    private int _itemsProcessed;

    private string _stash = "";
    private readonly List<string> _itemQueue = [];

    public event EventHandler<ItemReceivedData> ItemReady;

    public void Process()
    {
        // Wait for an item in the queue
        if (_itemQueue.Count == 0) return;
        // Wait for all received items to have been processed
        bool useStash = _itemsProcessed < _itemsReceived;

        string item = useStash ? _stash : _itemQueue.First();
        ItemReady?.Invoke(this, new ItemReceivedData()
        {
            ItemName = item,
            ItemCount = _itemsReceived,
        });
        if (useStash) return;
        _itemQueue.Remove(item);
        _stash = item;
        _itemsReceived++;
    }

    public void OnItemReceived(IReceivedItemsHelper helper)
    {
        ItemInfo itemInfo = helper.PeekItem();
        string itemReceivedName = itemInfo.ItemDisplayName;
        if (_itemsProcessed <= _itemsReceived)
        {
            _itemQueue.Add(itemReceivedName);
        }
        else
        {
            _itemsReceived++;
        }
        helper.DequeueItem();
    }

    public void Reset(int count = 0)
    {
        _itemsReceived = 0;
        _itemsProcessed = count;
        _itemQueue.Clear();
    }

    public void IncrementItemProcessed() => _itemsProcessed++;
}

public interface IItemQueue
{
    event EventHandler<ItemReceivedData> ItemReady;
    void Process();
    void OnItemReceived(IReceivedItemsHelper helper);
    void Reset(int count = 0);
    void IncrementItemProcessed();
}

public record ItemReceivedData
{
    public string ItemName;
    public int ItemCount;
};