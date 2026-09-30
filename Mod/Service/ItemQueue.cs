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
    private const float RefreshRate = 0.1f;

    [Log] private readonly ILogger<ItemQueue> _log = null!;

    private int _itemsReceived;
    private readonly List<string> _itemQueue = [];

    public event EventHandler<ItemReceivedData> ItemReady;

    public void Process()
    {
        if (_itemQueue.Count == 0) return;
        string item = _itemQueue.First();
        ItemReady?.Invoke(this, new ItemReceivedData()
        {
            ItemName = item,
            ItemCount = _itemsReceived,
        });
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
    event EventHandler<ItemReceivedData> ItemReady;
    void Process();
    void OnItemReceived(IReceivedItemsHelper helper);
    void Reset();
}

public record ItemReceivedData
{
    public string ItemName;
    public int ItemCount;
};