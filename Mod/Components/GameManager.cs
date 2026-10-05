using System;
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

    protected override void OnEnable()
    {
        base.OnEnable();
        Instance = this;
        TfwrConfig.SetupConfig(Plugin.Instance.Config);
    }

    public override void OnInject() => ItemService.Initialize();

    public override void Initialize()
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

        APManager.Instance?.ItemQueue.ItemReady += OnItemReceived;
        OnDisabled += () => APManager.Instance?.ItemQueue.ItemReady -= OnItemReceived;

        APManager.Instance?.APService.OptionsLoaded += OnOptionsLoaded;
        OnDisabled += () => APManager.Instance?.APService.OptionsLoaded -= OnOptionsLoaded;
    }

    private static void OnOptionsLoaded(object sender, APOptions options) =>
        ResourceManagerPatch.ManipulateCropCosts(options.CropCosts);
    
    /// <summary>Add the connection details to the config</summary>
    private void OnConnectionAttempt(object _, ConnectionInfo info) => TfwrConfig.ConnectionInfo = info;

    /// <summary>Save the config on success</summary>
    private void OnConnectionResult(object _, LoginResult result)
    {
        if (result.Successful) TfwrConfig.Save();
    }

    private void OnAchievementUnlocked(object sender, string achievement) => UnlockHat(achievement);

    // Disable the interprocess communication if the mod is loaded. Sorry, no tapping here.
    private static void OnGameLoaded(object sender, ModSaveGame saveGame)
    {
        IpcPatch.SetRunning(saveGame is not null);
        if (saveGame?.Options?.CropCosts is not null)
            ResourceManagerPatch.ManipulateCropCosts(saveGame.Options.CropCosts);
    }

    private static void OnGoalEvent(object sender, GoalEvent goalEvent)
    {
        switch (goalEvent.GoalType)
        {
            case GoalType.Achievement:
                APManager.Instance?.APService.UnlockAchievement(goalEvent.Name);
                break;
            case GoalType.Statistic:
                APManager.Instance?.APService.SubmitLocationById(goalEvent.Id);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(goalEvent.GoalType), "Unknown GoalType");
        }
    }

    public void OnItemReceived(object sender, ItemReceivedData itemData)
    {
        string itemName = itemData.ItemName;
        int itemsReceived = itemData.ItemCount;
        GameService.Result result = GameService.CanGivePlayerItem(itemName, itemsReceived);
        switch (result)
        {
            case Service.GameService.Result.ModNotInitialized:
                return;
            case Service.GameService.Result.ItemAlreadyReceived:
                return;
            case Service.GameService.Result.ProcessItem:
                ItemProcessed item = GivePlayerItem(itemName);
                if (item.NotificationNeeded)
                    RaiseNewItemReceived(new Notification { Title = "You received an item!", Message = itemName });
                if (!item.Processed) return;
                GameService.IncrementItemCount();
                APManager.Instance?.ItemQueue.IncrementItemProcessed();
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private class ItemProcessed(bool processed, bool notificationNeeded)
    {
        public bool Processed { get; set; } = processed;
        public bool NotificationNeeded { get; set; } = notificationNeeded;
    }

    private ItemProcessed GivePlayerItem(string itemName)
    {
        try
        {
            if (ItemService.CanProcess(itemName))
            {
                return ItemService.Process(itemName)
                    ? new ItemProcessed(true, true)
                    : new ItemProcessed(false, false);
            }

            _log.LogError($"Unable to process {itemName}. Not registered?");
        }
        catch (Exception e)
        {
            _log.LogInformation("GivePlayerItem");
            _log.LogError(e.Message);
            if (e.InnerException != null)
            {
                _log.LogError(e.InnerException.Message);
            }

            _log.LogInformation(e.StackTrace);
        }

        return new ItemProcessed(false, false);
    }

    public static string DefaultSaveName() => OptionHolder.GetString("activeSave", "Save0");

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
        GameService.SaveProgress(
            GoalManager.Instance?.UserStatsSave(),
            APManager.Instance?.APService.GetOptions(),
            DefaultSaveName());
    });
}