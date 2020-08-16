using System;

namespace Base.Logging
{
    public interface ILogger
    {
        void Log(LogLevel type, string message, long? messageId = null);
        void LogException(Exception exception, string message = null, long? messageId = null);
    }
}