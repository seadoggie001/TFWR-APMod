using System.Collections.Concurrent;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

namespace com.seadoggie.TFWRArchipelago.UI;

public class ProgressGUI : BaseGUI
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.ProgGUI");
    private const float RefreshRate = 2.0f; // Every 2 seconds

    private UIDocument _uiDocument;
    private IEnumerable<Row> _rows;
    private Visibility _visible;

    private enum Visibility
    {
        /// <summary>GUI is Open</summary>
        Visible,

        /// <summary>GUI is temporarily software-closed</summary>
        Hidden,

        /// <summary>GUI is closed</summary>
        Closed,

        /// <summary>GUI is closed due to it not being an AP Game</summary>
        Disabled,
    }

    private Action _unregisterCallback = () => { };

    /// <summary>Queue of statistic changes to be processed</summary>
    private ConcurrentQueue<Stat> _statQueue = new();

    /// <summary>Used to signal the UI should be rebuilt</summary>
    private bool _reload = false;
    private UpdateInformation _updateInformation;

    private class UpdateInformation
    {
        public IEnumerable<KeyValuePair<string, List<Milestone>>> GroupedMilestones { get; set; }
        public Dictionary<string, double> Stats { get; set; }
        public IEnumerable<APLocation> AllLocations { get; set; }
    }

    private class Row
    {
        public GroupBox GroupBox;
        [CanBeNull] public ProgressBar ProgressBar;
        public Toggle Toggle;

        [CanBeNull] public Milestone Milestone;
        [CanBeNull] public APLocation APLocation;

        public void Completed(bool completed)
        {
            Toggle.value = completed;
            if (completed)
                GroupBox.AddToClassList("complete");
            else
                GroupBox.RemoveFromClassList("complete");
        }

        public void UpdateProgress(double? current = null, double? target = null)
        {
            if (ProgressBar is null) return;
            if (current is not null) ProgressBar.value = (float)current;
            if (target is not null) ProgressBar.highValue = (float)target;
            ProgressBar.title = $"{ProgressBar.value:N0} / {ProgressBar.highValue:N0}";
        }
    }

    public VisualElement RootElement;

    private void Awake()
    {
        Log.LogInfo("Initializing Statistics GUI");

        // Create the GUI and setup styles
        Initialize();
    }

    // Repeatedly invoke RefreshUI. After RefreshRate seconds, repeat every RefreshRate seconds
    private void Start() => InvokeRepeating(nameof(RefreshUI), RefreshRate, RefreshRate);

    #region button presses

    public void Show() => ChangeVisibility(Visibility.Visible, false);

    public void Minimize() => ChangeVisibility(Visibility.Hidden, false);

    public void Disable() => ChangeVisibility(Visibility.Disabled, false, true);

    public void Enable() => ChangeVisibility(Visibility.Hidden, false, true);

    private void Closed(MouseUpEvent _) => ChangeVisibility(Visibility.Closed, true);

    private void ExpandGUI(MouseUpEvent _) => ChangeVisibility(Visibility.Visible, true);

    #endregion

    public void MarkCompleted(string key, double value)
    {
        foreach (Row matchingRow in _rows.Where(m =>
                     m.Milestone != null
                     && m.ProgressBar != null
                     && m.Milestone.APLocation.statistic?.key == key
                     && Math.Abs(m.ProgressBar.highValue - value) < 1))
        {
            matchingRow.Completed(true);
            return;
        }

        Log.LogWarning("There are no statistics matching: " + key);
    }

    public void MarkCompleted(string key)
    {
        foreach (Row row in _rows.Where(m => m.APLocation?.name == key))
        {
            row.Completed(true);
            return;
        }

        Log.LogError("There are no achievements matching: " + key);
    }

    public override bool IsMouseOverWindow() =>
        RootElement.worldBound.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));

    public void StatUpdate(string item, double value) => _statQueue.Enqueue(new Stat(item, value));

    /// <summary>
    /// This is called to alert the GUI that a new set of statistics needs to be processed
    /// </summary>
    /// <param name="groupedMilestones"></param>
    /// <param name="stats"></param>
    /// <param name="allLocations"></param>
    public void LoadStats(IEnumerable<KeyValuePair<string, List<Milestone>>> groupedMilestones,
        Dictionary<string, double> stats,
        IEnumerable<APLocation> allLocations)
    {
        if (groupedMilestones is null) Log.LogWarning("GroupedMilestones is null");
        if (stats is null) Log.LogWarning("Stats is null");
        if (allLocations is null) Log.LogWarning("AllLocations is null");

        // Save the information needed for a reload
        _updateInformation = new UpdateInformation
        {
            GroupedMilestones = groupedMilestones,
            Stats = stats,
            AllLocations = allLocations,
        };
        // Clear the statistic queue
        _statQueue = new ConcurrentQueue<Stat>();
        _reload = true;
    }

    private void ChangeVisibility(Visibility visibility, bool user, bool force = false)
    {
        if (force || user)
        {
            _visible = visibility;
        }
        else
        {
            if (!Plugin.Instance.Enabled) return;

            if (visibility is Visibility.Hidden or Visibility.Visible)
            {
                if (_visible < Visibility.Closed) _visible = visibility;
            }
            else
            {
                Plugin.Log.LogInfo("Ooops, GUI visibility went weird");
            }
        }

        switch (_visible)
        {
            case Visibility.Visible:
                RootElement.style.display = DisplayStyle.Flex;
                RootElement.RemoveFromClassList("collapse");
                break;
            case Visibility.Hidden:
            case Visibility.Closed:
                RootElement.style.display = DisplayStyle.Flex;
                RootElement.AddToClassList("collapse");
                break;
            case Visibility.Disabled:
                RootElement.style.display = DisplayStyle.None;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_visible),
                    "Unexpected visibility found in ChangeVisibility");
        }
    }

    //ToDo: Destroy the object and recreate it instead. This is pain.
    private void RebuildUI()
    {
        // Unregister all callbacks
        _unregisterCallback?.Invoke();
        // Reset unregister callback
        _unregisterCallback = () => { };
        if (_uiDocument?.rootVisualElement is null)
            Initialize();
        else
            // Remove all elements from the GUI
            _uiDocument.rootVisualElement.Clear();

        // clear internal data
        _rows = [];
        // Create the layout again
        CreateLayout(_updateInformation.GroupedMilestones, _updateInformation.AllLocations);

        _reload = false;
        _updateInformation = null;
    }

    private void Initialize()
    {
        GameObject root = new("TFWRAPStatisticsGUI");
        DontDestroyOnLoad(root);

        _uiDocument = root.AddComponent<UIDocument>();

        PanelSettings settings = Resources.PanelSettings;
        if (settings is null) Log.LogException("Failed to actually load PanelSettings!");

        ThemeStyleSheet themeStyleSheet = Resources.ThemeStyleSheet;
        if (themeStyleSheet is null) Log.LogException("Failed to load ThemeStyleSheet!");

        settings?.themeStyleSheet = themeStyleSheet;
        _uiDocument.panelSettings = settings;

        StyleSheet styleSheet = Resources.AchievementStyleSheet;
        if (styleSheet is null) Log.LogException("Failed to load StyleSheet!");

        _uiDocument.rootVisualElement.styleSheets.Add(styleSheet);
        RootElement = _uiDocument.rootVisualElement;
    }

    private void CreateLayout(IEnumerable<KeyValuePair<string, List<Milestone>>> groupedMilestones,
        IEnumerable<APLocation> allLocations)
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.top = 75;
        root.style.maxWidth = 300;
        root.style.display = DisplayStyle.Flex;
        if (_visible <= Visibility.Hidden)
        {
            root.AddToClassList("collapse");
        }

        // Title row
        VisualElement titleRow = CreateWithClass(["title-row", "row", "p-0"]);
        root.Add(titleRow);

        // Title
        Label title = new("Checks");
        title.AddToClassList("collapse-header");
        titleRow.Add(title);

        // Button group
        VisualElement buttonGroup = CreateWithClass(["btn-group"]);

        Button statBtn = new()
        {
            text = "Statistic",
        };
        statBtn.AddToClassList("btn");
        statBtn.AddToClassList("btn-toggle");

        Button achievementBtn = new()
        {
            text = "Achievement",
        };
        achievementBtn.AddToClassList("btn");
        achievementBtn.AddToClassList("btn-toggle");

        Button completedBtn = new()
        {
            text = "Completed",
            tooltip = "Toggle completed",
        };
        completedBtn.AddToClassList("btn");
        completedBtn.AddToClassList("btn-toggle");

        Button close = new()
        {
            text = "X",
        };
        close.AddToClassList("btn");
        close.AddToClassList("btn-close");

        buttonGroup.Add(statBtn);
        buttonGroup.Add(achievementBtn);
        buttonGroup.Add(completedBtn);
        buttonGroup.Add(close);
        titleRow.Add(buttonGroup);

        Button expandBtn = new()
        {
            text = ">>",
            tooltip = "Expand",
        };
        expandBtn.AddToClassList("btn");
        expandBtn.AddToClassList("btn-expand");
        expandBtn.AddToClassList("collapse-header");
        titleRow.Add(expandBtn);

        // ScrollView
        ScrollView scrollView = new()
        {
            style =
            {
                maxWidth = 300,
                maxHeight = new StyleLength(Length.Percent(100))
            }
        };
        scrollView.AddToClassList("scroll");

        // Main Container
        VisualElement container = new();
        container.AddToClassList("statistic-container");

        bool first = true;

        foreach (KeyValuePair<string, List<Milestone>> statistic in groupedMilestones)
        {
            List<Milestone> milestones = statistic.Value.OrderBy(milestone => milestone.BaseNumber).ToList();
            foreach (Milestone milestone in milestones)
            {
                container.Add(CreateMilestoneRow(milestone, first));
                first = false;
            }
        }

        foreach (APLocation apLocation in allLocations.Where(m =>
                     m.statistic is null && m.timed is null && m.region != "GrassSanity"))
        {
            container.Add(CreateActionRow(apLocation));
        }

        scrollView.Add(container);
        root.Add(scrollView);

        statBtn.RegisterCallback<MouseUpEvent>(ToggleStatistics);
        _unregisterCallback += () => statBtn.UnregisterCallback<MouseUpEvent>(ToggleStatistics);

        achievementBtn.RegisterCallback<MouseUpEvent>(ToggleAchievements);
        _unregisterCallback += () => achievementBtn.UnregisterCallback<MouseUpEvent>(ToggleAchievements);

        completedBtn.RegisterCallback<MouseUpEvent>(ToggleCompleted);
        _unregisterCallback += () => completedBtn.UnregisterCallback<MouseUpEvent>(ToggleCompleted);

        expandBtn.RegisterCallback<MouseUpEvent>(ExpandGUI);
        _unregisterCallback += () => expandBtn.UnregisterCallback<MouseUpEvent>(ExpandGUI);

        close.RegisterCallback<MouseUpEvent>(Closed);
        _unregisterCallback += () => close.UnregisterCallback<MouseUpEvent>(Closed);

        return;

        void ToggleCompleted(MouseUpEvent evt)
        {
            completedBtn.ToggleInClassList("toggled");
            container.ToggleInClassList("hide-complete");
        }

        void ToggleAchievements(MouseUpEvent evt)
        {
            achievementBtn.ToggleInClassList("toggled");
            container.ToggleInClassList("hide-achievement");
        }

        void ToggleStatistics(MouseUpEvent evt)
        {
            statBtn.ToggleInClassList("toggled");
            container.ToggleInClassList("hide-progress");
        }
    }

    private static VisualElement CreateWithClass(IEnumerable<string> classes)
    {
        VisualElement element = new();
        foreach (string className in classes) element.AddToClassList(className);
        return element;
    }

    private VisualElement CreateMilestoneRow(Milestone milestone, bool first)
    {
        GroupBox groupBox = new();
        groupBox.AddToClassList("statistic");
        groupBox.AddToClassList("progress");
        groupBox.AddToClassList("border");
        if (milestone.Triggered) groupBox.AddToClassList("complete");
        if (first) groupBox.AddToClassList("border-first");

        // Title row
        VisualElement titleRow = new();
        titleRow.AddToClassList("row");
        Toggle toggle = new()
        {
            value = milestone.Triggered,
        };
        toggle.SetEnabled(false);
        toggle.AddToClassList("toggle");
        toggle.AddToClassList("border");
        Label titleLabel = new(milestone.APLocation.name);
        titleLabel.AddToClassList("title");

        titleRow.Add(toggle);
        titleRow.Add(titleLabel);

        // Description row
        VisualElement descriptionRow = new();
        descriptionRow.AddToClassList("row");
        Label descLabel = new(milestone.APLocation.description);
        descLabel.AddToClassList("statistic-desc");
        descriptionRow.Add(descLabel);

        // Progress row
        VisualElement progressRow = new();
        progressRow.AddToClassList("row");
        ProgressBar progressBar = new()
        {
            lowValue = 0,
            highValue = (float)(milestone.Target ?? Math.Pow(10, 9)),
        };
        progressBar.AddToClassList("progress-bar");
        progressRow.Add(progressBar);

        groupBox.Add(titleRow);
        groupBox.Add(descriptionRow);
        groupBox.Add(progressRow);

        Row row = new()
        {
            ProgressBar = progressBar,
            GroupBox = groupBox,
            Toggle = toggle,
            Milestone = milestone,
            APLocation = milestone.APLocation,
        };
        row.UpdateProgress();
        _rows = _rows.AddItem(row);

        return groupBox;
    }

    private VisualElement CreateActionRow(APLocation apLocation)
    {
        GroupBox row = new();
        row.AddToClassList("statistic");
        row.AddToClassList("achievement");
        row.AddToClassList("border");

        // Title row
        VisualElement titleRow = new();
        titleRow.AddToClassList("row");
        Toggle toggle = new()
        {
            value = false,
        };
        toggle.SetEnabled(false);
        toggle.AddToClassList("toggle");
        toggle.AddToClassList("border");
        Label titleLabel = new(apLocation.name);
        titleLabel.AddToClassList("title");

        titleRow.Add(toggle);
        titleRow.Add(titleLabel);

        // Description row
        VisualElement descriptionRow = new();
        descriptionRow.AddToClassList("row");
        Label descLabel = new(apLocation.description);
        descLabel.AddToClassList("statistic-desc");
        descriptionRow.Add(descLabel);

        row.Add(titleRow);
        row.Add(descriptionRow);

        _rows = _rows.AddItem(new Row()
        {
            ProgressBar = null,
            GroupBox = row,
            Toggle = toggle,
            APLocation = apLocation
        });

        return row;
    }

    public void ApplyOptions(Dictionary<double, double> modifiedValues)
    {
        try
        {
            foreach (KeyValuePair<double, double> modifiedValue in modifiedValues)
            {
                foreach (Row row in _rows.Where(m => 
                             m.Milestone != null && Math.Abs(m.Milestone.BaseNumber - modifiedValue.Key) < 1))
                {
                    row.UpdateProgress(null, modifiedValue.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogException("Failed to update values", ex);
        }
    }

    private void RefreshUI()
    {
        if (_reload) RebuildUI();
        if (_visible != Visibility.Visible || _statQueue.IsEmpty) return;

        // Hold a unique list of stats to refresh
        Dictionary<string, double> refreshStats = new();

        // While there are more stats
        while (_statQueue.TryDequeue(out Stat stat))
        {
            // Add or combine the stat into the dictionary
            if (refreshStats.ContainsKey(stat.Name))
            {
                // These are totals of the statistic, use the larger value
                refreshStats[stat.Name] = Math.Max(refreshStats[stat.Name], stat.Value);
            }
            else
            {
                refreshStats.Add(stat.Name, stat.Value);
            }
        }

        // For each stat to refresh
        foreach (KeyValuePair<string, double> stat in refreshStats)
        {
            foreach (Row row in _rows.Where(m => m.Milestone?.APLocation.statistic?.key == stat.Key))
            {
                row.UpdateProgress(stat.Value);
            }
        }
    }
}