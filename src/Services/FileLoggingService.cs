using ste_tool_studio.Configuration;
using System.IO;

namespace ste_tool_studio.Services
{
    /// <summary>
    /// File-based logging service
    /// </summary>
    public class FileLoggingService : ILoggingService
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public FileLoggingService(AppConfiguration config)
        {
            _logFilePath = config.GetLogFilePath();
        }

        public void LogError(string message)
        {
            Log($"[ERROR {DateTime.Now}] {message}");
        }

        public void LogInfo(string message)
        {
            Log($"[INFO {DateTime.Now}] {message}");
        }

        public void LogWarning(string message)
        {
            Log($"[WARNING {DateTime.Now}] {message}");
        }

        public void LogSeparator()
        {
            Log("======================================================");
        }

        private void Log(string message)
        {
            lock (_lockObject)
            {
                File.AppendAllText(_logFilePath, $"\n{message}\n");
            }
        }
    }
}

