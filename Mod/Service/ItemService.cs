using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.seadoggie.TFWRArchipelago.Items;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Service;

[Injectable(typeof(IItemService))]
public class ItemService : IItemService
{
    [Log] private readonly ILogger<ItemService> _log = null!;

    private static readonly Dictionary<string, ItemInfo> RegisteredItems = [];

    private record ItemInfo(Type Type, bool IsTrap)
    {
        public Type Type { get; } = Type;
        public bool IsTrap { get; } = IsTrap;
    }

    public ItemService()
    {
        _log.LogInformation("Registering Items from {assembly}", Assembly.GetExecutingAssembly().FullName);
        // Using reflection, find any type with an Item attribute
        IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes();

        Register<ItemAttribute>(types, false);
        Register<TrapAttribute>(types, true);

        // Manually register each unlock with the UnlockItem
        foreach (string unlock in APItem.Unlocks) RegisteredItems[unlock] = new ItemInfo(typeof(UnlockItem), false);
        _log.LogInformation("Registered UnlockItems: {items}", string.Join(",", RegisteredItems.Keys));
    }

    private static void Register<T>(IEnumerable<Type> types, bool traps) where T : ItemAttribute
    {
        // For each type found
        foreach (Type type in types.Where(m => m.GetCustomAttribute<T>() != null))
        {
            // Register the primary name
            T item = type.GetCustomAttribute<T>();
            RegisteredItems[item.Name] = new ItemInfo(type, traps);
            // Log.LogInfo($"Item: {item.Name} Type: {type.FullName}");

            // Register each alternative name 
            foreach (string acceptedName in item.AcceptedNames ?? [])
            {
                RegisteredItems[acceptedName] = new ItemInfo(type, traps);
                // Log.LogInfo($"\t Alternate: {acceptedName}");
            }
        }
    }

    public bool CanProcess(string name) => RegisteredItems.ContainsKey(name);

    private readonly object _componentLock = new();

    public bool Process(string name)
    {
        if (RegisteredItems.TryGetValue(name, out ItemInfo item))
        {
            lock (_componentLock)
            {
                // If the component already exists, return that this was processed only if it is a trap
                // (traps sent while the trap is active will be ignored)
                if (Plugin.Instance.MainGameObject.TryGetComponent(item.Type, out Component _))
                {
                    _log.LogWarning("Failed to process item: {Name}", name);
                    return item.IsTrap;
                }

                _log.LogInformation("Creating and processing item: {Name}", name);
                // Create the component and inject any references it needs
                Component component = BepInExHelper.CreateAndInjectComponent(item.Type);
                if (component is UnlockItem unlock) unlock.SetUnlock(name);
                return true;
            }
        }

        Plugin.Log.LogError("Failed to locate item: {Name}", name);
        return false;
    }
}

public interface IItemService
{
    bool CanProcess(string name);
    bool Process(string name);
}