using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace com.seadoggie.TFWRArchipelago.Configuration;

/// <summary>
/// A GUI for editing the APConnection settings in game
/// </summary>
public class ArchipelagoSettings : MonoBehaviour
{
    public static ArchipelagoSettings Instance { get; private set; }

    // Spacing constants
    private const int ControlHeight = 25, ControlWidth = 200;
    private const int LabelWidth = 100;

    private const int VerticalSpacing = 20, HorizontalSpacing = 20;

    // ToDo: Figure out the correct width/height
    private const int WindowWidth = 500, WindowHeight = 500;

    private string _archUrl = "";
    private string _archPort = "";
    private string _archUsername = "";
    private string _archPassword = "";

    private Rect _windowRect;
    private CursorLockMode _prevCursorLockMode;
    private bool _prevCursorVisible;

    private Task<bool> _archConnectTask;
    private bool _readyToConnect = true;

    /// <summary>
    /// Is the config manager main window displayed on screen
    /// </summary>
    public bool DisplayingWindow
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            if (field)
            {
                CalculateWindowRect();

                _prevCursorLockMode = Cursor.lockState;
                _prevCursorVisible = Cursor.visible;
            }
            else
            {
                if (!_prevCursorVisible || _prevCursorLockMode != CursorLockMode.None)
                    UnlockCursor(_prevCursorLockMode, _prevCursorVisible);
            }
        }
    }

    private void CalculateWindowRect()
    {
        int width = Mathf.Min(Screen.width, WindowWidth);
        int height = WindowHeight > Screen.height ? Screen.height - 100 : WindowHeight;
        int offsetX = Mathf.RoundToInt((Screen.width - width) / 2f);
        int offsetY = Mathf.RoundToInt((Screen.height - height) / 2f);
        _windowRect = new Rect(offsetX, offsetY, width, height);
    }

    private static void UnlockCursor(CursorLockMode mode, bool visible)
    {
        Cursor.lockState = mode;
        Cursor.visible = visible;
    }

    public void Awake()
    {
        Instance = this;
        if (!Plugin.Instance.Loaded) Plugin.Log.LogWarning("Not opening GUI; failed to load properly.");
        _archUrl = Plugin.Instance.ConnectionSettings.Url;
        _archPort = Plugin.Instance.ConnectionSettings.Port.ToString();
        _archUsername = Plugin.Instance.ConnectionSettings.Username;
        _archPassword = Plugin.Instance.ConnectionSettings.Password;

        DisplayingWindow = true;
        Plugin.Instance.WriteUnlocks();
        Plugin.Instance.WriteItems();
    }

    public void OnGUI()
    {
        if (!DisplayingWindow || !enabled) return;
        if (_windowRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.MouseDown
                || Event.current.type == EventType.MouseUp
                || Event.current.type == EventType.MouseDrag)
            {
                Event.current.Use();
                UnlockCursor(CursorLockMode.None, true);
            }
        }

        _windowRect = GUILayout.Window(-619, _windowRect, DrawWindow, "Archipelago Settings");
    }

    public bool IsMouseOverWindow()
    {
        return _windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
    }

    [HarmonyPatch(typeof(GraphicRaycaster), nameof(UnityEngine.UI.GraphicRaycaster.Raycast))]
    [HarmonyPrefix]
    public static bool GraphicRaycaster() => !ArchipelagoSettings.Instance.DisplayingWindow ||
                                             !ArchipelagoSettings.Instance.IsMouseOverWindow();

    // Draws the GUI in rows, reusing two rects
    public void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();
        {
            GUI.Box(new Rect(0, 0, WindowWidth, WindowHeight), "");
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1f);

            Rect labelRect = new(HorizontalSpacing, VerticalSpacing + ControlHeight, LabelWidth, ControlHeight);
            Rect contRect = new(HorizontalSpacing * 2 + LabelWidth, VerticalSpacing + ControlHeight, ControlWidth,
                ControlHeight);
            GUI.Label(labelRect, "Archipelago URL");
            _archUrl = GUI.TextField(contRect, _archUrl);

            labelRect.y += VerticalSpacing + ControlHeight;
            contRect.y += VerticalSpacing + ControlHeight;
            GUI.Label(labelRect, "Archipelago Port");
            _archPort = GUI.TextField(contRect, _archPort);

            labelRect.y += VerticalSpacing + ControlHeight;
            contRect.y += VerticalSpacing + ControlHeight;
            GUI.Label(labelRect, "Username");
            _archUsername = GUI.TextField(contRect, _archUsername);

            labelRect.y += VerticalSpacing + ControlHeight;
            contRect.y += VerticalSpacing + ControlHeight;
            GUI.Label(labelRect, "Password");
            _archPassword = GUI.PasswordField(contRect, _archPassword, '*');

            labelRect.y += VerticalSpacing + ControlHeight;
            contRect.y += VerticalSpacing + ControlHeight;
            if (GUI.Button(new Rect(HorizontalSpacing, contRect.y, 80, ControlHeight), "Submit") &&
                _readyToConnect)
            {
                SaveChanges();
            }

            if (GUI.Button(new Rect(_windowRect.width - 80 - HorizontalSpacing, contRect.y, 80, ControlHeight),
                    "Close"))
            {
                enabled = false;
                DisplayingWindow = false;
            }

            if (_archConnectTask != null)
            {
                labelRect.y += VerticalSpacing + ControlHeight;
                string text;
                if (_archConnectTask.IsCompleted)
                {
                    text = _archConnectTask.Result
                        ? "Connected"
                        : "Failed to connect, please check the logs for details";
                    _readyToConnect = !_archConnectTask.Result;
                }
                else
                {
                    text = "Connecting to Archipelago... please wait...";
                }

                // make this a larger label so it can display a longer message if needed
                labelRect.width = WindowWidth - (HorizontalSpacing * 2);
                labelRect.height = 100;
                GUI.Label(labelRect, text);
            }
        }
        GUI.DragWindow(new Rect(0, 0, WindowWidth, 20));
    }

    private void SaveChanges()
    {
        Plugin.Log.LogInfo("Saving changes to connection info");

        Plugin.Instance.ConnectionSettings.Url = _archUrl;
        Plugin.Instance.ConnectionSettings.Port = int.Parse(_archPort);
        Plugin.Instance.ConnectionSettings.Username = _archUsername;
        Plugin.Instance.ConnectionSettings.Password = _archPassword;

        _archConnectTask = Plugin.Instance.TryEnableAsync();
        _readyToConnect = false;
    }
}