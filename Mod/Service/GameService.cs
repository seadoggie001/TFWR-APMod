using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Service;

public class GameService : IGameService
{
    private const string FileName = "tfwrap.json";
    [CanBeNull] private ModSaveGame _modSaveGame;

    public event EventHandler<ModSaveGame> GameLoaded;
    public event EventHandler<EventArgs> PreLoadGame;
    public event EventHandler<bool> MenuOpen;
    public event EventHandler<string> GrassSanity;

    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.GameSvc");
    public static string GetFilePath(string saveName) => Path.Combine(Saver.GetPathOfSaveDirectory(saveName), FileName);

    /// <summary>
    /// Saves the current stats of the game
    /// </summary>
    /// <param name="statistics"></param>
    /// <param name="fileName"></param>
    public void SaveProgress(List<Pair<string, double>> statistics, string fileName)
    {
        try
        {
            _modSaveGame?.Statistics = statistics;
            string json = JsonUtility.ToJson(_modSaveGame);
            string filePath = GetFilePath(fileName);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Log.LogException(nameof(SaveProgress), ex);
        }
    }

    public void Load(string fileName)
    {
        // Try to load even if the plugin isn't enabled
        try
        {
            PreLoadGame?.Invoke(this, EventArgs.Empty);
            ModSaveGame modSaveGame = new();
            string filePath = GetFilePath(fileName);
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

            Plugin.Log.LogInfo("Save game was loaded");

            Plugin.Instance.Enabled = true;
            _modSaveGame = modSaveGame;
            GameLoaded?.Invoke(this, _modSaveGame);
        }
        catch (Exception e)
        {
            Log.LogException("Failed to load data", e);
        }
    }

    public Result CanGivePlayerItem(string itemName, int itemsReceived)
    {
        if (_modSaveGame is null)
        {
            Log.LogError($"Failed to give {itemName} because the ModSaveGame isn't loaded yet");
            return Result.ModNotInitialized;
        }

        // If we've previously received this item
        if (itemsReceived < _modSaveGame.ItemsReceived) return Result.ItemAlreadyReceived;

        Log.LogInfo("Only unlocked " + _modSaveGame.ItemsReceived);
        _modSaveGame.ItemsReceived += 1;

        return APTrapItems.AllTrapItems.Contains(itemName)
            ? Result.ItsATrap
            : Result.ProcessItem;
    }

    public void RaiseMenuOpen(bool open)
    {
        Task.Run(() =>
        {
            try
            {
                MenuOpen?.Invoke(this, open);
            }
            catch (Exception ex)
            {
                Log.LogException(nameof(RaiseMenuOpen), ex);
            }
        });
    }

    public void RaiseGrassSanity(Vector2Int position)
    {
        Task.Run(() =>
        {
            try
            {
                if (_modSaveGame is null) return;
                // Check if it needs to be submitted
                if (!_modSaveGame.Grass.Add(position)) return;

                string locName = $"Grass ({position.x}, {position.y})";
                Log.LogInfo($"Grass insanity was triggered. Name: {locName}");
                GrassSanity?.Invoke(this, locName);
            }
            catch (Exception ex)
            {
                Log.LogException(nameof(RaiseGrassSanity), ex);
            }
        });
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
    
    event EventHandler<string> GrassSanity;

    /// <inheritdoc cref="GameService.SaveProgress(List{Pair{string, double}}, string)" />
    void SaveProgress(List<Pair<string, double>> statistics, string fileName);
    
    /// <inheritdoc cref="GameService.Load(string)" />
    void Load(string fileName);
    
    GameService.Result CanGivePlayerItem(string itemName, int itemsReceived);
    void RaiseMenuOpen(bool open);
    void RaiseGrassSanity(Vector2Int position);
}