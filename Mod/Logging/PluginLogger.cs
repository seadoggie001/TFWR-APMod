using System;
using BepInEx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace com.seadoggie.TFWRArchipelago.Logging;

/// <inheritdoc />
public class PluginLogger : ILogger
{
    private readonly ManualLogSource _source;
    
    public PluginLogger(string name)
    {
        _source = BepInEx.Logging.Logger.CreateLogSource(name);
    }

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => null!;
    
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                break;
            case LogLevel.Information:
                _source.LogInfo(formatter.Invoke(state, exception));
                break;
            case LogLevel.Warning:
                _source.LogWarning(formatter.Invoke(state, exception));
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                _source.LogError(formatter.Invoke(state, exception));
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
}