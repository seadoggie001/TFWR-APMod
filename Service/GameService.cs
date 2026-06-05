using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Service;

public class GameService : IGameService
{
    public const string FileName = "tfwrap.json";
    [CanBeNull] public ModSaveGame ModSaveGame;

    public event EventHandler<ModSaveGame> GameLoaded;
    public event EventHandler<EventArgs> PreLoadGame;
    public event EventHandler<bool> MenuOpen;
    public event EventHandler<Notification> NewItemReceived;

    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.GameSvc");
    public static string GetFilePath(string saveName) => Path.Combine(Saver.GetPathOfSaveDirectory(saveName), FileName);

    public void SaveProgress()
    {
        try
        {
            ModSaveGame?.Statistics = GoalManager.Instance.UserStatsSave();
            string json = JsonUtility.ToJson(ModSaveGame);
            string filePath = GetFilePath(GameManager.DefaultSaveName());
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            Log.LogException("Failed to save progress", e);
        }
    }

    public void Load()
    {
        PreLoadGame?.Invoke(this, EventArgs.Empty);
        ModSaveGame modSaveGame = new();
        // Try to load even if the plugin isn't enabled
        try
        {
            string filePath = GetFilePath(GameManager.DefaultSaveName());
            if (!File.Exists(filePath))
            {
                Plugin.Instance.Enabled = false;
                return;
            }

            string json = File.ReadAllText(filePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                modSaveGame = JsonUtility.FromJson<ModSaveGame>(json);
            }
        }
        catch (Exception e)
        {
            Log.LogException("Failed to load data", e);
            return;
        }

        if (GameManager.Instance is null)
        {
            Plugin.Log.LogError("Save game was not loaded!");
            return;
        }

        Plugin.Log.LogInfo("Save game was loaded");

        Plugin.Instance.Enabled = true;
        ModSaveGame = modSaveGame;
        GameLoaded?.Invoke(this, ModSaveGame);
    }

    public Result CanGivePlayerItem(string itemName, int itemsReceived)
    {
        if (ModSaveGame is null)
        {
            Log.LogError($"Failed to give {itemName} because the ModSaveGame isn't loaded yet");
            return Result.ModNotInitialized;
        }

        // If we've previously received this item
        if (itemsReceived < ModSaveGame.ItemsReceived) return Result.ItemAlreadyReceived;

        Log.LogInfo("Only unlocked " + ModSaveGame.ItemsReceived);
        ModSaveGame.ItemsReceived += 1;

        return APTrapItems.AllTrapItems.Contains(itemName)
            ? Result.ItsATrap
            : Result.ProcessItem;
    }

    public void RaiseMenuOpen(bool open) => MenuOpen?.Invoke(this, open);

    public void RaiseNewItemReceived(Notification notification) => NewItemReceived?.Invoke(this, notification);

    public void RaiseGrassSanity(Vector2Int position)
    {
        // idk, check if it needs to be submitted?
        if(ModSaveGame?.Grass.Contains(position) ?? true) return;
        string locName = $"Grass ({position.x}, {position.y})";
        APLocation location = APManager.Instance?.GetLocations().FirstOrDefault(m => m.name == locName);
        if (location is null)
        {
            Log.LogError($"Could not find location {locName}");
            return;
        }

        APManager.Instance?.APService.SubmitLocationById(location.id);
        ModSaveGame.Grass.Add(position);
    }

    public enum Result
    {
        ModNotInitialized,
        ItemAlreadyReceived,
        ProcessItem,
        ItsATrap,
    }
}

public interface IGameService
{
    /// <summary>
    /// Fired before a new game is loaded 
    /// </summary>
    event EventHandler<EventArgs> PreLoadGame;

    /// <summary>
    /// Fired after a game is loaded
    /// </summary>
    event EventHandler<ModSaveGame> GameLoaded;

    event EventHandler<bool> MenuOpen;

    event EventHandler<Notification> NewItemReceived;

    /// <summary>
    /// Saves the current stats of the game
    /// </summary>
    void SaveProgress();

    void Load();
    GameService.Result CanGivePlayerItem(string itemName, int itemsReceived);
    void RaiseMenuOpen(bool open);
    void RaiseNewItemReceived(Notification notification);
    void RaiseGrassSanity(Vector2Int position);
}