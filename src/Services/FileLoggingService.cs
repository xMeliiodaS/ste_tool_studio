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
            Log($"[ERROR {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}");
        }

        public void LogInfo(string message)
        {
            Log($"[INFO {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}");
        }

        public void LogWarning(string message)
        {
            Log($"[WARNING {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}");
        }

        public void LogDebug(string message)
        {
            Log($"[DEBUG {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}");
        }

        public void LogSeparator()
        {
            Log("======================================================");
        }

        private void Log(string message)
        {
            lock (_lockObject)
            {
                string dir = System.IO.Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.AppendAllText(_logFilePath, $"\n{message}\n");
            }
        }
    }
}

