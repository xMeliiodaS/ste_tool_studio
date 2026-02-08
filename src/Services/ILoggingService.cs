namespace ste_tool_studio.Services
{
    /// <summary>
    /// Interface for application logging
    /// </summary>
    public interface ILoggingService
    {
        void LogError(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogDebug(string message);
        void LogSeparator();
    }
}

