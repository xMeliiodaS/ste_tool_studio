namespace ste_tool_studio.Constants
{
    /// <summary>
    /// Centralized constants to eliminate magic strings and hard-coded values
    /// </summary>
    public static class AppConstants
    {
        // Application Info
        public const string AppName = "ste_tool_studio";
        public const string AppDisplayName = "STD Baseline Verifier";

        // File Names
        public const string ConfigFileName = "config.json";
        public const string LogFileName = "app.log";
        public const string AutomationLogFileName = "automation_log.log";
        public const string ExportedSTD = "Exported_STD";

        // Executable Names
        public const string AutomationExeName = "test_bugs_std_validation.exe";
        public const string ViolationCheckExeName = "test_excel_violations.exe";
        public const string WordTableNormalizationExeName = "test_document_normalization.exe";

        // Report File Names
        public const string AutomationReportFileName = "automation_results.html";
        public const string RulesViolationsReportFileName = "rules_violations_report.html";

        // Baseline Verifier-specific Config Keys
        public const string ConfigKeyUrl = "url";
        public const string ConfigKeyExcelPath = "excel_path";
        public const string ConfigKeyStdName = "std_name";
        public const string ConfigKeyCurrentVersion = "current_version";
        public const string ConfigKeyIterationPath = "iteration_path";

        // Template Normalizer-specific Config Keys
        public const string ConfigKeyDocNumber = "doc_number";
        public const string ConfigKeyProjectNumber = "project_number";
        public const string ConfigKeyTestPlan = "test_plan";
        public const string ConfigKeyPreparedBy = "prepared_by";
        public const string ConfigKeyFooter = "footer";

        // Process Output Markers
        public const string ProgressTotalMarker = "PROGRESS_TOTAL:";
        public const string ProgressMarker = "PROGRESS:";
        public const string ProcessFinishedMarker = "PROCESS_FINISHED";

        // File Filters
        public const string ExcelFileFilter = "Excel Files|*.xls;*.xlsx";
        public const string ExcelFileDialogTitle = "Select an Excel File";
        public const string DocxFileFilter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*";
        public const string DocxFileDialogTitle = "Select DOCX File";

        // Default Values
        public const string DefaultStdNamePlaceholder = "Enter STD name";
        public const string DefaultStatusReady = "Ready";
        public const string DefaultStatusRunning = "Running...";
        public const string DefaultStatusAllBugsProcessed = "All bugs processed! ✅";

        // Error Messages
        public const string ErrorSelectExcelFile = "Please Select an Excel File.";
        public const string ErrorFillAllFields = "Please Fill all fields.";
        public const string ErrorExeNotFound = "Error: {0} not found.";
        public const string ErrorExecutionFailed = "Execution failed. See log for details.";
        public const string ErrorInvalidFileFormat = "Invalid file format. Please drop an Excel file.";
        public const string ErrorNoReportFound = "No report file found.";
        public const string ErrorFailedToOpenReport = "Failed to open report: {0}";
        public const string ErrorConfigNotFound = "Default {0} not found at {1}";
        public const string ErrorConfigCreationFailed = "Failed to ensure user config exists: {0}";
        public const string ErrorUpdatingConfig = "Error updating config: {0}";

        // Success Messages
        public const string SuccessAutomationCompleted = "Automation completed successfully!";
        public const string SuccessViolationCheckCompleted = "Violation check completed successfully!";
        public const string SuccessExcelPathUpdated = "Excel path updated to: {0}";

        // Button Text
        public const string ButtonTextCheckStdBugs = "Check STD Bugs in VSTS";
        public const string ButtonTextValidateStdRules = "Validate STD Rules";
        public const string ButtonTextRunning = "Running…";
        public const string ButtonTextSelectStd = "Select STD (Excel)";

        // UI Text
        public const string DragDropHint = "Or drag and drop an Excel file here";
        public const string NoFileSelected = "No file selected";
        public const string PlaceholderStdName = "STD Name";
        public const string PlaceholderIterationPath = "Iteration Path";
        public const string PlaceholderCurrentVvVersion = "Current VV Version";
        public const string DeveloperCredit = "Developed by Bahaa";
    }
}

