using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Configuration;
using com.seadoggie.TFWRArchipelago.Constants;
using com.seadoggie.TFWRArchipelago.Helpers;
using com.seadoggie.TFWRArchipelago.Patches;
using HarmonyLib;
using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GameName = "The Farmer Was Replaced";
    public static Plugin Instance { get; private set; } = null!;
    public static ManualLogSource Log { get; private set; } = null!;

    public const bool HarmonySkipFunction = false;

    public bool Loaded { get; private set; }= true;
    public ModSaveGame SaveGame { get; set; }
    
    public readonly APConnectionConfig ConnectionSettings = new();
    public readonly LocationHelper LocationHelper = new();
    public readonly ItemHelper ItemHelper = new();

    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);
    
    public ArchipelagoSession Session { get; set; }

    /// <summary>
    /// Should any of the mod's features be running?
    /// </summary>
    public bool Enabled { get; set; }

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        try
        {
            _harmony.PatchAll();
        }
        catch (Exception e)
        {
            LogError("Failed to load properly! Harmony patch issues.", e);
            Loaded = false;
            return;
        }
        
        ConnectionSettings.SetupConfig(Config);
        APLocation.Initialize();
        UserStats.Initialize();
        
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! Running v{MyPluginInfo.PLUGIN_VERSION}");
        
    }

    private void Update()
    {
        try
        {
            ItemHelper.Update();
            LocationHelper.Update();
        }
        catch (Exception e)
        {
            LogError("[Plugin.cs] Failed to update item list!", e);
        }
    }

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
        
        Session.Items.ItemReceived += ItemHelper.OnItemReceived;
        
        RoomInfoPacket roomInfoPacket = await ConnectAsync();
        if (roomInfoPacket == null)
        {
            Log.LogError($"Failed to connect to room. Connection Details: {{URL: {ConnectionSettings.Url}:{ConnectionSettings.Port}}}");
            return false;
        }
        Session.Locations.CheckedLocationsUpdated += LocationHelper.OnLocationsReceived;

        LoginResult loginResult = await LoginAsync();
        if (loginResult.Successful)
        {
            Log.LogInfo("Successfully logged in.");
            Enabled = true;
            return true;
        }
        Log.LogError(
            $"Failed to connect. Connection Details: {{URL: {ConnectionSettings.Url}:{ConnectionSettings.Port}, " +
            $"Username: {ConnectionSettings.Username}, " +
            $"Password? {!string.IsNullOrWhiteSpace(ConnectionSettings.Password)}}}");
        return false;
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

    public static void LogError(string message, [CanBeNull] Exception ex = null)
    {
        string exceptionMessage = $"{message}";
        if (ex != null) exceptionMessage = $" [Exception] Message: {ex.Message}\n{ex.StackTrace}";
        if (ex is { InnerException: not null }) exceptionMessage += $"\n\t[InnerException] Message: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
        Log.LogError(exceptionMessage);
    }

    /// <summary>
    /// Output some helpful information for debugging about unlocks
    /// </summary>
    public void WriteUnlocks()
    {
        string file = SaverPatch.GetFilePath(SaverPatch.SaveName()).Replace(SaverPatch.FileName, "unlocks.txt");
        File.WriteAllText(file, string.Join("\n", ResourceManager.GetAllUnlocks().Select(m =>
        {
            string text = $"[Unlock] {m.unlockName}";
            text += $"\n\tParent: {m.parentUnlock}";
            text += $"\n\tDescription: {m.description}";
            foreach (string unlock in m.unlocks)
            {
                text += $"\n\t\t- {unlock}";
            }
            return text;
        })));
    }

    /// <summary>
    /// Output some helpful information for debugging about items
    /// </summary>
    public void WriteItems()
    {
        string file = SaverPatch.GetFilePath(SaverPatch.SaveName()).Replace(SaverPatch.FileName, "items.txt");
        File.WriteAllText(file, string.Join("\n", ResourceManager.GetAllItems().Select(m =>
        {
            string text = $"[Item] {m.itemName}";
            text += $"\n\tTrackStats: {m.trackStats}";
            text += $"\n\tDescription: {m.description}";
            text += $"\n\tenabled: {m.enabled}";
            return text;
        })));
    }
}