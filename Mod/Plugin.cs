using BepInEx;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Functions;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using UnityEngine;
using ILogger = com.seadoggie.TFWRArchipelago.Logging.ILogger;

namespace com.seadoggie.TFWRArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GameName = "The Farmer Was Replaced";
    public static Plugin Instance { get; private set; } = null!;
    public GameObject MainGameObject { get; private set; }

    private static readonly ILogService LogService = new LogService();
    public static readonly ILogger Log = LogService.CreateLog("TFWRAP.Main");

    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    /// <summary>
    /// Should any of the mod's features be running?
    /// </summary>
    public bool Enabled { get; set; }

    private void Awake()
    {
        Instance = this;

        // Use field dependency injection (for testing and my sanity)
        InjectionService.Register(typeof(ILogService), LogService);
        InjectionService.Register(typeof(ILogger), Log);
        InjectionService.Register(typeof(IEnabledService), new EnabledService());

        // Create Managers
        MainGameObject = new GameObject("Archipelago");
        MainGameObject.AddComponent<UIManager>();
        MainGameObject.AddComponent<APManager>();
        MainGameObject.AddComponent<GoalManager>();
        MainGameObject.AddComponent<GameManager>();

        // Apply game patches
        try
        {
            DroneLib.Plugin.PatchAll();
            _harmony.PatchAll();
        }
        catch (Exception e)
        {
            Log.LogException($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load properly! Harmony patch issues.", e);
            return;
        }

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! Running version {MyPluginInfo.PLUGIN_VERSION}");
    }

    private void Start()
    {
        DroneLib.Functions.Registration.RegisterFunction(new FastFlip());
        DroneLib.Functions.Registration.RegisterFunction(new Teleport());
        DroneLib.Item.Registration.RegisterItem(new FakeAPItem());
    }
}