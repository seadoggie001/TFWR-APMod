namespace com.seadoggie.TFWRArchipelago.Service;

public class EnabledService : IEnabledService
{
    public bool PluginIsEnabled() => Plugin.Instance.Enabled;
}

public interface IEnabledService
{
    bool PluginIsEnabled();
}