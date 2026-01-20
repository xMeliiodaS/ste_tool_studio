namespace ste_tool_studio.Services
{
    /// <summary>
    /// Interface for managing report files
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Opens a report file in the default browser
        /// </summary>
        /// <param name="reportFileName">Name of the report file</param>
        /// <returns>True if the report was opened successfully, false otherwise</returns>
        bool OpenReport(string reportFileName);

        /// <summary>
        /// Checks if a report file exists
        /// </summary>
        bool ReportExists(string reportFileName);

        /// <summary>
        /// Gets the full path to a report file
        /// </summary>
        string GetReportPath(string reportFileName);
    }
}

