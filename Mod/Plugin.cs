using System;
using BepInEx;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Functions;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Service;
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

    public static ILoggerFactory LogFactory;
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
            LogFactory = new LoggerFactory([new PluginLoggerProvider()]);
            Log = LogFactory.CreateLogger<Plugin>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[TFWRAP] Failed to create Log");
            Console.WriteLine($"Exception Message {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return;
        }
        
        InjectionService.AutomaticRegistration();
        
        // Create Managers
        MainGameObject = new GameObject("Archipelago");
        InjectionService.CreateAndInjectComponent<UIManager>();
        InjectionService.CreateAndInjectComponent<APManager>();
        InjectionService.CreateAndInjectComponent<GoalManager>();
        InjectionService.CreateAndInjectComponent<GameManager>();

        // Apply game patches
        try
        {
            DroneLib.Main.PatchAll();
            _harmony.PatchAll();
        }
        catch (Exception e)
        {
            Log.LogError($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load properly! Harmony patch issues.", e);
            return;
        }

        Log.LogInformation($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! Running version {MyPluginInfo.PLUGIN_VERSION}");
    }

    private void Start()
    {
        DroneLib.Function.Registration.RegisterFunction(new FastFlip());
        DroneLib.Function.Registration.RegisterFunction(new Teleport());
        DroneLib.Item.Registration.RegisterItem(new FakeAPItem());
    }
}