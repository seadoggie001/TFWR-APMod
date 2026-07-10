using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

namespace com.seadoggie.TFWRArchipelago.UI;

public class FloatingActionButton : BaseGUI
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.UI-FAB");
    private UIDocument _uiDocument;
    private VisualElement _rootElement;
    private VisualElement _fab;
    [CanBeNull] private VisualElement _overlayIcon;
    private Action _onDisabled;

    private void Start()
    {
        Log.LogInfo("Awaking FAB");

        // Create the GUI and setup styles
        Initialize();
    }

    private void OnDisable() => _onDisabled?.Invoke();

    public void ConnectionStatus(bool isConnected)
    {
        if (isConnected)
        {
            _overlayIcon?.AddToClassList("d-none");
        }
        else
        {
            _overlayIcon?.RemoveFromClassList("d-none");
        }
    }

    private void Initialize()
    {
        Log.LogInfo("Initializing FAB");
        GameObject root = new("TFWRAP-FAB");
        DontDestroyOnLoad(root);

        _uiDocument = root.AddComponent<UIDocument>();

        PanelSettings settings = Resources.PanelSettings;
        if (settings is null)
        {
            Log.LogException("Failed to actually load PanelSettings!");
            return;
        }

        ThemeStyleSheet themeStyleSheet = Resources.ThemeStyleSheet;
        if (themeStyleSheet is null) Log.LogException("Failed to load ThemeStyleSheet!");

        settings.themeStyleSheet = themeStyleSheet;
        _uiDocument.panelSettings = settings;
        _rootElement = _uiDocument.rootVisualElement;

        StyleSheet styleSheet = Resources.AchievementStyleSheet;
        if (styleSheet is null) Log.LogException("Failed to load StyleSheet!");

        _rootElement.styleSheets.Add(styleSheet);

        _fab = new VisualElement
        {
            style =
            {
                backgroundColor = (Color)ThemeManager.Inst.Theme.ui.button.NormalColor
            }
        };
        _fab.AddToClassList("fab-button");

        _fab.RegisterCallback<PointerDownEvent>(Clicked);
        _onDisabled += () => _fab.UnregisterCallback<PointerDownEvent>(Clicked);
        _rootElement.Add(_fab);

        VisualElement background = new();
        background.AddToClassList("fab-background");
        _fab.Add(background);

        VisualElement icon = new();
        icon.AddToClassList("icon");
        background.Add(icon);

        _overlayIcon = new();
        _overlayIcon.AddToClassList("icon-modifier");
        icon.Add(_overlayIcon);

        Log.LogInfo("Completed initializing FAB");
    }

    private static void Clicked(PointerDownEvent _)
    {
        Log.LogInfo("Clicked FAB");
        UIManager.Instance?.OpenConnectionSettings();
    }

    public override bool IsMouseOverWindow() =>
        _fab.worldBound.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
}