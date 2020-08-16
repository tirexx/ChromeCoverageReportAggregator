namespace Base.Logging
{
    public interface ILogFabric
    {
        ILogger GetLog(object typeOrNameForTyppedLogger = null);
    }
}