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
        Component created = Plugin.Instance.MainGameObject.AddComponent(target);
        if (!created)
        {
            throw new Exception($"Failed to create component of type: {target.FullName}");
        }

        created = InjectionService.Inject(created);
        Plugin.Log.LogInformation("Created and Injected component: {component}", target.Name);
        return created;
    }

    public static T CreateAndInjectComponent<T>() where T : MonoBehaviour
    {
        T created = Plugin.Instance.MainGameObject.AddComponent<T>();
        created?.enabled = false;
        if (created is null)
        {
            throw new Exception($"Failed to create component of type: {typeof(T).FullName}");
        }

        created = InjectionService.Inject(created);
        Plugin.Log.LogInformation("Created and Injected component: {component}", typeof(T).Name);
        created.enabled = true;
        return created;
    }
}