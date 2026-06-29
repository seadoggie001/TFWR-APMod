using Archipelago.MultiClient.Net;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Components;

public class UIManager : BaseComponent
{
    [CanBeNull] public static UIManager Instance;
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.UIMgr");

    public StatisticsGUI statisticsGUI;
    public ArchipelagoSettingsGUI settingsGUI;
    public FloatingActionButton floatingActionButton;
    public NotificationPopup notificationPopup;

    protected override void OnEnable()
    {
        Instance = this;
        base.OnEnable();
    }

    public override void OnDisable()
    {
        Log.LogInfo($"OnDisable");
        OnDisabled();
        base.OnDisable();
    }

    private void Start()
    {
        GoalManager.Instance?.StatsService.GoalEvent += OnGoalEvent;
        OnDisabled += () => GoalManager.Instance?.StatsService.GoalEvent -= OnGoalEvent;
        GoalManager.Instance?.StatsService.StatTotalEvent += OnStatTotalEvent;
        OnDisabled += () => GoalManager.Instance?.StatsService.StatTotalEvent -= OnStatTotalEvent;

        APManager.Instance?.APService.APDisconnected += OnAPDisconnected;
        OnDisabled += () => APManager.Instance?.APService.APDisconnected -= OnAPDisconnected;
        APManager.Instance?.LocationQueue.APLocationGiven += OnAPLocationGiven;
        OnDisabled += () => APManager.Instance?.LocationQueue.APLocationGiven -= OnAPLocationGiven;
        APManager.Instance?.APService.ConnectionResult += OnConnectionResult;
        OnDisabled += () => APManager.Instance?.APService.ConnectionResult -= OnConnectionResult;

        GameManager.Instance?.GameService.GameLoaded += OnGameLoaded;
        OnDisabled += () => GameManager.Instance?.GameService.GameLoaded -= OnGameLoaded;
        GameManager.Instance?.GameService.MenuOpen += OnMenuOpen;
        OnDisabled += () => GameManager.Instance?.GameService.MenuOpen -= OnMenuOpen;
        GameManager.Instance?.NewItemReceived += NotifyItemReceived;
        OnDisabled += () => GameManager.Instance?.NewItemReceived -= NotifyItemReceived;

        statisticsGUI = new GameObject("StatGUI").AddComponent<StatisticsGUI>();
        statisticsGUI.transform.SetParent(Plugin.Instance.MainGameObject.transform);
        statisticsGUI.Hide();

        floatingActionButton = new GameObject("FabGUI").AddComponent<FloatingActionButton>();
        floatingActionButton.transform.SetParent(Plugin.Instance.MainGameObject.transform);

        settingsGUI = new GameObject("SettingsGUI").AddComponent<ArchipelagoSettingsGUI>();
        settingsGUI.transform.SetParent(Plugin.Instance.MainGameObject.transform);
        settingsGUI.DisplayingWindow = false;
        settingsGUI.debugMode = GameManager.Instance?.TfwrConfig.Debug ?? false;
        
        notificationPopup = new GameObject("Notification").AddComponent<NotificationPopup>();
        notificationPopup.transform.SetParent(Plugin.Instance.MainGameObject.transform);
    }

    private void NotifyItemReceived(object sender, Notification notification) =>
        notificationPopup.Show(notification.Title, notification.Message);

    private void OnMenuOpen(object sender, bool isOpen)
    {
        if (isOpen)
            statisticsGUI.Hide();
        else
            statisticsGUI.Show();
    }

    public bool MouseOverAnyWindow()
    {
        // if (_statisticsGUI && _statisticsGUI.IsVisible() && _statisticsGUI.IsMouseOverWindow())
        //     return true;

        // ToDo: IsMouseOverWindow on FAB doesn't work yet... need to determine why
        // if (_floatingActionButton && _floatingActionButton.IsMouseOverWindow())
        // {
        //     return true;
        // }

        return settingsGUI && settingsGUI.DisplayingWindow
                                       && settingsGUI.IsMouseOverWindow();
    }

    public void OpenConnectionSettings()
    {
        Task.Run(() =>
        {
            settingsGUI.Show(GameManager.Instance?.TfwrConfig.ConnectionInfo);
        });
    }

    private void OnGameLoaded(object sender, ModSaveGame e)
    {
        if (e is null)
        {
            statisticsGUI.Hide();
        }
        else
        {
            statisticsGUI.LoadStats(
                GoalManager.Instance?.StatsService.MilestoneCopy(),
                GoalManager.Instance?.StatsService.StatCopy(),
                APManager.Instance?.GetLocations()
            );
            statisticsGUI.Show();
        }
    }

    public bool StatGuiOpen() => !statisticsGUI;

    public Rect StatGuiBounds() => statisticsGUI.RootElement.worldBound;

    private void OnStatTotalEvent(object sender, Stat e) => statisticsGUI.StatUpdate(e.Name, e.Value);

    private void OnAPLocationGiven(object sender, APLocation location)
    {
        if(location.region != "GrassSanity") statisticsGUI.MarkCompleted(location.name);
    } 

    // ToDo: tell the user (somehow) why the connection was cancelled? Launch the GUI?
    private void OnAPDisconnected(object sender, string reason)
    {
        floatingActionButton.ConnectionStatus(false);
        settingsGUI.Disconnected(reason);
    }

    private void OnConnectionResult(object sender, LoginResult result)
    {
        settingsGUI.ConnectionAttempt(result);
        floatingActionButton.ConnectionStatus(result.Successful);
        if (!result.Successful) return;
        statisticsGUI.Show();
    }

    private void OnGoalEvent(object sender, GoalEvent e) => statisticsGUI.MarkCompleted(e.Name);
}