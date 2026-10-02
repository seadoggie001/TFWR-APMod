using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// StructuredMessageTemplate isn't available in 4.6.2?
#pragma warning disable CA2254

namespace com.seadoggie.TFWRArchipelago;

public static class LoggingExtensions
{
    extension(ILogger log)
    {
        //ToDo: Figure out how to format exceptions so I "like" them
        public void LogException(string message, Exception exception) =>
            log?.LogError(exception, message);
        
        public void LogException(Exception exception, [CanBeNull] string message, [ItemCanBeNull] params object[] args) =>
            log?.LogError(exception, message, args);
    }
}