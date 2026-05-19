using BepInEx.Logging;
using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago.Utils;

public static class LoggerExtension
{
    public static void LogException(this ManualLogSource logger, string message, [CanBeNull] Exception ex = null)
    {
        string exceptionMessage = $"{message}";
        if (ex != null) exceptionMessage = $" [Exception] Message: {ex.Message}\n{ex.StackTrace}";
        if (ex is { InnerException: not null }) exceptionMessage += $"\n\t[InnerException] Message: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
        logger.LogError(exceptionMessage);
    }
}