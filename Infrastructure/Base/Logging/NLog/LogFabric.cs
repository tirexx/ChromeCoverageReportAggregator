using System;
using NLog;

namespace Base.Logging.NLog
{
    public class LogFabric : ILogFabric
    {
        public ILogger GetLog(object typeOrNameForTyppedLogger)
        {
            var type = typeOrNameForTyppedLogger as Type;
            string loggerName;
            if (type == null)
                if (typeOrNameForTyppedLogger == null)
                    loggerName = "NULL";
                else
                    loggerName = typeOrNameForTyppedLogger.GetType().FullName;
            else
                loggerName = type.FullName;

            var log = LogManager.GetLogger(loggerName);

            return new Logger(log);
        }
    }
}