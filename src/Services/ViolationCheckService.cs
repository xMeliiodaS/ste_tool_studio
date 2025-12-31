using STD_baseline_verifier.Constants;
using System;
using System.Threading.Tasks;

namespace STD_baseline_verifier.Services
{
    /// <summary>
    /// Service for running violation check processes
    /// </summary>
    public class ViolationCheckService
    {
        private readonly IProcessExecutionService _processService;
        private readonly ILoggingService _loggingService;

        public ViolationCheckService(IProcessExecutionService processService, ILoggingService loggingService)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Runs the violation check process
        /// </summary>
        public async Task<ProcessExecutionResult> RunCheckAsync(
            string excelFilePath,
            System.Action<int, int, string> progressCallback = null)
        {
            _loggingService.LogSeparator();
            _loggingService.LogInfo($"Starting violation check for: {excelFilePath}");

            string arguments = $"\"{excelFilePath}\"";
            var result = await _processService.ExecuteAsync(
                AppConstants.ViolationCheckExeName,
                arguments,
                progressCallback);

            if (result.IsSuccess)
            {
                _loggingService.LogInfo("Violation check completed successfully");
            }
            else
            {
                _loggingService.LogError($"Violation check failed: {result.Error}");
            }

            _loggingService.LogSeparator();
            return result;
        }
    }
}
