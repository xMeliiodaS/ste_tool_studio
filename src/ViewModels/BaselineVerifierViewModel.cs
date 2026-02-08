using ste_tool_studio.Configuration;
using ste_tool_studio.Constants;
using ste_tool_studio.Services;
using System.Windows;
using System.Windows.Input;
using IOPath = System.IO.Path;

namespace ste_tool_studio.ViewModels
{
    /// <summary>
    /// ViewModel specifically for Baseline Verifier tool
    /// </summary>
    public class BaselineVerifierViewModel : BaseViewModel
    {
        private string _stdName;
        private string _iterationPath;
        private string _vvVersion;
        private bool _isAutomationRunning;
        private bool _isViolationRunning;

        public BaselineVerifierViewModel(
            AppConfiguration config,
            ValidationService validationService,
            IReportService reportService,
            ILoggingService loggingService)
            : base(config, validationService, reportService, loggingService)
        {
            // Configure for Excel files
            FileFilter = AppConstants.ExcelFileFilter;
            FileDialogTitle = AppConstants.ExcelFileDialogTitle;
            AllowedExtensions = new[] { ".xls", ".xlsx" };

            // Initialize commands
            SelectFileCommand = new RelayCommand(ExecuteSelectFile);
            RunAutomationCommand = new AsyncRelayCommand(ExecuteRunAutomation, CanRunAutomation);
            RunViolationCheckCommand = new AsyncRelayCommand(ExecuteRunViolationCheck, CanRunAutomation);
            OpenLastBugsReportCommand = new RelayCommand(ExecuteOpenLastBugsReport);
            OpenLastRulesReportCommand = new RelayCommand(ExecuteOpenLastRulesReport);
        }

        // Baseline Verifier specific properties
        public string StdName
        {
            get => _stdName;
            set
            {
                if (_stdName != value)
                {
                    _stdName = value;
                    OnPropertyChanged(nameof(StdName));
                    OnPropertyChanged(nameof(HasStdName));
                }
            }
        }

        public bool HasStdName => !string.IsNullOrWhiteSpace(StdName);

        public string IterationPath
        {
            get => _iterationPath;
            set
            {
                if (_iterationPath != value)
                {
                    _iterationPath = value;
                    OnPropertyChanged(nameof(IterationPath));
                }
            }
        }

        public string VvVersion
        {
            get => _vvVersion;
            set
            {
                if (_vvVersion != value)
                {
                    _vvVersion = value;
                    OnPropertyChanged(nameof(VvVersion));
                }
            }
        }

        public bool IsAutomationRunning
        {
            get => _isAutomationRunning;
            set
            {
                if (_isAutomationRunning != value)
                {
                    _isAutomationRunning = value;
                    OnPropertyChanged(nameof(IsAutomationRunning));
                    UpdateRunningState();
                }
            }
        }

        public bool IsViolationRunning
        {
            get => _isViolationRunning;
            set
            {
                if (_isViolationRunning != value)
                {
                    _isViolationRunning = value;
                    OnPropertyChanged(nameof(IsViolationRunning));
                    UpdateRunningState();
                }
            }
        }

        private void UpdateRunningState()
        {
            IsRunning = IsAutomationRunning || IsViolationRunning;
        }

        // Commands
        public ICommand RunAutomationCommand { get; }
        public ICommand RunViolationCheckCommand { get; }
        public ICommand OpenLastBugsReportCommand { get; }
        public ICommand OpenLastRulesReportCommand { get; }

        // Public methods for simple click handlers
        public async void RunAutomation()
        {
            await ExecuteRunAutomation(null);
        }

        public async void RunViolationCheck()
        {
            await ExecuteRunViolationCheck(null);
        }

        public void OpenLastBugsReport()
        {
            ExecuteOpenLastBugsReport(null);
        }

        public void OpenLastRulesReport()
        {
            ExecuteOpenLastRulesReport(null);
        }

        // Command implementations
        private void ExecuteSelectFile(object obj)
        {
            SelectFile();
        }

        protected override void OnSelectedFilePathChanged(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                _config.ExcelPath = filePath;
            }
        }

        protected override void OnFileSelected(string filePath)
        {
            string fileNameWithoutExt = IOPath.GetFileNameWithoutExtension(filePath);
            bool isPlaceholder = fileNameWithoutExt.Contains("Book");
            StdName = isPlaceholder
                        ? AppConstants.DefaultStdNamePlaceholder
                        : fileNameWithoutExt;

            // Request focus and select all text if placeholder was set
            if (isPlaceholder)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RequestStdNameFocus?.Invoke();
                });
            }
        }

        public event Action RequestStdNameFocus;

        private bool CanRunAutomation(object arg)
        {
            return !IsRunning;
        }

        private async Task ExecuteRunAutomation(object obj)
        {
            if (!ValidateAutomationInputs())
            {
                return;
            }

            _loggingService.LogInfo("User clicked Run Automation.");
            _loggingService.LogInfo($"Inputs: STD=\"{StdName}\", IterationPath=\"{IterationPath}\", VvVersion=\"{VvVersion}\", File=\"{SelectedFilePath}\"");

            IsAutomationRunning = true;
            SetStatus(AppConstants.DefaultStatusRunning, false);

            try
            {
                TrimAndNormalizeInputs();
                _config.UpdateBaselineVerifierConfig(StdName, IterationPath, VvVersion);

                var result = await _validationService.RunAutomationAsync(
                    SelectedFilePath,
                    StdName,
                    OnProgressUpdate);

                if (result.IsSuccess)
                {
                    _loggingService.LogInfo("Output: Automation completed successfully.");
                    SetStatus(AppConstants.SuccessAutomationCompleted, false);
                }
                else
                {
                    _loggingService.LogInfo($"Output: Automation failed. ExitCode={result.ExitCode}. {result.Error?.Trim()}");
                    SetStatus(AppConstants.ErrorExecutionFailed, true);
                    _loggingService.LogError($"Automation failed: {result.Error}");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogInfo($"Output: Automation failed. {ex.Message}");
                SetStatus(AppConstants.ErrorExecutionFailed, true);
                _loggingService.LogError(ex.ToString());
            }
            finally
            {
                IsAutomationRunning = false;
            }
        }

        private async Task ExecuteRunViolationCheck(object obj)
        {
            if (string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                SetStatus("Please select a file.", true);
                return;
            }

            _loggingService.LogInfo("User clicked Run Violation Check.");
            _loggingService.LogInfo($"Inputs: File=\"{SelectedFilePath}\"");

            IsViolationRunning = true;
            SetStatus(AppConstants.DefaultStatusRunning, false);

            try
            {
                var result = await _validationService.RunViolationCheckAsync(
                    SelectedFilePath,
                    OnProgressUpdate);

                if (result.IsSuccess)
                {
                    _loggingService.LogInfo("Output: Violation check completed successfully.");
                    SetStatus(AppConstants.SuccessViolationCheckCompleted, false);
                }
                else
                {
                    _loggingService.LogInfo($"Output: Violation check failed. ExitCode={result.ExitCode}. {result.Error?.Trim()}");
                    SetStatus(AppConstants.ErrorExecutionFailed, true);
                    _loggingService.LogError($"Violation check failed: {result.Error}");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogInfo($"Output: Violation check failed. {ex.Message}");
                SetStatus(AppConstants.ErrorExecutionFailed, true);
                _loggingService.LogError(ex.ToString());
            }
            finally
            {
                IsViolationRunning = false;
            }
        }

        private bool ValidateAutomationInputs()
        {
            if (string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                SetStatus("Please select a file.", true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(StdName) ||
                string.IsNullOrWhiteSpace(IterationPath) ||
                string.IsNullOrWhiteSpace(VvVersion))
            {
                SetStatus(AppConstants.ErrorFillAllFields, true);
                return false;
            }

            return true;
        }

        private void TrimAndNormalizeInputs()
        {
            StdName = StdName?.Trim() ?? string.Empty;
            IterationPath = IterationPath?.Trim() ?? string.Empty;
            VvVersion = VvVersion?.Trim().Replace(',', '.') ?? string.Empty;
        }

        private void ExecuteOpenLastBugsReport(object obj)
        {
            _loggingService.LogInfo("User clicked Open last bugs report.");

            if (!_reportService.ReportExists(AppConstants.AutomationReportFileName))
            {
                _loggingService.LogInfo("Output: Report not found.");
                MessageBox.Show(
                    AppConstants.ErrorNoReportFound,
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (_reportService.OpenReport(AppConstants.AutomationReportFileName))
                _loggingService.LogInfo("Output: Opened bugs report.");
            else
            {
                _loggingService.LogInfo("Output: Failed to open report.");
                MessageBox.Show(
                    string.Format(AppConstants.ErrorFailedToOpenReport, "Unknown error"),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenLastRulesReport(object obj)
        {
            _loggingService.LogInfo("User clicked Open last rules report.");

            if (!_reportService.ReportExists(AppConstants.RulesViolationsReportFileName))
            {
                _loggingService.LogInfo("Output: Report not found.");
                MessageBox.Show(
                    AppConstants.ErrorNoReportFound,
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (_reportService.OpenReport(AppConstants.RulesViolationsReportFileName))
                _loggingService.LogInfo("Output: Opened rules report.");
            else
            {
                _loggingService.LogInfo("Output: Failed to open report.");
                MessageBox.Show(
                    string.Format(AppConstants.ErrorFailedToOpenReport, "Unknown error"),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

