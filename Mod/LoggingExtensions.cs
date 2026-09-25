using System;
using Microsoft.Extensions.Logging;

namespace com.seadoggie.TFWRArchipelago;

public static class LoggingExtensions
{
    extension(ILogger log)
    {
        public void LogException(string message, Exception exception) =>
            log?.LogError(exception, message);

        public void LogInfo(string message) =>
            log?.LogInformation(message);
    }
}