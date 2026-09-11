using BepInEx.Logging;
using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago.Logging;

/// <inheritdoc />
public class PluginLogger(ManualLogSource manualLogSource) : ILogger
{
    public void Log(LogLevel level, object data) => manualLogSource.Log(level, data);

    public void LogFatal(object data) => Log(LogLevel.Fatal, data);

    public void LogError(object data) => Log(LogLevel.Error, data);

    public void LogWarning(object data) => Log(LogLevel.Warning, data);

    public void LogMessage(object data) => Log(LogLevel.Message, data);

    public void LogInfo(object data) => Log(LogLevel.Info, data);

    public void LogDebug(object data) => Log(LogLevel.Debug, data);
    
    public void LogException(string message, [CanBeNull] Exception ex = null)
    {
        string exceptionMessage = $"{message}";
        if (ex != null) exceptionMessage += $" [{ex.GetType().Name}] {ex.Message}\n{ex.StackTrace}";
        if (ex is { InnerException: not null }) exceptionMessage += $"\n\t[InnerException] Message: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
        LogError(exceptionMessage);
    }

    public void Dispose() => manualLogSource.Dispose();
}