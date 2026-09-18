using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.seadoggie.TFWRArchipelago.Service;

public static class InjectionService
{
    private static readonly Dictionary<Type, object> InjectableServices = new();

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
    /// Inject registered services into any properties (ie: those with get and set)
    /// </summary>
    /// <param name="target">Object to inject services into</param>
    /// <exception cref="Exception"></exception>
    public static T Inject<T>(T target)
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
        }

        return target;
    }
}

public class ModInjectAttribute : Attribute
{
}