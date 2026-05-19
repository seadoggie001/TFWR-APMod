using BepInEx.Configuration;
using com.seadoggie.TFWRArchipelago.Model;

namespace com.seadoggie.TFWRArchipelago.Configuration;

/// <summary>
/// This allows for editing and saving connection information to a file (.cfg)
/// Information is also editable via the ConfigurationManager (only if installed) 
/// </summary>
public class TfwrConfig
{
    private ConfigEntry<string> _urlBinding;
    private ConfigEntry<int> _portBinding;
    private ConfigEntry<string> _usernameBinding;
    private ConfigEntry<string> _passwordBinding;
    private ConfigEntry<bool> _disableIpc;
    private ConfigEntry<bool> _debug;

    public void SetupConfig(ConfigFile config)
    {
        #region General Config Options

        _urlBinding = config.Bind("General", "Archipelago Url", "archipelago.gg",
            new ConfigDescription("The URL of the archipelago server.", null,
                new ConfigurationManagerAttributes { Order = 3 }));
        _portBinding = config.Bind("General", "Archipelago Port", 1234,
            new ConfigDescription("The port assigned to your game.", null,
                new ConfigurationManagerAttributes { Order = 2 }));
        _usernameBinding = config.Bind("General", "Username", "",
            new ConfigDescription("The username assigned to your game.", null,
                new ConfigurationManagerAttributes { Order = 1 }));
        _passwordBinding = config.Bind("General", "Password", "",
            new ConfigDescription("The password to your game.", null,
                new ConfigurationManagerAttributes { Order = 0 }));

        #endregion

        #region Advanced Config Options

        _disableIpc = config.Bind("Advanced", "Disable Connected Games", false,
            new ConfigDescription("Tap Tap Loot and Bongo Cat will be disabled, but the log won't error", null,
                new ConfigurationManagerAttributes { Order = 0 }));

        _debug = config.Bind("Advanced", "Debug Mode", false, "You don't need debug");

        #endregion
    }

    public ConnectionInfo ConnectionInfo
    {
        get => new(_urlBinding.Value, _portBinding.Value, _usernameBinding.Value, _passwordBinding.Value);
        set
        {
            _urlBinding.Value = value.Url;
            _portBinding.Value = value.Port;
            _usernameBinding.Value = value.Username;
            _passwordBinding.Value = value.Password;
        }
    }

    public bool DisableIpc => _disableIpc.Value;
    public bool Debug => _debug.Value;
}