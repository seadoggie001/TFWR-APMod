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

        APManager.Instance?.LocationQueue.APLocationGiven += OnAPLocationGiven;
        OnDisabled += () => APManager.Instance?.LocationQueue.APLocationGiven -= OnAPLocationGiven;

        GameService.GameLoaded += OnGameLoaded;
        OnDisabled += () => GameService.GameLoaded -= OnGameLoaded;
        
        GameService.Load();
    }

    // Disable the interprocess communication if the mod is loaded. Sorry, no tapping here.
    private static void OnGameLoaded(object sender, ModSaveGame e) => IpcPatch.SetRunning(e is not null);

    private static void OnAPLocationGiven(object sender, APLocation apLocation)
    {
        try
        {
            if (apLocation.achievement == null) return;
            // Find the farm
            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                Log.LogException($"Failed to find Farm. Location name: {apLocation.name}");
                return;
            }

            int count = farm.NumUnlocked(apLocation.achievement);

            farm.Unlock(apLocation.achievement, count + 1);
        }
        catch (Exception e)
        {
            Log.LogError(e.Message);
            if (e.InnerException != null)
            {
                Log.LogError(e.InnerException.Message);
            }

            Log.LogInfo(e.StackTrace);
        }
    }

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
            case Service.GameService.Result.ItemAlreadyRecevied:
                return true;
            case Service.GameService.Result.ProcessItem:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (itemName != "RickRoll") return GivePlayerItem(itemName);

        RickRoll();
        return true;
    }

    private static bool GivePlayerItem(string itemName)
    {
        try
        {
            string unlockName = Unlocks.ItemToUnlock(itemName);
            if (string.IsNullOrWhiteSpace(unlockName))
            {
                Log.LogWarning($"Failed to find unlock item: {itemName}");
                return true; // Don't keep it in the queue
            }

            Farm farm = MainSimPatch.GetMainSim()?.farm;
            if (farm is null)
            {
                Log.LogException("[GivePlayerItem] Failed to find Farm.");
                return false;
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
            return true;
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
            return false;
        }
    }

    public static string DefaultSaveName() => OptionHolder.GetString("activeSave", "Save0");

    public void RickRoll() => Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

    public void UnlockHat(string hatName)
    {
        if (hatName == null) return;
        HatSO hat = ResourceManager.GetHat(hatName);
        if (hat is null) Log.LogError($"Failed to find hat: {hatName}");
        MainSim.Inst.UnlockHat(hat);
    }

    public void Load() => GameService.Load();

    public void SaveProgress() => GameService.SaveProgress();
}