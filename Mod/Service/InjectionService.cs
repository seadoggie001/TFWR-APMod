using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Service;

public static class InjectionService
{
    public static ILoggerFactory LoggerFactory { private get; set; }
    public static ILogger Log { private get; set; }
    private static readonly Dictionary<Type, object> InjectableServices = new();

    public static void AutomaticRegistration()
    {
        IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(m => m.GetCustomAttribute<InjectableAttribute>() != null);
        foreach (Type type in types)
        {
            // Find an empty constructor
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor is null)
            {
                Log.LogWarning($"Failed to auto-register {type.Name}: Missing empty constructor");
                continue;
            }

            // Create the object with that constructor
            object injectableObject = constructor.Invoke([]);
            InjectableAttribute attr = type.GetCustomAttribute<InjectableAttribute>();
            if (attr is null) continue;

            Log.LogInformation($"Injectable: [{type.Name} = {attr.Interface.Name}]");
            // Save it as an injectable service
            InjectableServices.Add(attr.Interface, injectableObject);
        }
    }

    /// <summary>
    /// Register a type and a service to inject
    /// </summary>
    /// <param name="type"></param>
    /// <param name="service"></param>
    public static void Register(Type type, object service)
    {
        if (InjectableServices.ContainsKey(type)) return;
        InjectableServices.Add(type, service);
    }

    /// <inheritdoc cref="Register" />
    public static void Register<T>(object service) => Register(typeof(T), service);

    /// <summary>
    /// Inject registered services into any properties or fields
    /// </summary>
    /// <param name="target">Object to inject services into</param>
    /// <exception cref="Exception"></exception>
    public static T Inject<T>(T target)
    {
        target = InjectProperties(target);
        target = InjectFields(target);
        target = InjectLog(target);
        if (target is IInjectable injectable) injectable.OnInject();
        return target;
    }

    private static T InjectProperties<T>(T target)
    {
        // Get all properties on the object
        PropertyInfo[] properties = target.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // For each property with a ModInject attribute
        foreach (PropertyInfo propertyInfo in properties
                     .Where(m => m.GetCustomAttribute<ModInjectAttribute>() is not null))
        {
            // Look for a service in the dictionary to use
            if (!InjectableServices.TryGetValue(propertyInfo.PropertyType, out object value))
            {
                throw new Exception($"Missing required injectable service. " +
                                    $"Expected Type: {propertyInfo.PropertyType.FullName}");
            }

            // Set the value of the property (ie: inject the value)
            propertyInfo.SetValue(target, value);
            Log.LogInformation($"{typeof(T).Name}.{propertyInfo.Name} = {value.GetType().Name}");
        }

        return target;
    }

    private static T InjectFields<T>(T target)
    {
        FieldInfo[] fields = target.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo fieldInfo in fields
                     .Where(m => m.GetCustomAttribute<ModInjectAttribute>() is not null))
        {
            if (!InjectableServices.TryGetValue(fieldInfo.FieldType, out object value))
            {
                throw new Exception($"Missing required injectable service. " +
                                    $"Expected Type: {fieldInfo.FieldType.FullName}");
            }

            fieldInfo.SetValue(target, value);
            Log.LogInformation($"{typeof(T).Name}.{fieldInfo.Name} = {value.GetType().Name}");
        }

        return target;
    }

    private static T InjectLog<T>(T target)
    {
        IEnumerable<FieldInfo> fieldInfo = typeof(T)
            .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo info in fieldInfo)
        {
            LogAttribute attribute = info.GetCustomAttribute<LogAttribute>();
            if(attribute is null) continue;
    
            if (info.FieldType == typeof(ILogger<T>))
            {
                info.SetValue(target, LoggerFactory.CreateLogger<T>());
            }
            else
            {
                Log.LogWarning($"Cannot inject a log into a non-standard log field. {typeof(T)}.{info.Name} is of type {info.FieldType.Name}");
            }
        }
    
        return target;
    }

    public static Component CreateAndInjectComponent(Type target)
    {
        Component created = Plugin.Instance.MainGameObject.AddComponent(target);
        if (!created)
        {
            throw new Exception($"Failed to create component of type: {target.FullName}");
        }

        created = Inject(created);
        Log.LogInfo($"Created and Injected component: {target.Name}");
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

        created = Inject(created);
        Log.LogInfo($"Created and Injected component: {typeof(T).Name}");
        created.enabled = true;
        return created;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ModInjectAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class LogAttribute : Attribute
{
}

/// <summary>
/// Use this attribute to mark an object as injectable for the interface. It will be automatically registered.
/// </summary>
/// <remarks>Must have a parameter-less constructor</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InjectableAttribute(Type interfaceFor) : Attribute
{
    public Type Interface { get; set; } = interfaceFor;
}

public interface IInjectable
{
    void OnInject();
}