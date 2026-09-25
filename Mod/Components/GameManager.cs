using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using com.seadoggie.TFWRArchipelago.Configuration;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using com.seadoggie.TFWRArchipelago.Service;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Components;

public class GameManager : BaseComponent
{
    [CanBeNull] public static GameManager Instance;
    [Log] private readonly ILogger<GameManager> _log = null!;
    [ModInject] public readonly IGameService GameService = null!;
    [ModInject] public readonly IItemService ItemService = null!;

    public readonly TfwrConfig TfwrConfig = new();

    public event EventHandler<Notification> NewItemReceived;

    private Dictionary<string, Action<Simulation>> _fillerHandlers;

    protected override void OnEnable()
    {
        base.OnEnable();
        Instance = this;
        TfwrConfig.SetupConfig(Plugin.Instance.Config);
        _fillerHandlers = new Dictionary<string, Action<Simulation>>
        {
            { APItem.FreeHay, Filler_FreeHay }
        };
    }

    private void Start()
    {
        GoalManager.Instance?.StatsService.GoalEvent += OnGoalEvent;
        OnDisabled += () => GoalManager.Instance?.StatsService.GoalEvent -= OnGoalEvent;

        GameService.GameLoaded += OnGameLoaded;
        OnDisabled += () => GameService.GameLoaded -= OnGameLoaded;

        APManager.Instance?.APService.AchievementUnlocked += OnAchievementUnlocked;
        OnDisabled += () => APManager.Instance?.APService.AchievementUnlocked -= OnAchievementUnlocked;

        APManager.Instance?.APService.ConnectionResult += OnConnectionResult;
        OnDisabled += () => APManager.Instance?.APService.ConnectionResult -= OnConnectionResult;

        UIManager.Instance?.settingsGUI.ConnectionAttemptEvent += OnConnectionAttempt;
        OnDisabled += () => UIManager.Instance?.settingsGUI.ConnectionAttemptEvent -= OnConnectionAttempt;
    }

    /// <summary>Add the connection details to the config</summary>
    private void OnConnectionAttempt(object _, ConnectionInfo e) => TfwrConfig.ConnectionInfo = e;

    /// <summary>Save the config on success</summary>
    private void OnConnectionResult(object _, LoginResult e)
    {
        if (!e.Successful) return;
        TfwrConfig.Save();
    }

    private void OnAchievementUnlocked(object sender, string achievement) => UnlockHat(achievement);

    // Disable the interprocess communication if the mod is loaded. Sorry, no tapping here.
    private static void OnGameLoaded(object sender, ModSaveGame e) => IpcPatch.SetRunning(e is not null);

    private static void OnGoalEvent(object sender, GoalEvent e)
    {
        if (e.GoalType == GoalType.Achievement)
        {
            APManager.Instance?.APService.UnlockAchievement(e.Name);
        }
        else
        {
            APManager.Instance?.APService.SubmitLocationById(e.Id);
        }
    }

    public bool GiveItem(string itemName, int itemsReceived)
    {
        GameService.Result result = GameService.CanGivePlayerItem(itemName, itemsReceived);
        switch (result)
        {
            case Service.GameService.Result.ModNotInitialized:
                return false;
            case Service.GameService.Result.ItemAlreadyReceived:
                return true;
            case Service.GameService.Result.ProcessItem:
                ItemProcessed item = GivePlayerItem(itemName);
                if (item.given)
                    RaiseNewItemReceived(new Notification { Title = "You received an item!", Message = itemName });
                return item.processed;
            case Service.GameService.Result.ItsATrap:
                ProcessTrapItem(itemName);
                RaiseNewItemReceived(new Notification { Title = "It's a trap!", Message = itemName });
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ProcessTrapItem(string itemName)
    {
        switch (itemName)
        {
            case APTrapItems.RickRoll:
                RickRoll();
                break;
            default:
                _log.LogError($"Unexpected trap name: {itemName}");
                break;
        }
    }

    private class ItemProcessed(bool processed, bool given)
    {
        public bool processed { get; set; } = processed;
        public bool given { get; set; } = given;
    }
    
    private ItemProcessed GivePlayerItem(string itemName)
    {
        try
        {
            if (_fillerHandlers.TryGetValue(itemName, out Action<Simulation> action))
            {
                action.Invoke(MainSimPatch.GetMainSim());
                return new ItemProcessed(true, true);
            }
            
            string unlockName = Unlocks.ItemToUnlock(itemName);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                _log.LogWarning($"Failed to find unlock item: {itemName}");
                // Don't keep it in the queue
                return new ItemProcessed(true, false);
            }

            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                _log.LogError("[GivePlayerItem] Failed to find Farm.");
                return new ItemProcessed(false, false);
            }

            int count = farm.NumUnlocked(unlockName);
            _log.LogInfo($"Found {count} unlocked {unlockName}");

            // Hopefully we do not allow for "too many" items... but I think the game handles that internally
            farm.Unlock(unlockName, count + 1);
            UnlockSO unlock = farm.GetUnlockOf(unlockName);
            foreach (string unlockItemName in unlock.unlocks)
            {
                _log.LogInfo("  - and unlocks " + unlockItemName);
            }

            farm.UnlockAllIn(unlock);
            return new ItemProcessed(true, true);
        }
        catch (Exception e)
        {
            _log.LogInfo("GivePlayerItem");
            _log.LogError(e.Message);
            if (e.InnerException != null)
            {
                _log.LogError(e.InnerException.Message);
            }

            _log.LogInfo(e.StackTrace);
            return new ItemProcessed(false, false);
        }
    }

    private void Filler_FreeHay(Simulation sim)
    {
        try
        {
            int? hayId = ResourceManager.GetAllItems().FirstOrDefault(m => m.itemName == "hay")?.itemId;
            if (hayId is null)
            {
                _log.LogError($"Failed to locate {APItem.FreeHay} itemId!");
                return;
            }

            double gifted = sim.farm.Items.GetNumber((int)hayId) * 0.2;
            sim.farm.Items.AddItem((int)hayId, Math.Floor(gifted));
            GoalManager.Instance?.RaiseStatEvent("hay", Math.Floor(gifted));
        }
        catch (Exception ex)
        {
            _log.LogException($"{nameof(Filler_FreeHay)}", ex);
        }
    }
    
    public static string DefaultSaveName() => OptionHolder.GetString("activeSave", "Save0");

    public void RickRoll() => Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

    public void RaiseNewItemReceived(Notification notification) => NewItemReceived?.Invoke(this, notification);

    public void UnlockHat(string achievement)
    {
        string hatName = achievement switch
        {
            Achievement.CauseARuntimeError => Model.Hat.TrafficCone.Resource,
            Achievement.StackOverflow => Model.Hat.TrafficConeStack.Resource,
            Achievement.HigherOrderProgramming => Model.Hat.Wizard.Resource,
            _ => null
        };
        if (hatName == null) return;

        HatSO hat = ResourceManager.GetHat(hatName);
        if (hat is null) _log.LogError($"Failed to find hat: {hatName}");

        MainSim.Inst.UnlockHat(hat);
    }

    public void Load() => Task.Run(() => { GameService.Load(DefaultSaveName()); });

    public void SaveProgress() => Task.Run(() =>
    {
        GameService.SaveProgress(GoalManager.Instance?.UserStatsSave(), DefaultSaveName());
    });
}