using System.Threading.Tasks;

namespace ste_tool_studio.Services
{
    /// <summary>
    /// Interface for executing external processes with progress reporting
    /// </summary>
    public interface IProcessExecutionService
    {
        /// <summary>
        /// Executes an external process and reports progress
        /// </summary>
        /// <param name="exeName">Name of the executable to run</param>
        /// <param name="arguments">Command line arguments</param>
        /// <param name="progressCallback">Callback for progress updates (current, total, message)</param>
        /// <returns>Process exit code and output</returns>
        Task<ProcessExecutionResult> ExecuteAsync(
            string exeName,
            string arguments,
            System.Action<int, int, string> progressCallback = null);
    }

    /// <summary>
    /// Result of process execution
    /// </summary>
    public class ProcessExecutionResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public bool IsSuccess => ExitCode == 0 && string.IsNullOrEmpty(Error);
    }
}

