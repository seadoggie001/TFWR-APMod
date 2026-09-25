using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using com.seadoggie.TFWRArchipelago.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Service;

//ToDo: Replace UnityEngine.JsonUtility and UnityEngine.Vector2Int to allow testing of the GameService
[Injectable(typeof(IGameService))]
public class GameService : IGameService
{
    private const string FileName = "tfwrap.json";
    [CanBeNull] private ModSaveGame _modSaveGame;
    [Log] private readonly ILogger<GameService> _log = null!;

    public event EventHandler<ModSaveGame> GameLoaded;
    public event EventHandler<EventArgs> PreLoadGame;
    public event EventHandler<bool> MenuOpen;
    public event EventHandler<string> GrassSanity;

    public static string GetFilePath(string saveName) => Path.Combine(Saver.GetPathOfSaveDirectory(saveName), FileName);

    /// <summary>
    /// Saves the current stats of the game
    /// </summary>
    /// <param name="statistics"></param>
    /// <param name="fileName"></param>
    public void SaveProgress(List<KeyValuePair<string, double>> statistics, string fileName)
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
            _log.LogException(nameof(SaveProgress), ex);
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
                GameLoaded?.Invoke(this, null);
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
            _log.LogException("Failed to load data", e);
        }
    }

    public Result CanGivePlayerItem(string itemName, int itemsReceived)
    {
        if (_modSaveGame is not null)
            // Check if we've previously received this item
            return itemsReceived < _modSaveGame.ItemsReceived
                ? Result.ItemAlreadyReceived
                : Result.ProcessItem;

        _log.LogError($"Failed to give {itemName} because the ModSaveGame isn't loaded yet");
        return Result.ModNotInitialized;
    }

    /// <summary>
    /// Increment the number of items received
    /// </summary>
    public void IncrementItemCount() => _modSaveGame?.ItemsReceived += 1;

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
                _log.LogException(nameof(RaiseMenuOpen), ex);
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
                GrassSanity?.Invoke(this, locName);
            }
            catch (Exception ex)
            {
                _log.LogException(nameof(RaiseGrassSanity), ex);
            }
        });
    }

    public enum Result
    {
        ModNotInitialized,
        ItemAlreadyReceived,
        ProcessItem,
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

    /// <inheritdoc cref="GameService.SaveProgress(List{KeyValuePair{string, double}}, string)" />
    void SaveProgress(List<KeyValuePair<string, double>> statistics, string fileName);

    /// <inheritdoc cref="GameService.Load(string)" />
    void Load(string fileName);

    GameService.Result CanGivePlayerItem(string itemName, int itemsReceived);
    void IncrementItemCount();
    void RaiseMenuOpen(bool open);
    void RaiseGrassSanity(Vector2Int position);
}