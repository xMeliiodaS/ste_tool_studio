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
        public static MainViewModel CreateMainViewModel()
        {
            var config = new AppConfiguration();
            var processService = new ProcessExecutionService();
            var loggingService = new FileLoggingService(config);
            var automationService = new AutomationService(processService, loggingService);
            var violationService = new ViolationCheckService(processService, loggingService);
            var reportService = new ReportService(config);

            return new MainViewModel(
                config,
                automationService,
                violationService,
                reportService,
                loggingService);
        }
    }
}

