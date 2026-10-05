using System;
using com.seadoggie.TFWRArchipelago.Service;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Utils;

public static class BepInExHelper
{
    /// <summary>Return this to skip the original function</summary>
    public const bool HarmonySkipFunction = false;

    public static Component CreateAndInjectComponent(Type target)
    {
        try
        {
            Component created = Plugin.Instance.MainGameObject.AddComponent(target);
            if (!created)
            {
                throw new Exception($"Failed to create component of type: {target.FullName}");
            }

            InjectionService.InjectType(created, target);
            Plugin.Log.LogDebug("Created and Injected component: {component}", target.Name);
            return created;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("Failed to create component: {target}", target.Name);
            return null;
        }
    }

    public static T CreateAndInjectComponent<T>() where T : MonoBehaviour
    {
        try
        {
            T created = Plugin.Instance.MainGameObject.AddComponent<T>();
            created?.enabled = false;
            if (created is null)
            {
                throw new Exception($"Failed to create component of type: {typeof(T).FullName}");
            }

            created = InjectionService.Inject(created);
            Plugin.Log.LogDebug("Created and Injected component: {component}", typeof(T).Name);
            created.enabled = true;
            return created;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("Failed to create component: {target}", typeof(T).Name);
        }

        return null;
    }
}