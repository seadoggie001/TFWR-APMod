using System;
using System.Collections.Concurrent;
using com.seadoggie.TFWRArchipelago.Service;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago.Logging;

[Injectable(typeof(ILoggerProvider))]
public class PluginLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, PluginLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    
    public ILogger CreateLogger(string categoryName)
    {
        categoryName = categoryName.Replace("com.seadoggie.TFWRArchipelago", "TRFRAP");
        return _loggers.GetOrAdd(categoryName, new PluginLogger(categoryName));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}