namespace com.seadoggie.TFWRArchipelago.Logging;

public class LogService : ILogService
{
    public ILogger CreateLog(string source) => new PluginLogger(BepInEx.Logging.Logger.CreateLogSource(source));
}

public interface ILogService
{
    public ILogger CreateLog(string source);
}