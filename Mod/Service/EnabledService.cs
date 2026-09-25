namespace com.seadoggie.TFWRArchipelago.Service;

[Injectable(typeof(IEnabledService))]
public class EnabledService : IEnabledService
{
    public bool PluginIsEnabled() => Plugin.Instance.Enabled;
}

public interface IEnabledService
{
    bool PluginIsEnabled();
}