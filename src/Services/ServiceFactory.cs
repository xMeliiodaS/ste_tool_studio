using ste_tool_studio.Configuration;
using ste_tool_studio.ViewModels;

namespace ste_tool_studio.Services
{
    /// <summary>
    /// Simple factory for creating service instances and ViewModels
    /// In a production app, consider using a proper DI container (e.g., Microsoft.Extensions.DependencyInjection)
    /// </summary>
    public static class ServiceFactory
    {
        /// <summary>
        /// Creates a BaselineVerifierViewModel with its own configuration instance
        /// </summary>
        public static BaselineVerifierViewModel CreateBaselineVerifierViewModel()
        {
            var config = new AppConfiguration();
            var processService = new ProcessExecutionService();
            var loggingService = new FileLoggingService(config);
            var validationService = new ValidationService(processService, loggingService);
            var reportService = new ReportService(config);

            return new BaselineVerifierViewModel(
                config,
                validationService,
                reportService,
                loggingService);
        }

        /// <summary>
        /// Creates a STDTemplateNormalizerViewModel with its own configuration instance
        /// </summary>
        public static STDTemplateNormalizerViewModel CreateSTDTemplateNormalizerViewModel()
        {
            var config = new AppConfiguration();
            var processService = new ProcessExecutionService();
            var loggingService = new FileLoggingService(config);
            var validationService = new ValidationService(processService, loggingService);
            var reportService = new ReportService(config);

            return new STDTemplateNormalizerViewModel(
                config,
                validationService,
                reportService,
                loggingService);
        }

        /// <summary>
        /// Legacy method for backward compatibility - creates BaselineVerifierViewModel
        /// </summary>
        [Obsolete("Use CreateBaselineVerifierViewModel() instead")]
        public static MainViewModel CreateMainViewModel()
        {
            // For backward compatibility, return a BaselineVerifierViewModel wrapped as MainViewModel
            // This should be removed once all references are updated
            throw new NotSupportedException("MainViewModel is deprecated. Use CreateBaselineVerifierViewModel() or CreateSTDTemplateNormalizerViewModel() instead.");
        }
    }
}

