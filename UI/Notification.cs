using System.Collections;
using System.Collections.Concurrent;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Resources = com.seadoggie.TFWRArchipelago.Assets.Resources;

namespace com.seadoggie.TFWRArchipelago.UI;

public class Notification : BaseGUI
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.UI-Notif");
    private UIDocument _uiDocument;
    private VisualElement _rootElement;
    private VisualElement _container;
    private VisualElement _notification;
    private Label _title;
    private Label _text;
    private bool _isDisplayed;

    private readonly ConcurrentQueue<string> _messageQueue = new();
    
    private void Start()
    {
        GameObject root = new("TFWRAP-Notif");
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

        _container = new();
        _container.AddToClassList("notification-container");
        _rootElement.Add(_container);

        _notification = new VisualElement();
        _notification.AddToClassList("notification");
        _container.Add(_notification);
        
        _title = new Label();
        _title.AddToClassList("notification-title");
        _notification.Add(_title);
        
        _text = new Label();
        _text.AddToClassList("notification-text");
        _notification.Add(_text);
    }

    private void Update()
    {
        if (_isDisplayed || _messageQueue.IsEmpty) return;
        if(!_messageQueue.TryDequeue(out string text)) return;
        _isDisplayed = true;
        string left = text.Split('|')[0];
        string right = text.Split('|')[1];
        _title.text = left;
        _text.text = right;
        
        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        _container.AddToClassList("expanded");
        yield return new WaitForSeconds(0.5f);
        _notification.AddToClassList("expanded");
        yield return new WaitForSeconds(5);
        _notification.RemoveFromClassList("expanded");
        yield return new WaitForSeconds(0.5f);
        _container.RemoveFromClassList("expanded");
        yield return new WaitForSeconds(1);
        _isDisplayed = false;
    }
    
    public void ShowPopup(string title, string text)
    {
        _messageQueue.Enqueue(title + "|" + text);
    }

    public override bool IsMouseOverWindow()
    {
        return false;
    }
}