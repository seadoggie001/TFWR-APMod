using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using com.seadoggie.TFWRArchipelago.Model;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Service;

/// <inheritdoc cref="ILocationQueue" />
[Injectable(typeof(ILocationQueue))]
public class LocationQueue : ILocationQueue
{
    [Log] private readonly ILogger<LocationQueue> _log = null!;
    private readonly HashSet<long> _locationQueue = [];

    private IEnumerable<APLocation> _allLocations = null;

    public event EventHandler<APLocation> APLocationGiven;

    public void Process()
    {
        if (_locationQueue.Count == 0) return;
        long location = _locationQueue.ElementAt(0);
        try
        {
            if (_allLocations is null) return;
            if (GivePlayerLocation(location)) _locationQueue.Remove(location);
        }
        catch (Exception e)
        {
            _log.LogException($"Failed to process {location}", e);
        }
    }

    public void SetLocations(IEnumerable<APLocation> locations) => _allLocations = locations;

    public void OnLocationsReceived(ReadOnlyCollection<long> locations) => _locationQueue.UnionWith(locations);

    private bool GivePlayerLocation(long location)
    {
        try
        {
            APLocation apLocation = _allLocations.FirstOrDefault(m => m.id == location);

            if (apLocation is null)
            {
                _log.LogError($"Failed to find AP Location with ID: {location}");
                return false;
            }

            APLocationGiven?.Invoke(this, apLocation);

            return true;
        }
        catch (Exception e)
        {
            _log.LogError(e.Message);
            if (e.InnerException != null)
            {
                _log.LogError(e.InnerException.Message);
            }

            _log.LogInfo(e.StackTrace);
            return false;
        }
    }
}

/// <summary>
/// Queues received Archipelago locations until the mod is ready to process them
/// </summary>
public interface ILocationQueue
{
    /// <summary>
    /// Raised when a Location should be given to the player
    /// </summary>
    event EventHandler<APLocation> APLocationGiven;

    void Process();
    
    /// <summary>
    /// Tell the queue which locations to process
    /// </summary>
    /// <param name="locations"></param>
    void SetLocations(IEnumerable<APLocation> locations);

    /// <summary>
    /// Add locations to the queue
    /// </summary>
    /// <param name="locations"></param>
    void OnLocationsReceived(ReadOnlyCollection<long> locations);
}