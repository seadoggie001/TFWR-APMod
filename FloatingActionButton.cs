using com.seadoggie.TFWRArchipelago.Configuration;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.seadoggie.TFWRArchipelago;

public class FloatingActionButton : MonoBehaviour
{
    public static FloatingActionButton Instance;

    public static void Show()
    {
        Instance ??= new GameObject().AddComponent<FloatingActionButton>();
        Instance.enabled = true;
    }
    
    private UIDocument _uiDocument;
    private VisualElement _overlayIcon;
    private Action _onDisabled = null;
    
    void Awake()
    {
        Plugin.Log.LogInfo("Awaking FAB");
        Instance?.OnDisable();
        Instance?.enabled = false;
        Instance = this;

        // Create the GUI and setup styles
        Initialize();
        Plugin.Instance.APConnected += (sender, args) => ConnectionStatus(true);
    }

    void OnDisable()
    {
        _onDisabled?.Invoke();
    }

    public void ConnectionStatus(bool isConnected)
    {
        if (isConnected)
        {
            _overlayIcon.AddToClassList("d-none");
        }
        else
        {
            _overlayIcon.RemoveFromClassList("d-none");
        }
    }
    
    private void Initialize()
    {
        Plugin.Log.LogInfo("Initializing FAB");
        GameObject root = new("TFWRAP-FAB");
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

        VisualElement fab = new();
        fab.style.backgroundColor = (Color) ThemeManager.Inst.Theme.ui.button.NormalColor;
        fab.AddToClassList("fab-button");

        fab.RegisterCallback<PointerDownEvent>(Clicked);
        _onDisabled += () => fab.UnregisterCallback<PointerDownEvent>(Clicked);
        _uiDocument.rootVisualElement.Add(fab);
        
        VisualElement background = new();
        background.AddToClassList("fab-background");
        fab.Add(background);
        
        VisualElement icon = new();
        icon.AddToClassList("icon");
        background.Add(icon);
        
        _overlayIcon = new();
        _overlayIcon.AddToClassList("icon-modifier");
        icon.Add(_overlayIcon);
        
        Plugin.Log.LogInfo("Completed initializing FAB");
    }

    private void Clicked(PointerDownEvent _)
    {
        Plugin.Log.LogInfo("Clicked FAB");
        ArchipelagoSettingsGUI.Show();
    }
}