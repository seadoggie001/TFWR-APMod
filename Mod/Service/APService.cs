using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;

namespace com.seadoggie.TFWRArchipelago.Service;

public class APService(IEnumerable<APLocation> allLocations) : IAPService
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.APSrv");

    private readonly HashSet<string> _achievementCache = [];

    private ArchipelagoSession Session { get; set; }
    private APLocation _goal;
    private Dictionary<string, object> _slotData;
    private APOptions _options;

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

    public void ResetAchievementCache(object sender, ModSaveGame modSaveGame) => _achievementCache.Clear();

    /// <summary>
    /// Submits a location based on an achievementName
    /// </summary>
    /// <param name="achievementName"></param>
    public void UnlockAchievement(string achievementName)
    {
        Task.Run(() =>
        {
            // Skip already unlocked achievements
            if (!_achievementCache.Add(achievementName)) return;
            // Don't allow for Steam Achievements
            Achievements.enabled = false;
            // Let everything know that an achievement was unlocked
            AchievementUnlocked?.Invoke(this, achievementName);
            // Find the relevant location for this achievement
            APLocation location = allLocations.FirstOrDefault(m => m.achievement == achievementName);
            if (location is null)
            {
                Log.LogWarning("Unable to locate location for achievement " + achievementName);
                return;
            }

            SubmitLocationById(location.id);
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
            Log.LogInfo("Attempting to sign in to Archipelago");

            // Only attempt to connect if not connected already
            if (Session?.Socket?.Connected ?? false)
            {
                Log.LogWarning("Already signed in");
                return true;
            }

            Log.LogInfo($"Creating a session. URL: {connectionSettings.Url}:{connectionSettings.Port}");
            // Create the session
            Session = ArchipelagoSessionFactory.CreateSession(connectionSettings.Url, connectionSettings.Port);

            Log.LogInfo("Setting up item queue");
            Session.Items.ItemReceived += itemQueue.OnItemReceived;

            Log.LogInfo("Connecting");
            RoomInfoPacket roomInfoPacket = await ConnectAsync();
            if (roomInfoPacket == null)
            {
                Log.LogError(
                    $"Failed to connect to room. Connection Details: {{URL: {connectionSettings.Url}:{connectionSettings.Port}}}");
                ConnectionResult?.Invoke(this,
                    new LoginFailure("Failed to connect. Please review the connection settings."));
                return false;
            }

            Log.LogInfo("Setting up location queue");
            Session.Locations.CheckedLocationsUpdated += locationQueue.OnLocationsReceived;

            Log.LogInfo("Logging in");
            LoginResult loginResult = await LoginAsync(connectionSettings);
            ConnectionResult?.Invoke(this, loginResult);
            if (loginResult.Successful)
            {
                Log.LogInfo("Successfully logged in.");
                Session.Socket.SocketClosed += reason => APDisconnected?.Invoke(this, reason);
                Session.Socket.ErrorReceived += (Exception _, string message) => APDisconnected?.Invoke(this, message);

                // Load slot data
                _slotData = await Session.DataStorage.GetSlotDataAsync();

                _options = new APOptions
                {
                    GoalName = 0 == (long)_slotData["goal"]
                        ? "Gold Farmer"
                        : "Size Matters",
                    RandomizedCosts = (long)_slotData["crop_cost"] == 1,
                    CropCosts = new Dictionary<string, List<string>>()
                };
                
                if (_options.RandomizedCosts)
                {
                    IEnumerable<string> cropOptions =
                    [
                        "crops.Hay",
                        "crops.Bush",
                        "crops.Tree",
                        "crops.Carrot",
                        "crops.Cactus",
                        "crops.Dinosaur",
                        "crops.Sunflower",
                        "crops.Pumpkin",
                    ];
                    foreach (string cropOption in cropOptions)
                    {
                        if (!_slotData.TryGetValue(cropOption, out object cost)) continue;
                        string cropName = cropOption.Replace("crops.", "").ToLower();

                        // Dinosaurs don't need a cost, but apples do. I know it's weird, but trust me.
                        cropName = cropName.Replace("dinosaur", "apple");

                        Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray)cost;
                        _options.CropCosts[cropName] = array.Values<string>().ToList();
                    }
                }

                // Determine goal location
                _goal = allLocations.First(m => m.name == _options.GoalName);

                Log.LogInfo("Goal name was set to " + _options.GoalName);
                if (_goal is null) Log.LogError("_goal is null!");

                return true;
            }

            Log.LogError(
                $"Failed to connect. Connection Details: {{URL: {connectionSettings.Url}:{connectionSettings.Port}, " +
                $"Username: {connectionSettings.Username}, " +
                $"Password? {!string.IsNullOrWhiteSpace(connectionSettings.Password)}}}");
            return false;
        }
        catch (Exception ex)
        {
            Log.LogException("Failed to connect to AP with exception", ex);
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
            Log.LogError("Exception Type: " + e.GetType());
            Log.LogError(e.Message);
            Log.LogError(e.StackTrace);
            if (e.InnerException == null) return null;
            Log.LogError(e.GetBaseException().Message);
            Log.LogError(e.InnerException.StackTrace);
            return null;
        }

        Log.LogInfo($"[RoomInfo] " +
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
            Log.LogError($"Exception Message: {e.Message}");
            Log.LogError($"Base Exception Message: {e.GetBaseException().Message}");
        }

        if (loginResult.Successful) return loginResult;
        Log.LogError($"Failed to connect to the server. All errors (if any?) to follow");
        foreach (string error in ((LoginFailure)loginResult).Errors)
        {
            Log.LogInfo(error);
        }

        return loginResult;
    }

    public APOptions GetOptions() => _options;

    public void SubmitGrass(string grassName)
    {
        if (!_options.GrassSanity) return;
        APLocation location = allLocations.FirstOrDefault(m => m.name == grassName);
        if (location == null) return;
        SubmitLocationById(location.id);
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

    void ResetAchievementCache(object sender, ModSaveGame modSaveGame);
    void UnlockAchievement(string achievementName);
    void SubmitLocationById(long id);

    Task<bool> TryEnableAsync(ConnectionInfo connectionSettings, ILocationQueue locationQueue,
        IItemQueue itemQueue);

    void Disconnect(object sender, EventArgs e);
    APOptions GetOptions();
    void SubmitGrass(string grassName);
}