using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using com.seadoggie.TFWRArchipelago.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Service;
using com.seadoggie.TFWRArchipelago.UI;
using JetBrains.Annotations;
using UnityEngine;
using ILogger = com.seadoggie.TFWRArchipelago.Logging.ILogger;

namespace com.seadoggie.TFWRArchipelago.Components;

public class UIManager : BaseComponent
{
    [CanBeNull] public static UIManager Instance;

    [ModInject]
    public ILogService LogService
    {
        set => Log = value.CreateLog("TFWRAP.UIMgr");
    }

    private ILogger Log;

    public ProgressGUI progressGUI;
    public ArchipelagoSettingsGUI settingsGUI;
    public FloatingActionButton floatingActionButton;
    public NotificationPopup notificationPopup;

    protected override void OnEnable()
    {
        base.OnEnable();
        Instance = this;
    }

    public override void OnDisable()
    {
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
        APManager.Instance?.APService.OptionsLoaded += OnOptionsLoaded;
        OnDisabled += () => APManager.Instance?.APService.OptionsLoaded -= OnOptionsLoaded;

        GameManager.Instance?.GameService.GameLoaded += OnGameLoaded;
        OnDisabled += () => GameManager.Instance?.GameService.GameLoaded -= OnGameLoaded;
        GameManager.Instance?.GameService.MenuOpen += OnMenuOpen;
        OnDisabled += () => GameManager.Instance?.GameService.MenuOpen -= OnMenuOpen;
        GameManager.Instance?.NewItemReceived += NotifyItemReceived;
        OnDisabled += () => GameManager.Instance?.NewItemReceived -= NotifyItemReceived;

        progressGUI = new GameObject("StatGUI").AddComponent<ProgressGUI>();
        progressGUI.transform.SetParent(Plugin.Instance.MainGameObject.transform);
        progressGUI.Disable();

        floatingActionButton = new GameObject("FabGUI").AddComponent<FloatingActionButton>();
        floatingActionButton.transform.SetParent(Plugin.Instance.MainGameObject.transform);

        settingsGUI = new GameObject("SettingsGUI").AddComponent<ArchipelagoSettingsGUI>();
        settingsGUI.transform.SetParent(Plugin.Instance.MainGameObject.transform);
        settingsGUI.DisplayingWindow = false;
        settingsGUI.debugMode = GameManager.Instance?.TfwrConfig.Debug ?? false;

        notificationPopup = new GameObject("Notification").AddComponent<NotificationPopup>();
        notificationPopup.transform.SetParent(Plugin.Instance.MainGameObject.transform);

        InjectionService.Inject(progressGUI);
        InjectionService.Inject(floatingActionButton);
        InjectionService.Inject(settingsGUI);
        InjectionService.Inject(notificationPopup);
    }

    private void OnOptionsLoaded(object sender, APOptions options)
    {
        progressGUI.ApplyOptions(options.ModifiedValues);
    }

    private void NotifyItemReceived(object sender, Notification notification) =>
        notificationPopup.Show(notification.Title, notification.Message);

    private void OnMenuOpen(object sender, bool isOpen)
    {
        if (isOpen)
            progressGUI.Minimize();
        else
            progressGUI.Show();
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
        Task.Run(() => { settingsGUI.Show(GameManager.Instance?.TfwrConfig.ConnectionInfo); });
    }

    private void OnGameLoaded(object sender, ModSaveGame modSaveGame)
    {
        if (modSaveGame is null)
        {
            progressGUI.Disable();
        }
        else
        {
            progressGUI.Enable();
            progressGUI.LoadStats(
                GoalManager.Instance?.StatsService.MilestoneCopy(),
                GoalManager.Instance?.StatsService.StatCopy(),
                APManager.Instance?.GetLocations()
            );
        }
    }

    public bool StatGuiOpen() => !progressGUI;

    public Rect StatGuiBounds() => progressGUI.RootElement.worldBound;

    private void OnStatTotalEvent(object sender, Stat e) => progressGUI.StatUpdate(e.Name, e.Value);

    private void OnAPLocationGiven(object sender, APLocation location)
    {
        if (location.region != "GrassSanity") progressGUI.MarkCompleted(location.name);
    }

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
        progressGUI.Show();
    }

    private void OnGoalEvent(object sender, GoalEvent e) => progressGUI.MarkCompleted(e.Name);
}