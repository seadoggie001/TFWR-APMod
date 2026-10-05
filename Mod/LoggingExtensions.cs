using System;
using Microsoft.Extensions.Logging;

// StructuredMessageTemplate isn't available in 4.6.2?
#pragma warning disable CA2254

namespace com.seadoggie.TFWRArchipelago;

public static class LoggingExtensions
{
    extension(ILogger log)
    {
        public void LogException(string message, Exception exception, params object[] args)
        {
            log?.Log(LogLevel.Error, exception, message, args);
        }
    }
}