using System.Reflection;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago;

public static class Resources
{
    private const string BundleName = "com.seadoggie.TFWRArchipelago.Resources.archipelago";
    
    private static AssetBundle _bundle;
    private static Sprite _archipelago;
    
    public static AssetBundle Bundle()
    {
        _bundle ??= LoadBundle(BundleName);
        return _bundle;
    }
    
    public static Sprite Archipelago()
    {
        _archipelago ??= LoadAsset<Sprite>("archipelago");
        return _archipelago;
    }

    private static AssetBundle LoadBundle(string bundleName)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        Stream stream = assembly.GetManifestResourceStream(bundleName);
        if (stream == null) throw new NullReferenceException($"No bundle named '{bundleName}'.");
            
        AssetBundle bundle = AssetBundle.LoadFromStream(stream);
        if (bundle == null) throw new NullReferenceException($"Bundle not loaded '{bundleName}'.");
        return bundle;
    }
    
    private static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
    {
        try
        {
            T asset = Bundle().LoadAsset<T>(assetName);
            return asset == null ? throw new NullReferenceException($"No asset named '{assetName}'.") : asset;
        }
        catch (Exception e)
        {
            Plugin.LogError("Failed to load asset", e);
            return null;
        }
    }
}