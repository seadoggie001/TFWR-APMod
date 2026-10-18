using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using com.seadoggie.TFWRArchipelago.Service;

namespace com.seadoggie.TFWRArchipelago.Utils;

public class ModLogger : IModLog
{
    private readonly ManualLogSource _logger;

    public ModLogger(ManualLogSource logger) => _logger = logger;

    public ModLogger(Type type) : this(type.FullName?.Replace("com.seadoggie.TFWRArchipelago", "TFWRAP"))
    {
    }

    public ModLogger(string name) => _logger = BepInEx.Logging.Logger.CreateLogSource(name);

    public void LogInformation(string message) => _logger.LogInfo(message);

    public void LogInformation(Exception exception, string message, params object[] data)
    {
        LogInformation($"{message}\n\t{FormatException(exception)}");
    }

    public void LogInformation(string message, params object[] data) => LogInformation(FormatMessage(message, data));

    public void LogWarning(string message) => _logger.LogWarning(message);

    public void LogWarning(Exception exception, string message, params object[] data)
    {
        LogWarning($"{message}\n\t{FormatException(exception)}");
    }

    public void LogWarning(string message, params object[] data) => LogWarning(FormatMessage(message, data));

    public void LogError(string message) => _logger.LogError(message);

    public void LogError(Exception exception, string message, params object[] data)
    {
        LogError($"{FormatMessage(message, data)}\n\t{FormatException(exception)}");
    }

    public void LogError(string message, params object[] data) => LogError(FormatMessage(message, data));

    private static string FormatException(Exception exception)
    {
        return $"Exception: {exception.Message}\n\t{exception.StackTrace}";
    }

    private static string FormatMessage(string message, params object[] data)
    {
        Regex formatReplacer = new("(?<!:{){([a-zA-Z0-9_]+)}");
        int index = 0;
        formatReplacer.Replace(message, match => $"{{{index++}}}");
        try
        {
            return string.Format(message, data);
        }
        catch (FormatException)
        {
            return $"{message} | Args: {string.Join(", ", data)}";
        }
    }
}

public interface IModLog
{
    void LogInformation(Exception exception, string message, params object[] data);
    void LogInformation(string message, params object[] data);
    void LogWarning(Exception exception, string message, params object[] data);
    void LogWarning(string message, params object[] data);
    void LogError(Exception exception, string message, params object[] data);
    void LogError(string message, params object[] data);
}

public interface IModLogFactory
{
    IModLog Create(string name);
    IModLog Create(Type type);
}

[Injectable(typeof(IModLogFactory))]
public class ModLogFactory : IModLogFactory, IDisposable
{
    private readonly ConcurrentDictionary<string, IModLog> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public IModLog Create(string name)
    {
        name = name.Replace("com.seadoggie.TFWRArchipelago", "TFWRAP");
        return _loggers.GetOrAdd(name, new ModLogger(name));
    }

    public IModLog Create(Type type) => Create(type.FullName);

    public void Dispose()
    {
        _loggers.Clear();
    }
}