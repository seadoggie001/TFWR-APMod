using BepInEx;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GameName = "The Farmer Was Replaced";
    public static Plugin Instance { get; private set; } = null!;
    public static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.Main");
    
    public GameObject MainGameObject { get; private set; }
    
    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    /// <summary>
    /// Should any of the mod's features be running?
    /// </summary>
    public bool Enabled { get; set; }

    private void Awake()
    {
        Instance = this;

        // Create Managers
        MainGameObject = new GameObject("Archipelago");
        MainGameObject.AddComponent<UIManager>();
        MainGameObject.AddComponent<APManager>();
        MainGameObject.AddComponent<GoalManager>();
        MainGameObject.AddComponent<GameManager>();
        
        // Apply game patches
        try
        {
            _harmony.PatchAll();
        }
        catch (Exception e)
        {
            Log.LogException($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load properly! Harmony patch issues.", e);
            return;
        }
        
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! Running v{MyPluginInfo.PLUGIN_VERSION}");
    }
}