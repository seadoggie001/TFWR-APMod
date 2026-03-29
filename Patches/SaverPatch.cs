using HarmonyLib;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Patches;

[HarmonyPatch(typeof(Saver))]
public static class SaverPatch
{
    public const string FileName = "tfwrap.json";
    
    /// <summary>
    /// Patch in code to load data for the mod
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Saver.Load), typeof(MainSim))]
    public static void Load()
    {
        // ToDo: Forcibly shut the connection to AP here (!)
        ModSaveGame modSaveGame = new();
        // Try to load even if the plugin isn't enabled
        try
        {
            string filePath = GetFilePath(SaveName());
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    modSaveGame = JsonUtility.FromJson<ModSaveGame>(json);
                    UserStats.Load(modSaveGame.Statistics);
                }
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("Failed to load data", e);
        }
        Plugin.Instance.SaveGame = modSaveGame;
    }

    /// <summary>
    /// Patch in code to save mod data to the file system
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Saver.SaveProgress), typeof(MainSim))]
    public static void SaveProgress()
    {
        try
        {
            Plugin.Instance.SaveGame.Statistics = UserStats.Save();
            string json = JsonUtility.ToJson((object)Plugin.Instance.SaveGame);
            string filePath = GetFilePath(SaveName());
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            Plugin.LogError("Failed to save progress!", e);
        }
    }

    public static string SaveName() => OptionHolder.GetString("activeSave", "Save0");
    
    public static string GetFilePath(string saveName) => Path.Combine(Saver.GetPathOfSaveDirectory(saveName), FileName);
}