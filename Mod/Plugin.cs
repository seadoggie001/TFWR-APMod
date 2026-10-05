using System;
using BepInEx;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Functions;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GameName = "The Farmer Was Replaced";
    public static Plugin Instance { get; private set; } = null!;
    public GameObject MainGameObject { get; private set; }

    private static ILoggerFactory _logFactory;
    public static ILogger<Plugin> Log;

    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    /// <summary>
    /// Should any of the mod's features be running?
    /// </summary>
    public bool Enabled { get; set; }

    private void Awake()
    {
        Instance = this;
        
        try
        {
            _logFactory = new LoggerFactory([new PluginLoggerProvider()]);
            Log = new Logger<Plugin>(_logFactory);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[TFWRAP] Failed to create Log");
            Console.WriteLine($"Exception Message {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return;
        }

        InjectionService.LoggerFactory = _logFactory;
        InjectionService.Log = _logFactory.CreateLogger<InjectionService>();
        InjectionService.AutomaticRegistration();
        
        // Create Managers
        MainGameObject = new GameObject("Archipelago");
        BepInExHelper.CreateAndInjectComponent<UIManager>();
        BepInExHelper.CreateAndInjectComponent<APManager>();
        BepInExHelper.CreateAndInjectComponent<GoalManager>();
        BepInExHelper.CreateAndInjectComponent<GameManager>();

        // Apply game patches
        try
        {
            DroneLib.Main.PatchAll();
            _harmony.PatchAll();
        }
        catch (Exception e)
        {
            Log.LogError(e,$"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load properly! Harmony patch issues.");
            return;
        }

        Log.LogInformation($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! Running version {MyPluginInfo.PLUGIN_VERSION}");
    }

    private void Start()
    {
        DroneLib.Function.Registration.RegisterFunction(new DoubleFlip());
        DroneLib.Function.Registration.RegisterFunction(new Teleport());
        DroneLib.Item.Registration.RegisterItem(new FakeAPItem());
    }
}