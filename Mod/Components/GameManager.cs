using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Configuration;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Components;

public class GameManager : BaseComponent
{
    [CanBeNull] public static GameManager Instance;
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.GameMgr");

    public readonly TfwrConfig TfwrConfig = new();

    public readonly IGameService GameService = new GameService();

    public event EventHandler<Notification> NewItemReceived;

    protected override void OnEnable()
    {
        Instance = this;
        TfwrConfig.SetupConfig(Plugin.Instance.Config);
        Log.LogInfo("Game Manager Initialized");

        base.OnEnable();
    }

    private void Start()
    {
        GoalManager.Instance.GoalEvent += OnGoalEvent;
        OnDisabled += () => GoalManager.Instance.GoalEvent -= OnGoalEvent;

        GameService.GameLoaded += OnGameLoaded;
        OnDisabled += () => GameService.GameLoaded -= OnGameLoaded;
        
        APManager.Instance?.APService.AchievementUnlocked += OnAchievementUnlocked;
        OnDisabled += () => APManager.Instance?.APService.AchievementUnlocked -= OnAchievementUnlocked;
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
                Log.LogError($"Unexpected trap name: {itemName}");
                break;
        }
    }

    private class ItemProcessed(bool processed, bool given)
    {
        public bool processed { get; set; } = processed;
        public bool given { get; set; } = given;
    }

    private static ItemProcessed GivePlayerItem(string itemName)
    {
        try
        {
            string unlockName = Unlocks.ItemToUnlock(itemName);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                Log.LogWarning($"Failed to find unlock item: {itemName}");
                // Don't keep it in the queue
                return new ItemProcessed(true, false);
            }

            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                Log.LogError("[GivePlayerItem] Failed to find Farm.");
                return new ItemProcessed(false, false);
            }

            int count = farm.NumUnlocked(unlockName);
            Log.LogInfo($"Found {count} unlocked {unlockName}");

            // Hopefully we do not allow for "too many" items... but I think the game handles that internally
            farm.Unlock(unlockName, count + 1);
            UnlockSO unlock = farm.GetUnlockOf(unlockName);
            foreach (string unlockItemName in unlock.unlocks)
            {
                Log.LogInfo("  - and unlocks " + unlockItemName);
            }

            farm.UnlockAllIn(unlock);
            return new ItemProcessed(true, true);
        }
        catch (Exception e)
        {
            Log.LogInfo("GivePlayerItem");
            Log.LogError(e.Message);
            if (e.InnerException != null)
            {
                Log.LogError(e.InnerException.Message);
            }

            Log.LogInfo(e.StackTrace);
            return new ItemProcessed(false, false);
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
        if (hat is null) Log.LogError($"Failed to find hat: {hatName}");
        
        MainSim.Inst.UnlockHat(hat);
    }

    public void Load() => GameService.Load(DefaultSaveName());

    public void SaveProgress() => GameService.SaveProgress(GoalManager.Instance?.UserStatsSave(), DefaultSaveName());
}