using com.seadoggie.TFWRArchipelago.Constants;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Collections;


namespace com.seadoggie.TFWRArchipelago;

public class StatisticsGUI : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Dictionary<string, List<RowElements>> _statisticRows = new();
    private Dictionary<string, RowElements> _achievementRows = new();

    private class RowElements
    {
        public GroupBox Row;
        public ProgressBar ProgressBar;
        public Toggle Toggle;
    }

    public static StatisticsGUI Instance;
    public static void Show()
    {
        Instance ??= new GameObject().AddComponent<StatisticsGUI>();
        Instance.enabled = true;
    }
    
    public VisualElement RootElement;

    private void Awake()
    {
        Plugin.Log.LogInfo("Initializing Statistics GUI");
        Instance?.OnDisable();
        Instance?.enabled = false;
        Instance = this;

        // Create the GUI and setup styles
        Initialize();

        // Build the layout
        CreateLayout();
    }

    private void OnEnable() => UserStats.OnStatChange += OnStatUpdate;

    private void OnDisable() => UserStats.OnStatChange -= OnStatUpdate;

    public void MarkCompleted(string key, double value)
    {
        if (!_statisticRows.ContainsKey(key))
        {
            Plugin.LogError("There are no rows matching: " + key);
            return;
        }

        foreach (RowElements rowElements in _statisticRows[key])
        {
            if (!(Math.Abs(rowElements.ProgressBar.highValue - (float)value) < 1)) continue;
            rowElements.Toggle.value = true;
            rowElements.Row.AddToClassList("complete");
            return;
        }

        Plugin.LogError("There are no statistics matching: " + key);
    }

    public void MarkCompleted(string key)
    {
        _achievementRows.TryGetValue(key, out RowElements rowElements);
        if (rowElements is null)
        {
            Plugin.Log.LogError("There are no achievements matching: " + key);
            return;
        }

        rowElements.Toggle.value = true;
        rowElements.Row.AddToClassList("complete");
    }

    private void Initialize()
    {
        GameObject root = new("TFWRAPStatisticsGUI");
        DontDestroyOnLoad(root);

        _uiDocument = root.AddComponent<UIDocument>();

        PanelSettings settings = Resources.PanelSettings;
        if (settings is null) Plugin.LogError("Failed to actually load PanelSettings!");

        ThemeStyleSheet themeStyleSheet = Resources.ThemeStyleSheet;
        if (themeStyleSheet is null) Plugin.LogError("Failed to load ThemeStyleSheet!");

        settings.themeStyleSheet = themeStyleSheet;
        _uiDocument.panelSettings = settings;

        StyleSheet styleSheet = Resources.AchievementStyleSheet;
        if (styleSheet is null) Plugin.LogError("Failed to load StyleSheet!");

        _uiDocument.rootVisualElement.styleSheets.Add(styleSheet);
        RootElement = _uiDocument.rootVisualElement;
    }

    private void CreateLayout()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.top = 75;
        root.style.maxWidth = 300;

        // Button row
        VisualElement buttonRow = new();
        buttonRow.AddToClassList("row");
        buttonRow.AddToClassList("btn-row");
        
        // ToDo: Move to .USS file (.btn-row)
        buttonRow.style.flexGrow = 0;
        
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
        close.RegisterCallback<MouseUpEvent>(evt =>
        {
            Plugin.Log.LogInfo("Closing Stats GUI");
            Instance = null;
            _uiDocument.enabled = false;
        });
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
        foreach (KeyValuePair<string, List<Milestone>> statistic in UserStats.MilestoneCopy())
        {
            List<Milestone> milestones = statistic.Value.OrderBy(milestone => milestone.Target).ToList();
            foreach (Milestone milestone in milestones)
            {
                container.Add(CreateMilestoneRow(statistic.Key, milestone, first));
                first = false;
            }
        }

        foreach (APLocation apLocation in APLocation.APLocations.Where(m => m.statistic is null && m.timed is null))
        {
            container.Add(CreateActionRow(apLocation));
        }

        scrollView.Add(container);
        root.Add(scrollView);
        
        statBtn.RegisterCallback<MouseUpEvent>(evt =>
        {
            statBtn.ToggleInClassList("toggled");
            container.ToggleInClassList("hide-progress");
        });
        
        achievementBtn.RegisterCallback<MouseUpEvent>(evt =>
        {
            achievementBtn.ToggleInClassList("toggled");
            container.ToggleInClassList("hide-achievement");
        });
        
        completedBtn.RegisterCallback<MouseUpEvent>(evt =>
        {
            completedBtn.ToggleInClassList("toggled");
            container.ToggleInClassList("hide-complete");
        });
    }

    private VisualElement CreateMilestoneRow(string key, Milestone milestone, bool first)
    {
        GroupBox row = new();
        row.AddToClassList("statistic");
        row.AddToClassList("progress");
        row.AddToClassList("border");
        if(milestone.Triggered) row.AddToClassList("complete");
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
        if (!UserStats.TryGetValue(key, out double value)) value = 0;
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
        if (_statisticRows.ContainsKey(key))
        {
            _statisticRows[key].Add(rowElements);
        }
        else
        {
            _statisticRows[key] = [rowElements];
        }

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
        _achievementRows.Add(apLocation.name, new RowElements(){ ProgressBar = null, Row = row, Toggle = toggle });
                
        return row;
    }

    private void UpdateValues(RowElements rowElements, double value)
    {
        rowElements.ProgressBar.value = (float)value;
        rowElements.ProgressBar.title = $"{value:N0} / {rowElements.ProgressBar.highValue:N0}";

        rowElements.Toggle.value = value > rowElements.ProgressBar.highValue;
    }

    private void OnStatUpdate(string item, double value)
    {
        if (_statisticRows.ContainsKey(item))
        {
            foreach (RowElements rowElements in _statisticRows[item])
            {
                UpdateValues(rowElements, value);
            }
        }
    }
}