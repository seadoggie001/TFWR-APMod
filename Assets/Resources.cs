using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.seadoggie.TFWRArchipelago.Assets;

public static class Resources
{
    private static ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("Resources");
    private const string BundleName = "com.seadoggie.TFWRArchipelago.Assets.archipelago";

    public static Sprite Archipelago
    {
        get
        {
            field ??= LoadAsset<Sprite>("archipelago");
            return field;
        }
    }

    public static AssetBundle Bundle
    {
        get {
            field ??= LoadBundle(BundleName);
            return field;
        }
    }

    public static PanelSettings PanelSettings
    {
        get
        {
            field ??= LoadAsset<PanelSettings>("DefaultPanelSettings");
            return field;
        }
    }
    
    public static ThemeStyleSheet ThemeStyleSheet
    {
        get
        {
            field ??= LoadAsset<ThemeStyleSheet>("DefaultThemeStyleSheet");
            return field;
        }
    }
    
    public static StyleSheet AchievementStyleSheet
    {
        get
        {
            field ??= LoadAsset<StyleSheet>("Statistics");
            return field;
        }
    }

    private static AssetBundle LoadBundle(string bundleName)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        Stream stream = assembly.GetManifestResourceStream(bundleName);
        if (stream == null)
        {
            Log.LogError($"No bundle named '{bundleName}'.");
        }
        else
        {
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            if (bundle != null) return bundle;
            Log.LogError($"Bundle not loaded '{bundleName}'.");
        }
        return null;
    }
    
    private static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
    {
        if(Bundle == null) Log.LogError($"No bundle named '{BundleName}'.");
        T asset = Bundle?.LoadAsset<T>(assetName);
        if (asset == null) Log.LogError($"Failed to load {typeof(T)}: Asset {assetName} was not found");
        return asset;
    }
}