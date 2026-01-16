using System.Collections.ObjectModel;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Configuration;
using HarmonyLib;

namespace com.seadoggie.TFWRArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GameName = "The Farmer Was Replaced";
    public static Plugin Instance { get; private set; } = null!;
    public static ManualLogSource Log { get; private set; } = null!;
    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);
    public readonly APConnectionConfig ConnectionSettings = new();
    
    private static ArchipelagoSession Session { get; set; } = null;

    /// <summary>
    /// Should any of the mod's features be running?
    /// </summary>
    public bool Enabled { get; set; } = false;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        // Plugin startup logic
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        _harmony.PatchAll();
        
        ConnectionSettings.SetupConfig(Config);
    }

    // ToDo: Return a Login object? Use the SlotData?
    public async Task<bool> TryEnableAsync()
    {
        // Only attempt to connect if not connected already
        if (Session?.Socket?.Connected ?? false)
        {
            Enabled = true;
            return true;
        }
        
        // Create the session
        Session = ArchipelagoSessionFactory.CreateSession(ConnectionSettings.Url, ConnectionSettings.Port);
        
        //
        Session.Items.ItemReceived += OnItemReceived;
        
        RoomInfoPacket roomInfoPacket = await ConnectAsync();
        if (roomInfoPacket == null)
        {
            Log.LogError($"Failed to connect to room. Connection Details: {{URL: {ConnectionSettings.Url}:{ConnectionSettings.Port}}}");
            return false;
        }

        LoginResult loginResult = await LoginAsync();
        if (loginResult.Successful)
        {
            Log.LogInfo($"Successfully logged in.");
            Enabled = true;
            Session.Locations.CheckedLocationsUpdated += OnCheckedLocationsUpdated;
            return true;
        }
        Log.LogError(
            $"Failed to connect. Connection Details: {{URL: {ConnectionSettings.Url}:{ConnectionSettings.Port}, " +
            $"Username: {ConnectionSettings.Username}, " +
            $"Password? {!string.IsNullOrWhiteSpace(ConnectionSettings.Password)}}}");
        return false;
    }

    private void OnCheckedLocationsUpdated(ReadOnlyCollection<long> newCheckedLocations)
    {
        foreach (long newCheckedLocation in newCheckedLocations)
        {
            string locationName = Session.Locations.GetLocationNameFromId(newCheckedLocation, GameName);
            
            // Give the player the location... somehow...
            // Maybe I'll log it for now to see what's here
            Log.LogInfo($"Found location: {locationName}");
        }
    }

    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        string itemReceivedName = helper.PeekItem().ItemDisplayName;
        Log.LogInfo($"Found item: {itemReceivedName}");
        // Do stuff to get the item (!)
        GivePlayerItem(itemReceivedName);
        helper.DequeueItem();
    }

    private void GivePlayerItem(string itemName)
    {
        string unlockName = Unlocks.Item(itemName);
        Farm farm = MainSim.Inst.storedSim.farm; 
        int count = farm.NumUnlocked(unlockName);
        // Hopefully we do not allow for "too many" items... but I think the game handles that internally
        farm.Unlock(unlockName, count + 1);
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
            Log.LogError(e.Message);
            Log.LogError(e.StackTrace);
            if (e.InnerException == null) return null;
            Log.LogError(e.GetBaseException().Message);
            Log.LogError(e.InnerException.StackTrace);
            return null;
        }
        Log.LogInfo($"[RoomInfo] Seed: {roomInfoPacket.SeedName}");
        Log.LogInfo($"[RoomInfo] Games: {string.Join(",", roomInfoPacket.Games)}");
        Log.LogInfo($"[RoomInfo] Tags: {string.Join(",", roomInfoPacket.Tags)}");
        Log.LogInfo($"[RoomInfo] Version: {roomInfoPacket.GeneratorVersion.ToVersion()}");
        
        return roomInfoPacket;
    }

    private async Task<LoginResult> LoginAsync()
    {
        LoginResult loginResult;
        try
        {
            loginResult = await Session.LoginAsync(
                "The Farmer Was Replaced",
                ConnectionSettings.Username,
                ItemsHandlingFlags.AllItems,
                Version.Parse("0.6.4"),
                [],
                null,
                ConnectionSettings.Password
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
}