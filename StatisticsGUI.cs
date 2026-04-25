using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;


namespace com.seadoggie.TFWRArchipelago;

public class StatisticsGUI : MonoBehaviour
{
    private UIDocument _uiDocument;

    void Awake()
    {
        Plugin.Log.LogWarning("Initializing Statistics GUI");
        
        // 1. Create the GameObject
        GameObject root = new("TFWRAPStatisticsGUI");
        DontDestroyOnLoad(root);

        // 2. Add the UIDocument
        _uiDocument = root.AddComponent<UIDocument>();

        // 3. Create and configure PanelSettings (The "Canvas" of UI Toolkit)
        PanelSettings settings = Resources.PanelSettings;
        
        if(Resources.PanelSettings is null) Plugin.LogError("Failed to actually load PanelSettings!");

        ThemeStyleSheet themeStyleSheet = Resources.ThemeStyleSheet;
        if(themeStyleSheet is null) Plugin.LogError("Failed to load ThemeStyleSheet!");
        
        settings.themeStyleSheet = themeStyleSheet;
        _uiDocument.panelSettings = settings;

        StyleSheet styleSheet = Resources.AchievementStyleSheet;
        if(styleSheet is null) Plugin.LogError("Failed to load AchievementStyleSheet!");

        _uiDocument.rootVisualElement.styleSheets.Add(styleSheet);

        // Build the layout
        FixText(CreateLayout());
    }

    private VisualElement CreateLayout()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.top = 50;
        // Main Container
        VisualElement container = new();
        container.AddToClassList("achievement-container");

        Button close = new Button
        {
            text = "CLOSE ME",
            style =
            {
                alignSelf = Align.FlexStart
            }
        };

        close.RegisterCallback<MouseUpEvent>(evt => { enabled = false; });
        
        // Add Achievements
        // container.Add(new Label("--- ACHIEVEMENTS ---")
        //     { style = { unityTextAlign = TextAnchor.MiddleCenter, marginBottom = 10 } });
        bool first = true;
        foreach (KeyValuePair<string, List<Milestone>> statistic in UserStats.MilestoneCopy())
        {
            foreach (Milestone milestone in statistic.Value)
            {
                string title = $"{milestone.Achievement}";
                string description = $"Collect {milestone.Target:N0} {statistic.Key}";
                Plugin.Log.LogInfo(title);
                container.Add(CreateAchievementRow(title, description, milestone.Triggered, first));
                first = false;
            }
        }

        ScrollView scrollView = new()
        {
            style =
            {
                maxWidth = 300,
                maxHeight = new StyleLength(Length.Percent(100))
            }
        };
        scrollView.AddToClassList("scroll");
        scrollView.Add(container);
        root.Add(scrollView);
        return root;
    }
    
    private VisualElement CreateAchievementRow(string title, string description, bool isComplete, bool first)
    {
        GroupBox row = new();
        row.AddToClassList("statistic");
        row.AddToClassList("border");
        if(first) row.AddToClassList("border-first");

        // Title row
        VisualElement titleRow = new();
        titleRow.AddToClassList("row");
        Toggle toggle = new()
        {
            value = isComplete,
        };
        toggle.SetEnabled(false);
        toggle.AddToClassList("toggle");
        Label titleLabel = new("Location Title");
        titleLabel.AddToClassList("title");
        
        titleRow.Add(toggle);
        titleRow.Add(titleLabel);
        
        // Description row
        VisualElement descriptionRow = new();
        descriptionRow.AddToClassList("row");
        Label descLabel = new(description);
        descLabel.AddToClassList("statistic-desc");
        descriptionRow.Add(descLabel);
        
        // Progress row
        VisualElement progressRow = new();
        progressRow.AddToClassList("row");
        ProgressBar progressBar = new()
        {
            value = 22,
            title = "22 / 100",
            lowValue = 0,
            highValue = 100,
        };
        progressBar.AddToClassList("progress-bar");
        progressRow.Add(progressBar);
        
        row.Add(titleRow);
        row.Add(descriptionRow);
        row.Add(progressRow);
        return row;
    }

    private void FixText(VisualElement root)
    {
        // Try to find any font already loaded in the game (like Arial or the game's main font)
        Font gameFont = UnityEngine.Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault();
        
        if (gameFont != null)
        {
            // For older Unity versions:
            root.style.unityFont = gameFont;
        
            // For newer Unity versions (2021.3+), you usually need a FontDefinition:
            root.style.unityFontDefinition = new StyleFontDefinition(gameFont);
        
            Plugin.Log.LogInfo($"Applied font: {gameFont.name} to UI.");
        }
        else
        {
            Plugin.Log.LogError("Could not find any font in the game!");
        }
    }
}