using System;

namespace Base.Logging
{
    public static class LoggerExtensions
    {
        public static void Debug(this ILogger logger, string message, long? messageId = null)
        {
            logger.Log(LogLevel.Debug, message, messageId);
        }

        public static void Error(this ILogger logger, string message = null, long? messageId = null)
        {
            logger.LogException(new Exception(message), message, messageId);
        }

        public static void Error(this ILogger logger, Exception exception, string message = null,
            long? messageId = null)
        {
            logger.LogException(exception, message, messageId);
        }

        public static void Fatal(this ILogger logger, string message, long? messageId = null)
        {
            logger.Log(LogLevel.Fatal, message, messageId);
        }

        public static void Info(this ILogger logger, string message, long? messageId = null)
        {
            logger.Log(LogLevel.Info, message, messageId);
        }

        public static void Trace(this ILogger logger, string message, long? messageId = null)
        {
            logger.Log(LogLevel.Trace, message, messageId);
        }

        public static void Warn(this ILogger logger, string message, long? messageId = null)
        {
            logger.Log(LogLevel.Warn, message, messageId);
        }
    }
}