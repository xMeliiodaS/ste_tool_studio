using ste_tool_studio.Constants;

namespace ste_tool_studio.Services
{
    /// <summary>
    /// Unified service for running validation processes (automation and violation checks)
    /// </summary>
    public class ValidationService
    {
        private readonly IProcessExecutionService _processService;
        private readonly ILoggingService _loggingService;

        public ValidationService(IProcessExecutionService processService, ILoggingService loggingService)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Runs the automation validation process
        /// </summary>
        public async Task<ProcessExecutionResult> RunAutomationAsync(string excelFilePath, string stdName,
                                                                        Action<int, int, string> progressCallback = null)
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
                if (result.HasStderrOutput)
                    _loggingService.LogDebug($"Automation stderr (exit 0): {result.Error?.Trim()}");
            }
            else
            {
                _loggingService.LogError($"Automation validation failed (exit {result.ExitCode}): {result.Error}");
            }

            _loggingService.LogSeparator();
            return result;
        }

        /// <summary>
        /// Runs the violation check process
        /// </summary>
        public async Task<ProcessExecutionResult> RunViolationCheckAsync(string excelFilePath,
                                                                            Action<int, int, string> progressCallback = null)
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
                if (result.HasStderrOutput)
                    _loggingService.LogDebug($"Violation check stderr (exit 0): {result.Error?.Trim()}");
            }
            else
            {
                _loggingService.LogError($"Violation check failed (exit {result.ExitCode}): {result.Error}");
            }

            _loggingService.LogSeparator();
            return result;
        }

        /// <summary>
        /// Runs the STD template normalization process
        /// </summary>
        public async Task<ProcessExecutionResult> RunSTDNormalizationAsync(
            string templateFilePath,
            string stdName,
            string protocolNumber,
            string reportNumber,
            string testPlan,
            string stxNumber,
            string preparedBy,
            bool isReportMode,
            Action<int, int, string> progressCallback = null)
        {
            _loggingService.LogSeparator();
            _loggingService.LogInfo($"Starting STD normalization for: {templateFilePath}");
            _loggingService.LogInfo($"Mode: {(isReportMode ? "Report" : "Protocol")}");

            string mode = isReportMode ? "Report" : "Protocol";
            string arguments =
                                $"\"{templateFilePath}\" " +
                                $"\"{stdName}\" " +
                                $"\"{protocolNumber}\" " +
                                $"\"{reportNumber}\" " +
                                $"\"{testPlan}\" " +
                                $"\"{stxNumber}\" " +
                                $"\"{preparedBy}\" " +
                                $"\"{mode}\"";

            _loggingService.LogInfo($"STD Normalizer args: {arguments}");

            var result = await _processService.ExecuteAsync(
                AppConstants.WordTableNormalizationExeName,
                arguments,
                progressCallback);

            if (result.IsSuccess)
            {
                _loggingService.LogInfo("STD normalization completed successfully");
                if (result.HasStderrOutput)
                    _loggingService.LogDebug($"STD normalizer stderr (exit 0): {result.Error?.Trim()}");
            }
            else
            {
                _loggingService.LogError($"STD normalization failed (exit {result.ExitCode}): {result.Error}");
            }

            _loggingService.LogSeparator();
            return result;
        }
    }
}

