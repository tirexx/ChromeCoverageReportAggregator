using System;

namespace Base.Logging.NLog
{
    internal class Logger : ILogger
    {
        private readonly global::NLog.Logger _log;

        public Logger(global::NLog.Logger log)
        {
            _log = log;
        }

        public void Log(LogLevel level, string message, long? messageId = null)
        {
            _log.Log(global::NLog.LogLevel.FromOrdinal((int)level), message);
        }

        public void LogException(Exception exception, string message = null, long? messageId = null)
        {
            _log.Log(global::NLog.LogLevel.Error, message + "\r\n" + exception);
        }
    }
}