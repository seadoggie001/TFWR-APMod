using System;
using com.seadoggie.TFWRArchipelago.Components;
using com.seadoggie.TFWRArchipelago.Service;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

namespace com.seadoggie.TFWRArchipelago.UI;

public class FloatingActionButton : BaseGUI
{
    [Log] private readonly ILogger<FloatingActionButton> _log = null!;
    private UIDocument _uiDocument;
    private VisualElement _rootElement;
    private VisualElement _fab;
    [CanBeNull] private VisualElement _overlayIcon;
    private Action _onDisabled;

    private void Start()
    {
        _log.LogInfo("Awaking FAB");

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
        _log.LogInfo("Initializing FAB");
        GameObject root = new("TFWRAP-FAB");
        DontDestroyOnLoad(root);

        _uiDocument = root.AddComponent<UIDocument>();

        PanelSettings settings = Resources.PanelSettings;
        if (settings is null)
        {
            _log.LogError("Failed to actually load PanelSettings!");
            return;
        }

        ThemeStyleSheet themeStyleSheet = Resources.ThemeStyleSheet;
        if (themeStyleSheet is null) _log.LogError("Failed to load ThemeStyleSheet!");

        settings.themeStyleSheet = themeStyleSheet;
        _uiDocument.panelSettings = settings;
        _rootElement = _uiDocument.rootVisualElement;

        StyleSheet styleSheet = Resources.AchievementStyleSheet;
        if (styleSheet is null) _log.LogError("Failed to load StyleSheet!");

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

        _log.LogInfo("Completed initializing FAB");
    }

    private void Clicked(PointerDownEvent _)
    {
        _log.LogInfo("Clicked FAB");
        UIManager.Instance?.OpenConnectionSettings();
    }

    public override bool IsMouseOverWindow() =>
        _fab.worldBound.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
}