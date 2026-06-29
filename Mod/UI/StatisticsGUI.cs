using System.Collections.Concurrent;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

namespace com.seadoggie.TFWRArchipelago.UI;

public class StatisticsGUI : BaseGUI
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.StatGUI");
    private const float RefreshRate = 2.0f; // Every 2 seconds

    private UIDocument _uiDocument;
    private readonly Dictionary<string, List<RowElements>> _statisticRows = new();
    private readonly Dictionary<string, RowElements> _goalRowsByLocation = new();
    private bool _visible;
    private Action _unregisterCallback = () => { };
    private readonly ConcurrentQueue<Stat> _statQueue = new();

    private class RowElements
    {
        public GroupBox Row;
        public ProgressBar ProgressBar;
        public Toggle Toggle;
    }

    public VisualElement RootElement;

    private void Awake()
    {
        Log.LogInfo("Initializing Statistics GUI");

        // Create the GUI and setup styles
        Initialize();
        Hide();
    }

    private void Start()
    {
        // Repeatedly invoke RefreshUI. After RefreshRate seconds, repeat every RefreshRate seconds
        InvokeRepeating(nameof(RefreshUI), RefreshRate, RefreshRate);
    }

    public void Show()
    {
        RootElement.style.display = DisplayStyle.Flex;
        _visible = true;
    }

    public void Hide(MouseUpEvent e = null)
    {
        RootElement.style.display = DisplayStyle.None;
        _visible = false;
    }

    public bool IsVisible() => _visible;

    public void MarkCompleted(string key, double value)
    {
        if (!_statisticRows.TryGetValue(key, out List<RowElements> row))
        {
            Log.LogError("There are no rows matching: " + key);
            return;
        }

        foreach (RowElements rowElements in row.Where(rowElements =>
                     Math.Abs(rowElements.ProgressBar.highValue - (float)value) < 1))
        {
            rowElements.Toggle.value = true;
            rowElements.Row.AddToClassList("complete");
            return;
        }

        Log.LogWarning("There are no statistics matching: " + key);
    }

    public void MarkCompleted(string key)
    {
        _goalRowsByLocation.TryGetValue(key, out RowElements rowElements);
        if (rowElements is null)
        {
            Log.LogError("There are no achievements matching: " + key);
            return;
        }

        rowElements.Toggle.value = true;
        rowElements.Row.AddToClassList("complete");
    }

    public override bool IsMouseOverWindow() =>
        RootElement.worldBound.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));

    public void StatUpdate(string item, double value)
    {
        _statQueue.Enqueue(new Stat(item, value));
    }

    public void LoadStats(IEnumerable<KeyValuePair<string, List<Milestone>>> groupedMilestones,
        Dictionary<string, double> stats,
        IEnumerable<APLocation> allLocations)
    {
        if(groupedMilestones is null) Log.LogWarning("GroupedMilestones is null");
        if(stats is null) Log.LogWarning("Stats is null");
        if(allLocations is null) Log.LogWarning("AllLocations is null");
        // Unregister all callbacks
        _unregisterCallback?.Invoke();
        // Reset unregister callback
        _unregisterCallback = () => { };
        // Remove all stats
        _uiDocument.rootVisualElement.Clear();
        // clear internal data
        _statisticRows.Clear();
        _goalRowsByLocation.Clear();
        // Create the layout again
        CreateLayout(groupedMilestones, stats, allLocations);
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
        Dictionary<string, double> stats,
        IEnumerable<APLocation> allLocations)
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.top = 75;
        root.style.maxWidth = 300;

        // Button row
        VisualElement buttonRow = new();
        buttonRow.AddToClassList("row");
        buttonRow.AddToClassList("btn-row");

        // ToDo: Move to .USS file (.btn-row)... should be done? (June 1st, 2026)
        // buttonRow.style.flexGrow = 0;

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
        close.RegisterCallback<MouseUpEvent>(Hide);
        _unregisterCallback += () => close.UnregisterCallback<MouseUpEvent>(Hide);

        buttonRow.Add(statBtn);
        buttonRow.Add(achievementBtn);
        buttonRow.Add(completedBtn);
        buttonRow.Add(close);
        root.Add(buttonRow);

        // Enable scrolling of the container
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
            List<Milestone> milestones = statistic.Value.OrderBy(milestone => milestone.Target).ToList();
            if (!stats.TryGetValue(statistic.Key, out double value)) value = 0;
            foreach (Milestone milestone in milestones)
            {
                container.Add(CreateMilestoneRow(statistic.Key, milestone, first, value));
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

    private VisualElement CreateMilestoneRow(string key, Milestone milestone, bool first, double value)
    {
        GroupBox row = new();
        row.AddToClassList("statistic");
        row.AddToClassList("progress");
        row.AddToClassList("border");
        if (milestone.Triggered) row.AddToClassList("complete");
        if (first) row.AddToClassList("border-first");

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
            highValue = (float)milestone.Target,
        };
        progressBar.AddToClassList("progress-bar");
        progressRow.Add(progressBar);

        row.Add(titleRow);
        row.Add(descriptionRow);
        row.Add(progressRow);

        RowElements rowElements = new()
        {
            Row = row,
            ProgressBar = progressBar,
            Toggle = toggle,
        };
        if (_statisticRows.TryGetValue(key, out List<RowElements> statisticRow))
        {
            statisticRow.Add(rowElements);
        }
        else
        {
            _statisticRows[key] = [rowElements];
        }

        _goalRowsByLocation.Add(milestone.Location, rowElements);

        UpdateValues(rowElements, value);

        return row;
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
        _goalRowsByLocation.Add(apLocation.name, new RowElements() { ProgressBar = null, Row = row, Toggle = toggle });

        return row;
    }

    private static void UpdateValues(RowElements rowElements, double value)
    {
        try
        {
            rowElements.ProgressBar.value = (float)value;
            rowElements.ProgressBar.title = $"{value:N0} / {rowElements.ProgressBar.highValue:N0}";

            rowElements.Toggle.value = value > rowElements.ProgressBar.highValue;
        }
        catch (InvalidOperationException ex)
        {
            // This message gets thrown because I'm updating the GUI too quickly sometimes, I'm pretty sure
            if (ex.Message !=
                "VisualElements cannot be marked for dirty repaint under an active visual tree during generateVisualContent callback execution nor during visual tree rendering")
            {
                Log.LogException("Failed to update values", ex);
            }
        }
    }

    private void RefreshUI()
    {
        if (!_visible || _statQueue.IsEmpty) return;

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
            if (!_statisticRows.TryGetValue(stat.Key, out List<RowElements> row)) return;
            foreach (RowElements rowElements in row)
            {
                UpdateValues(rowElements, stat.Value);
            }
        }
    }
}