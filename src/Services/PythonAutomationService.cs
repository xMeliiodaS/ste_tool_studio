using ste_tool_studio.Constants;
using System;
using System.Threading.Tasks;

namespace ste_tool_studio.Services
{
    /// <summary>
    /// Service for running automation validation processes
    /// </summary>
    public class AutomationService
    {
        private readonly IProcessExecutionService _processService;
        private readonly ILoggingService _loggingService;

        public AutomationService(IProcessExecutionService processService, ILoggingService loggingService)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Runs the automation validation process
        /// </summary>
        public async Task<ProcessExecutionResult> RunAutomationAsync(
            string excelFilePath,
            string stdName,
            System.Action<int, int, string> progressCallback = null)
        {
            _loggingService.LogSeparator();
            _loggingService.LogInfo($"Starting automation validation for: {excelFilePath}, STD: {stdName}");

            string arguments = $"\"{excelFilePath}\" \"{stdName}\"";
            var result = await _processService.ExecuteAsync(
                AppConstants.AutomationExeName,
                arguments,
                progressCallback);

            if (result.IsSuccess)
            {
                _loggingService.LogInfo("Automation validation completed successfully");
            }
            else
            {
                _loggingService.LogError($"Automation validation failed: {result.Error}");
            }

            _loggingService.LogSeparator();
            return result;
        }
    }
}
