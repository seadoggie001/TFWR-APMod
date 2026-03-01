using System.Reflection;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago;

public static class Resources
{
    private const string BundleName = "com.seadoggie.TFWRArchipelago.Resources.archipelago";
    
    private static AssetBundle _bundle;
    private static Sprite _archipelago;

    public static Sprite Archipelago
    {
        get
        {
            _archipelago ??= LoadAsset<Sprite>("archipelago");
            return _archipelago;
        }
    }

    public static AssetBundle Bundle
    {
        get {
            _bundle ??= LoadBundle(BundleName);
            return _bundle;
        }
    }

    private static AssetBundle LoadBundle(string bundleName)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        Stream stream = assembly.GetManifestResourceStream(bundleName);
        if (stream == null)
        {
            Plugin.Log.LogError($"No bundle named '{bundleName}'.");
        }
        else
        {
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            if (bundle != null) return bundle;
            Plugin.Log.LogError($"Bundle not loaded '{bundleName}'.");
        }
        return null;
    }
    
    private static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
    {
        T asset = Bundle?.LoadAsset<T>(assetName);
        if (asset == null) Plugin.Log.LogError($"Failed to load {typeof(T)}: Asset {assetName} was not found");
        return asset;
    }
}