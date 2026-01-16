using BepInEx.Configuration;

namespace com.seadoggie.TFWRArchipelago.Configuration;

/// <summary>
/// This allows for editing and saving connection information to a file (.cfg)
/// Information is also editable via the ConfigurationManager (only if installed) 
/// </summary>
public class APConnectionConfig
{
    private ConfigEntry<string> _urlBinding;
    private ConfigEntry<int> _portBinding;
    private ConfigEntry<string> _usernameBinding;
    private ConfigEntry<string> _passwordBinding;
    
    public void SetupConfig(ConfigFile config)
    {
        _urlBinding = config.Bind("General", "Archipelago Url", "archipelago.gg",
            new ConfigDescription("The URL of the archipelago server.", null, new ConfigurationManagerAttributes { Order = 3 }));
        _portBinding = config.Bind("General", "Archipelago Port", 1234,
            new ConfigDescription("The port assigned to your game.", null, new ConfigurationManagerAttributes { Order = 2 }));
        _usernameBinding = config.Bind("General", "Username", "",
            new ConfigDescription("The username assigned to your game.", null, new ConfigurationManagerAttributes { Order = 1 }));
        _passwordBinding = config.Bind("General", "Password", "",
            new ConfigDescription("The password to your game.", null, new ConfigurationManagerAttributes { Order = 0 }));
    }

    public string Url { get => _urlBinding.Value; set => _urlBinding.Value = value; }
    public int Port { get => _portBinding.Value; set => _portBinding.Value = value; }
    public string Username { get => _usernameBinding.Value; set => _usernameBinding.Value = value; }
    public string Password { get => _passwordBinding.Value; set => _passwordBinding.Value = value; }
}