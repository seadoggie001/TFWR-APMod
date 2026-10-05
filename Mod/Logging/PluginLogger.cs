using System;
using BepInEx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace com.seadoggie.TFWRArchipelago.Logging;

/// <inheritdoc />
public class PluginLogger(string name) : ILogger
{
    private readonly ManualLogSource _source = BepInEx.Logging.Logger.CreateLogSource(name);

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

        string message = formatter(state, exception);
        if (exception is not null)
            message = $"{exception.GetType().Name}: {message}\n\tException: {exception.Message}\n\t{exception.StackTrace?.Replace("\n", "\n\t")}";

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                _source.LogDebug(message);
                break;
            case LogLevel.Information:
                _source.LogInfo(message);
                break;
            case LogLevel.Warning:
                _source.LogWarning(message);
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                _source.LogError(message);
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
}