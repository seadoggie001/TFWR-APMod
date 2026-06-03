using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using com.seadoggie.TFWRArchipelago.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.Components;

public class UIManager : BaseComponent, IUIManagerDelegates
{
    [CanBeNull] public static UIManager Instance;
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.UIMgr");

    public event EventHandler<ConnectionInfo> ConnectionAttemptEvent;

    private StatisticsGUI _statisticsGUI;
    private ArchipelagoSettingsGUI _archipelagoSettingsGUI;
    private FloatingActionButton _floatingActionButton;
    private Notification _notification;

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
        GoalManager.Instance.GoalEvent += OnGoalEvent;
        OnDisabled += () => GoalManager.Instance.GoalEvent -= OnGoalEvent;
        GoalManager.Instance.StatTotalEvent += OnStatTotalEvent;
        OnDisabled += () => GoalManager.Instance.StatTotalEvent -= OnStatTotalEvent;

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
        GameManager.Instance?.GameService.NewItemReceived += OnNewItemReceived;
        OnDisabled += () => GameManager.Instance?.GameService.NewItemReceived -= OnNewItemReceived;

        _statisticsGUI = new GameObject("StatGUI").AddComponent<StatisticsGUI>();
        _statisticsGUI.transform.SetParent(Plugin.Instance.MainGameObject.transform);
        _statisticsGUI.Hide();

        _floatingActionButton = new GameObject("FabGUI").AddComponent<FloatingActionButton>();
        _floatingActionButton.transform.SetParent(Plugin.Instance.MainGameObject.transform);

        _archipelagoSettingsGUI = new GameObject().AddComponent<ArchipelagoSettingsGUI>();
        _archipelagoSettingsGUI.transform.SetParent(Plugin.Instance.MainGameObject.transform);
        _archipelagoSettingsGUI.DisplayingWindow = false;
        
        _notification = new GameObject("Notification").AddComponent<Notification>();
        _notification.transform.SetParent(Plugin.Instance.MainGameObject.transform);
    }

    private void OnNewItemReceived(object sender, string e)
    {
        // inflate the hat popup somehow
        _notification.ShowPopup("You received an item!", e);
    }

    private void OnMenuOpen(object sender, bool isOpen)
    {
        if (isOpen)
            _statisticsGUI.Hide();
        else
            _statisticsGUI.Show();
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

        return _archipelagoSettingsGUI && _archipelagoSettingsGUI.DisplayingWindow
                                       && _archipelagoSettingsGUI.IsMouseOverWindow();
    }

    public void OpenConnectionSettings()
    {
        Log.LogInfo($"OpenConnectionSettings");
        _archipelagoSettingsGUI.Show(GameManager.Instance?.TfwrConfig.ConnectionInfo);
    }

    private void OnGameLoaded(object sender, ModSaveGame e)
    {
        if (e is null)
        {
            _statisticsGUI.Hide();
        }
        else
        {
            _statisticsGUI.LoadStats();
            _statisticsGUI.Show();
        }
    }

    public void RaiseConnectionAttemptEvent(ConnectionInfo attempt)
    {
        Log.LogInfo($"RaiseConnectionAttemptEvent");
        ConnectionAttemptEvent?.Invoke(this, attempt);
    }

    public bool StatGuiOpen() => !_statisticsGUI;

    public Rect StatGuiBounds() => _statisticsGUI.RootElement.worldBound;

    private void OnStatTotalEvent(object sender, Stat e) => _statisticsGUI.StatUpdate(e.Name, e.Value);

    private void OnAPLocationGiven(object sender, APLocation location) => _statisticsGUI.MarkCompleted(location.name);

    // ToDo: tell the user (somehow) why the connection was cancelled? Launch the GUI?
    private void OnAPDisconnected(object sender, string reason)
    {
        _floatingActionButton.ConnectionStatus(false);
        _archipelagoSettingsGUI.Disconnected(reason);
    }

    private void OnConnectionResult(object sender, bool success)
    {
        _archipelagoSettingsGUI.ConnectionAttempt(success);
        _floatingActionButton.ConnectionStatus(success);
        if (!success) return;
        _statisticsGUI.Show();
    }

    private void OnGoalEvent(object sender, GoalEvent e) => _statisticsGUI.MarkCompleted(e.Name);
}

public interface IUIManagerDelegates
{
    public event EventHandler<ConnectionInfo> ConnectionAttemptEvent;
}