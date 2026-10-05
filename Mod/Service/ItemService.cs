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

    public void Initialize()
    {
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

            // Register each alternative name 
            foreach (string acceptedName in item.AcceptedNames ?? [])
                RegisteredItems[acceptedName] = new ItemInfo(type, traps);
        }
    }

    public bool CanProcess(string name) => RegisteredItems.ContainsKey(name);

    public bool Process(string name)
    {
        if (RegisteredItems.TryGetValue(name, out ItemInfo item))
        {
            // If the component already exists, return that this was processed only if it is a trap
            // (traps sent while the trap is active will be ignored)
            if (Plugin.Instance.MainGameObject.TryGetComponent(item.Type, out Component _)) return item.IsTrap;

            _log.LogDebug("Creating and processing item: {Name}", name);
            // Create the component and inject any references it needs
            Component component = BepInExHelper.CreateAndInjectComponent(item.Type);
            if (component is UnlockItem unlock) unlock.SetUnlock(name);
            return true;
        }

        _log.LogError("Failed to locate item: {Name}", name);
        return false;
    }
}

public interface IItemService
{
    void Initialize();
    bool CanProcess(string name);
    bool Process(string name);
}