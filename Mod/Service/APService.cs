using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using com.seadoggie.TFWRArchipelago.Model;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Service;

[Injectable(typeof(IAPService))]
public class APService : IAPService
{
    [Log] private readonly ILogger<APService> _log = null!;

    private readonly HashSet<string> _achievementCache = [];

    private ArchipelagoSession Session { get; set; }
    private APLocation _goal;
    private Dictionary<string, object> _slotData;
    private APOptions _options;
    private IEnumerable<APLocation> _allLocations;

    public APService() => APDisconnected += OnAPDisconnected;

    private void OnAPDisconnected(object sender, string _) => Session?.Socket.DisconnectAsync();

    /// <summary>
    /// Fired whenever there is an error and the AP connection is lost
    /// </summary>
    public event EventHandler<string> APDisconnected;

    /// <summary>
    /// Fired when a login connection attempt is completed
    /// </summary>
    public event EventHandler<LoginResult> ConnectionResult;

    /// <summary>
    /// Fired when an achievement is unlocked
    /// </summary>
    public event EventHandler<string> AchievementUnlocked;

    /// <summary>
    /// Fired when options are loaded from the slot data
    /// </summary>
    public event EventHandler<APOptions> OptionsLoaded;

    public void ResetAchievementCache(object sender, ModSaveGame modSaveGame) => _achievementCache.Clear();

    /// <summary>
    /// Submits a location based on an achievementName
    /// </summary>
    /// <param name="achievementName"></param>
    public void UnlockAchievement(string achievementName)
    {
        Task.Run(() =>
        {
            try
            {
                // Skip already unlocked achievements
                if (!_achievementCache.Add(achievementName)) return;
                // Don't allow for Steam Achievements
                Achievements.enabled = false;
                // Let everything know that an achievement was unlocked
                AchievementUnlocked?.Invoke(this, achievementName);
                // Find the relevant location for this achievement
                APLocation location = _allLocations.FirstOrDefault(m => m.achievement == achievementName);
                if (location is null)
                {
                    _log.LogWarning("Unable to locate location for achievement " + achievementName);
                    return;
                }

                SubmitLocationById(location.id);
            }
            catch (Exception ex)
            {
                _log.LogException(nameof(UnlockAchievement), ex);
            }
        });
    }

    // Send the location to the server
    public void SubmitLocationById(long id)
    {
        Session?.Locations?.CompleteLocationChecks(id);

        // If this is the goal location, tell the server about it
        if (id == _goal?.id) Session?.SetGoalAchieved();
    }

    public async Task<bool> TryEnableAsync(
        ConnectionInfo connectionSettings,
        ILocationQueue locationQueue,
        IItemQueue itemQueue)
    {
        try
        {
            _log.LogInfo("Attempting to sign in to Archipelago");

            // Only attempt to connect if not connected already
            if (Session?.Socket?.Connected ?? false)
            {
                _log.LogWarning("Already signed in");
            }

            _log.LogInfo($"Creating a session. URL: {connectionSettings.Url}:{connectionSettings.Port}");
            // Create the session
            Session = ArchipelagoSessionFactory.CreateSession(connectionSettings.Url, connectionSettings.Port);

            _log.LogInfo("Setting up item queue");
            Session.Items.ItemReceived += itemQueue.OnItemReceived;

            _log.LogInfo("Connecting");
            RoomInfoPacket roomInfoPacket = await ConnectAsync();
            if (roomInfoPacket == null)
            {
                _log.LogError(
                    $"Failed to connect to room. Connection Details: {{URL: {connectionSettings.Url}:{connectionSettings.Port}}}");
                ConnectionResult?.Invoke(this,
                    new LoginFailure("Failed to connect. Please review the connection settings."));
                await Session.Socket.DisconnectAsync();
                return false;
            }

            _log.LogInfo("Setting up location queue");
            Session.Locations.CheckedLocationsUpdated += locationQueue.OnLocationsReceived;

            _log.LogInfo("Logging in");
            LoginResult loginResult = await LoginAsync(connectionSettings);
            ConnectionResult?.Invoke(this, loginResult);
            if (loginResult.Successful)
            {
                _log.LogInfo("Successfully logged in.");
                Session.Socket.SocketClosed += reason => APDisconnected?.Invoke(this, reason);
                Session.Socket.ErrorReceived += (_, message) => APDisconnected?.Invoke(this, message);

                // Load slot data
                _slotData = await Session.DataStorage.GetSlotDataAsync();

                // Load necessary options
                _options = new APOptions();
                InjectionService.Inject(_options);
                _options.LoadSlotData(_slotData);

                _log.LogInfo(_options);

                // Determine goal location
                _goal = _allLocations.First(m => m.name == _options.GoalName());

                _log.LogInfo("Goal name was set to " + _options.GoalName());
                if (_goal is null) _log.LogError("_goal is null!");

                OptionsLoaded?.Invoke(this, _options);

                return true;
            }

            _log.LogError(
                $"Failed to connect. Connection Details: {{URL: {connectionSettings.Url}:{connectionSettings.Port}, " +
                $"Username: {connectionSettings.Username}, " +
                $"Password? {!string.IsNullOrWhiteSpace(connectionSettings.Password)}}}");
            return false;
        }
        catch (Exception ex)
        {
            _log.LogException("Failed to connect to AP with exception", ex);
            return false;
        }
    }

    //ToDo: If auto-connect is ever set up, this will need to check (with event arguments)
    // if this is the initial connection
    public void Disconnect(object sender, EventArgs e)
    {
        if (Session?.Socket == null) return;
        Session.Socket.DisconnectAsync();
        APDisconnected?.Invoke(this, null);
    }

    private async Task<RoomInfoPacket> ConnectAsync()
    {
        RoomInfoPacket roomInfoPacket;
        try
        {
            roomInfoPacket = await Session.ConnectAsync();
        }
        catch (Exception e)
        {
            Plugin.Log.LogException("Session.ConnectAsync", e);
            _log.LogError("Exception Type: " + e.GetType());
            _log.LogError(e.Message);
            _log.LogError(e.StackTrace);
            if (e.InnerException == null) return null;
            _log.LogError(e.GetBaseException().Message);
            _log.LogError(e.InnerException.StackTrace);
            return null;
        }

        _log.LogInfo($"[RoomInfo] " +
                     $"{{ Seed: {roomInfoPacket.SeedName}; " +
                     $"Games: {string.Join(", ", roomInfoPacket.Games)}; " +
                     $"Tags: {string.Join(",", roomInfoPacket.Tags)}; " +
                     $"Version: {roomInfoPacket.GeneratorVersion.ToVersion()} }}");

        return roomInfoPacket;
    }

    private async Task<LoginResult> LoginAsync(ConnectionInfo connectionSettings)
    {
        LoginResult loginResult;
        try
        {
            loginResult = await Session.LoginAsync(
                Plugin.GameName,
                connectionSettings.Username,
                ItemsHandlingFlags.AllItems,
                Version.Parse("0.6.7"),
                [],
                null,
                connectionSettings.Password
            );
        }
        catch (Exception e)
        {
            loginResult = new LoginFailure(e.GetBaseException().Message);
            _log.LogError($"Exception Message: {e.Message}");
            _log.LogError($"Base Exception Message: {e.GetBaseException().Message}");
        }

        if (loginResult.Successful) return loginResult;
        _log.LogError($"Failed to connect to the server. All errors (if any?) to follow");
        foreach (string error in ((LoginFailure)loginResult).Errors)
        {
            _log.LogInfo(error);
        }

        return loginResult;
    }

    public APOptions GetOptions() => _options;

    public void SubmitGrass(string grassName)
    {
        if (!Plugin.Instance.Enabled || _options is null || !_options.GrassSanityEnabled()) return;
        APLocation location = _allLocations.FirstOrDefault(m => m.name == grassName);
        if (location == null)
        {
            _log.LogError($"Grass sanity is not found in the APLocations. Expected: {grassName}");
            return;
        }

        SubmitLocationById(location.id);
    }

    public void SetLocations(List<APLocation> locations)
    {
        _allLocations = locations;
    }
}

public interface IAPService
{
    /// <inheritdoc cref="APService.APDisconnected" />
    event EventHandler<string> APDisconnected;

    /// <inheritdoc cref="APService.ConnectionResult" />
    event EventHandler<LoginResult> ConnectionResult;

    /// <inheritdoc cref="APService.AchievementUnlocked" />
    event EventHandler<string> AchievementUnlocked;

    /// <inheritdoc cref="APService.OptionsLoaded" />
    event EventHandler<APOptions> OptionsLoaded;

    void ResetAchievementCache(object sender, ModSaveGame modSaveGame);
    void UnlockAchievement(string achievementName);
    void SubmitLocationById(long id);

    Task<bool> TryEnableAsync(ConnectionInfo connectionSettings, ILocationQueue locationQueue,
        IItemQueue itemQueue);

    void Disconnect(object sender, EventArgs e);
    APOptions GetOptions();
    void SubmitGrass(string grassName);
    void SetLocations(List<APLocation> locations);
}