using System;
using BepInEx.Logging;
using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago.Logging;

public interface ILogger : IDisposable
{
    public void Log(LogLevel level, object data);
    public void LogFatal(object data);
    public void LogError(object data);
    public void LogWarning(object data);
    public void LogMessage(object data);
    public void LogInfo(object data);
    public void LogDebug(object data);
    public void LogException(string message, [CanBeNull] Exception ex = null);
}