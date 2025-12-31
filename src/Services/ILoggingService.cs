namespace STD_baseline_verifier.Services
{
    /// <summary>
    /// Interface for application logging
    /// </summary>
    public interface ILoggingService
    {
        void LogError(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogSeparator();
    }
}

