using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Model;
using com.seadoggie.TFWRArchipelago.Patches;
using JetBrains.Annotations;
using UnityEngine;

namespace com.seadoggie.TFWRArchipelago.UI;

/// <summary>
/// A GUI for editing the APConnection settings in game
/// </summary>
public class ArchipelagoSettingsGUI : BaseGUI
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TFWRAP.APConnGUI");

    public event EventHandler<ConnectionInfo> ConnectionAttemptEvent;
    public event EventHandler<EventArgs> DisconnectRequestEvent;
    
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

    private Status _state = Status.None;
    private string _disconnectedReason;

    private enum Status
    {
        None,
        Connecting,
        ConnectionFailed,
        Connected,
        Disconnected,
    }

    public void Show(ConnectionInfo connectionSettings)
    {
        _archUrl = connectionSettings.Url;
        _archPort = connectionSettings.Port.ToString();
        _archUsername = connectionSettings.Username;
        _archPassword = connectionSettings.Password;
        DisplayingWindow = true;
    }

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

    public bool debugMode;

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

    private void OnGUI()
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

    public override bool IsMouseOverWindow() =>
        _windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));

    // Draws the GUI in rows, reusing two rects
    private void DrawWindow(int windowID)
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
            if (_state == Status.Connected)
            {
                if (GUI.Button(new Rect(HorizontalSpacing, contRect.y, 80, ControlHeight), "Disconnect"))
                {
                    // Request a disconnect from the server
                    DisconnectRequestEvent?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (GUI.Button(new Rect(HorizontalSpacing, contRect.y, 80, ControlHeight), "Submit") &&
                     _state != Status.Connecting)
            {
                SaveChanges();
            }

            if (GUI.Button(new Rect(_windowRect.width - 80 - HorizontalSpacing, contRect.y, 80, ControlHeight),
                    "Close")) DisplayingWindow = false;

            if (_state != Status.None)
            {
                string text = _state switch
                {
                    Status.Connected => "Connected",
                    Status.ConnectionFailed => "Failed to connect. Please review the connection settings.",
                    Status.Connecting => "Connecting to Archipelago... please wait...",
                    Status.None => "This is literally impossible",
                    Status.Disconnected => _disconnectedReason ?? "Disconnected",
                    _ => throw new ArgumentException("Unknown state")
                };

                labelRect.y += VerticalSpacing + ControlHeight;
                contRect.y += VerticalSpacing + ControlHeight;
                // make this a larger label so it can display a longer message if needed
                labelRect.width = WindowWidth - (HorizontalSpacing * 2);
                labelRect.height = 100;
                GUI.Label(labelRect, text);
            }

            if (debugMode)
            {
                labelRect.y += VerticalSpacing + ControlHeight;
                contRect.y += VerticalSpacing + ControlHeight;
                if (GUI.Button(new Rect(HorizontalSpacing, contRect.y, 80, contRect.height), "Debug"))
                {
                    HatPopupPatch.ShowWithoutHat("Expand");
                }
            }
        }
        GUI.DragWindow(new Rect(0, 0, WindowWidth, 20));
    }

    private void SaveChanges()
    {
        Log.LogInfo("Saving changes to connection info");

        // Update the internal status
        _state = Status.Connecting;

        // Request the login attempt
        ConnectionAttemptEvent?.Invoke(this, new ConnectionInfo(_archUrl, int.Parse(_archPort),
            _archUsername, _archPassword));
    }

    public void ConnectionAttempt(bool success) => _state = success ? Status.Connected : Status.ConnectionFailed;

    public void Disconnected([CanBeNull] string reason = null)
    {
        _state = Status.Disconnected;
        _disconnectedReason = reason;
    }
}