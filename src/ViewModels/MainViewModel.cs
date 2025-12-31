using Microsoft.Win32;
using ste_tool_studio.Configuration;
using ste_tool_studio.Constants;
using ste_tool_studio.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using IOPath = System.IO.Path;

namespace ste_tool_studio.ViewModels
{
    /// <summary>
    /// Main ViewModel implementing MVVM pattern for the application
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AppConfiguration _config;
        private readonly AutomationService _automationService;
        private readonly ViolationCheckService _violationService;
        private readonly IReportService _reportService;
        private readonly ILoggingService _loggingService;

        private string _selectedFilePath;
        private string _stdName;
        private string _iterationPath;
        private string _vvVersion;
        private string _statusMessage;
        private bool _isError;
        private bool _isRunning;
        private bool _isAutomationRunning;
        private bool _isViolationRunning;
        private int _progressCurrent;
        private int _progressTotal;

        public MainViewModel(
            AppConfiguration config,
            AutomationService automationService,
            ViolationCheckService violationService,
            IReportService reportService,
            ILoggingService loggingService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService));
            _violationService = violationService ?? throw new ArgumentNullException(nameof(violationService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

            // Initialize with empty values (fields start empty on app launch)
            StatusMessage = AppConstants.DefaultStatusReady;

            // Initialize commands
            SelectFileCommand = new RelayCommand(ExecuteSelectFile);
            RunAutomationCommand = new AsyncRelayCommand(ExecuteRunAutomation, CanRunAutomation);
            RunViolationCheckCommand = new AsyncRelayCommand(ExecuteRunViolationCheck, CanRunViolationCheck);
            OpenLastBugsReportCommand = new RelayCommand(ExecuteOpenLastBugsReport);
            OpenLastRulesReportCommand = new RelayCommand(ExecuteOpenLastRulesReport);
        }

        // Properties
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                if (_selectedFilePath != value)
                {
                    _selectedFilePath = value;
                    OnPropertyChanged(nameof(SelectedFilePath));
                    OnPropertyChanged(nameof(SelectedFileName));
                    OnPropertyChanged(nameof(HasSelectedFile));
                    if (!string.IsNullOrEmpty(value))
                    {
                        _config.ExcelPath = value;
                    }
                }
            }
        }

        public string SelectedFileName => string.IsNullOrEmpty(SelectedFilePath)
            ? AppConstants.NoFileSelected
            : IOPath.GetFileName(SelectedFilePath);

        public bool HasSelectedFile => !string.IsNullOrEmpty(SelectedFilePath);

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

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public bool IsError
        {
            get => _isError;
            set
            {
                if (_isError != value)
                {
                    _isError = value;
                    OnPropertyChanged(nameof(IsError));
                }
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged(nameof(IsRunning));
                    OnPropertyChanged(nameof(IsNotRunning));
                }
            }
        }

        public bool IsNotRunning => !IsRunning;

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

        public int ProgressCurrent
        {
            get => _progressCurrent;
            set
            {
                if (_progressCurrent != value)
                {
                    _progressCurrent = value;
                    OnPropertyChanged(nameof(ProgressCurrent));
                }
            }
        }

        public int ProgressTotal
        {
            get => _progressTotal;
            set
            {
                if (_progressTotal != value)
                {
                    _progressTotal = value;
                    OnPropertyChanged(nameof(ProgressTotal));
                }
            }
        }

        private void UpdateRunningState()
        {
            IsRunning = IsAutomationRunning || IsViolationRunning;
        }

        // Commands
        public ICommand SelectFileCommand { get; }
        public ICommand RunAutomationCommand { get; }
        public ICommand RunViolationCheckCommand { get; }
        public ICommand OpenLastBugsReportCommand { get; }
        public ICommand OpenLastRulesReportCommand { get; }

        // Command implementations
        private void ExecuteSelectFile(object obj)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = AppConstants.ExcelFileFilter,
                Title = AppConstants.ExcelFileDialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                HandleFileSelection(openFileDialog.FileName);
            }
        }

        public event Action RequestStdNameFocus;

        public void HandleFileSelection(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !IsValidExcelFile(filePath))
            {
                SetStatus(AppConstants.ErrorInvalidFileFormat, true);
                return;
            }

            SelectedFilePath = filePath;

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

            SetStatus(string.Format(AppConstants.SuccessExcelPathUpdated, filePath), false);
        }


        private bool IsValidExcelFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = IOPath.GetExtension(filePath).ToLowerInvariant();
            return extension == ".xls" || extension == ".xlsx";
        }

        private bool CanRunAutomation(object arg)
        {
            // Buttons should always be enabled - validation happens in Execute method
            return !IsRunning;
        }

        private bool CanRunViolationCheck(object arg)
        {
            // Buttons should always be enabled - validation happens in Execute method
            return !IsRunning;
        }

        private async Task ExecuteRunAutomation(object obj)
        {
            if (!ValidateAutomationInputs())
            {
                return;
            }

            IsAutomationRunning = true;
            SetStatus(AppConstants.DefaultStatusRunning, false);

            try
            {
                TrimAndNormalizeInputs();
                _config.UpdateConfig(StdName, IterationPath, VvVersion);

                var result = await _automationService.RunAutomationAsync(
                    SelectedFilePath,
                    StdName,
                    OnProgressUpdate);

                if (result.IsSuccess)
                {
                    SetStatus(AppConstants.SuccessAutomationCompleted, false);
                }
                else
                {
                    SetStatus(AppConstants.ErrorExecutionFailed, true);
                    _loggingService.LogError($"Automation failed: {result.Error}");
                }
            }
            catch (Exception ex)
            {
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
                SetStatus(AppConstants.ErrorSelectExcelFile, true);
                return;
            }

            IsViolationRunning = true;
            SetStatus(AppConstants.DefaultStatusRunning, false);

            try
            {
                var result = await _violationService.RunCheckAsync(
                    SelectedFilePath,
                    OnProgressUpdate);

                if (result.IsSuccess)
                {
                    SetStatus(AppConstants.SuccessViolationCheckCompleted, false);
                }
                else
                {
                    SetStatus(AppConstants.ErrorExecutionFailed, true);
                    _loggingService.LogError($"Violation check failed: {result.Error}");
                }
            }
            catch (Exception ex)
            {
                SetStatus(AppConstants.ErrorExecutionFailed, true);
                _loggingService.LogError(ex.ToString());
            }
            finally
            {
                IsViolationRunning = false;
            }
        }

        private void OnProgressUpdate(int current, int total, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (current >= 0 && total >= 0)
                {
                    ProgressCurrent = current;
                    ProgressTotal = total;
                }
                if (!string.IsNullOrEmpty(message))
                {
                    StatusMessage = message;
                    IsError = false;
                }
            });
        }

        private bool ValidateAutomationInputs()
        {
            if (string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                SetStatus(AppConstants.ErrorSelectExcelFile, true);
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

        public void SetStatus(string message, bool isError)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = message;
                IsError = isError;
            });
        }

        private void ExecuteOpenLastBugsReport(object obj)
        {
            if (!_reportService.ReportExists(AppConstants.AutomationReportFileName))
            {
                MessageBox.Show(
                    AppConstants.ErrorNoReportFound,
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!_reportService.OpenReport(AppConstants.AutomationReportFileName))
            {
                MessageBox.Show(
                    string.Format(AppConstants.ErrorFailedToOpenReport, "Unknown error"),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenLastRulesReport(object obj)
        {
            if (!_reportService.ReportExists(AppConstants.RulesViolationsReportFileName))
            {
                MessageBox.Show(
                    AppConstants.ErrorNoReportFound,
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!_reportService.OpenReport(AppConstants.RulesViolationsReportFileName))
            {
                MessageBox.Show(
                    string.Format(AppConstants.ErrorFailedToOpenReport, "Unknown error"),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
