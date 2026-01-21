using Microsoft.Win32;
using ste_tool_studio.Configuration;
using ste_tool_studio.Constants;
using ste_tool_studio.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using IOPath = System.IO.Path;

namespace ste_tool_studio.ViewModels
{
    /// <summary>
    /// Main ViewModel implementing MVVM pattern for the application
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AppConfiguration _config;
        private readonly ValidationService _validationService;
        private readonly IReportService _reportService;
        private readonly ILoggingService _loggingService;

        private string _selectedFilePath;
        private string _stdName;
        private string _iterationPath;
        private string _vvVersion;
        private string _statusMessage;
        
        // STD Normalizer specific properties
        private string _docNumber;
        private string _projectNumber;
        private string _testPlan;
        private string _preparedBy;
        private string _footer;
        private bool _isError;
        private bool _isRunning;
        private bool _isAutomationRunning;
        private bool _isViolationRunning;
        private int _progressCurrent;
        private int _progressTotal;

        // File type configuration - can be set by the window using this ViewModel
        public string FileFilter { get; set; } = AppConstants.ExcelFileFilter;
        public string FileDialogTitle { get; set; } = AppConstants.ExcelFileDialogTitle;
        public string[] AllowedExtensions { get; set; } = new[] { ".xls", ".xlsx" };

        public MainViewModel(
                            AppConfiguration config,
                            ValidationService validationService,
                            IReportService reportService,
                            ILoggingService loggingService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

            // Initialize with empty values (fields start empty on app launch)
            StatusMessage = AppConstants.DefaultStatusReady;

            // Initialize commands
            SelectFileCommand = new RelayCommand(ExecuteSelectFile);
            RunAutomationCommand = new AsyncRelayCommand(ExecuteRunAutomation, CanRunAutomation);
            RunViolationCheckCommand = new AsyncRelayCommand(ExecuteRunViolationCheck, CanRunAutomation);

            OpenLastBugsReportCommand = new RelayCommand(ExecuteOpenLastBugsReport);
            OpenLastRulesReportCommand = new RelayCommand(ExecuteOpenLastRulesReport);

            RunSTDNormalizerCommand = new AsyncRelayCommand(ExecuteSTDNormalizerCommand, CanRunAutomation);
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

        // STD Normalizer properties
        public string DocNumber
        {
            get => _docNumber;
            set
            {
                if (_docNumber != value)
                {
                    _docNumber = value;
                    OnPropertyChanged(nameof(DocNumber));
                }
            }
        }

        public string ProjectNumber
        {
            get => _projectNumber;
            set
            {
                if (_projectNumber != value)
                {
                    _projectNumber = value;
                    OnPropertyChanged(nameof(ProjectNumber));
                }
            }
        }

        public string TestPlan
        {
            get => _testPlan;
            set
            {
                if (_testPlan != value)
                {
                    _testPlan = value;
                    OnPropertyChanged(nameof(TestPlan));
                }
            }
        }

        public string PreparedBy
        {
            get => _preparedBy;
            set
            {
                if (_preparedBy != value)
                {
                    _preparedBy = value;
                    OnPropertyChanged(nameof(PreparedBy));
                }
            }
        }

        public string Footer
        {
            get => _footer;
            set
            {
                if (_footer != value)
                {
                    _footer = value;
                    OnPropertyChanged(nameof(Footer));
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

        // Baseline Verifier
        public ICommand SelectFileCommand { get; }
        public ICommand RunAutomationCommand { get; }
        public ICommand RunViolationCheckCommand { get; }
        public ICommand OpenLastBugsReportCommand { get; }
        public ICommand OpenLastRulesReportCommand { get; }

        // STDNormalizer
        public ICommand RunSTDNormalizerCommand { get; }

        // Public methods for simple click handlers (no ICommand needed)
        public void SelectFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = FileFilter,
                Title = FileDialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                HandleFileSelection(openFileDialog.FileName);
            }
        }

        public async void RunAutomation()
        {
            await ExecuteRunAutomation(null);
        }

        public async void RunViolationCheck()
        {
            await ExecuteRunViolationCheck(null);
        }

        public async void RunSTDNormalizer()
        {
            await ExecuteSTDNormalizerCommand(null);
        }

        public void OpenLastBugsReport()
        {
            ExecuteOpenLastBugsReport(null);
        }

        public void OpenLastRulesReport()
        {
            ExecuteOpenLastRulesReport(null);
        }

        // Command implementations (kept for backward compatibility if needed)
        private void ExecuteSelectFile(object obj)
        {
            SelectFile();
        }

        public event Action RequestStdNameFocus;

        public void HandleFileSelection(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !IsValidFile(filePath))
            {
                SetStatus($"Invalid file format. Expected: {string.Join(", ", AllowedExtensions)}", true);
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

            SetStatus($"File selected: {IOPath.GetFileName(filePath)}", false);
        }


        private bool IsValidFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = IOPath.GetExtension(filePath).ToLowerInvariant();
            return AllowedExtensions.Any(ext => extension == ext.ToLowerInvariant());
        }

        // Keep this method for backward compatibility with BaselineVerifierWindow
        private bool IsValidExcelFile(string filePath)
        {
            return IsValidFile(filePath);
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
                _config.UpdateBaselineVerifierConfig(StdName, IterationPath, VvVersion);

                var result = await _validationService.RunAutomationAsync(
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
                SetStatus("Please select a file.", true);
                return;
            }

            IsViolationRunning = true;
            SetStatus(AppConstants.DefaultStatusRunning, false);

            try
            {
                var result = await _validationService.RunViolationCheckAsync(
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


        private async Task ExecuteSTDNormalizerCommand(object obj)
        {
            if (!ValidateSTDNormalizerInputs())
            {
                return;
            }

            IsRunning = true;
            SetStatus("Normalizing STD template...", false);

            try
            {
                // Trim and normalize inputs
                StdName = StdName?.Trim() ?? string.Empty;
                DocNumber = DocNumber?.Trim() ?? string.Empty;
                ProjectNumber = ProjectNumber?.Trim() ?? string.Empty;
                TestPlan = TestPlan?.Trim() ?? string.Empty;
                PreparedBy = PreparedBy?.Trim() ?? string.Empty;
                Footer = Footer?.Trim() ?? string.Empty;
                PreparedBy = PreparedBy?.Trim() ?? string.Empty;

                _config.UpdateTemplateNormalizerConfig(StdName, DocNumber, ProjectNumber, TestPlan, PreparedBy, Footer, SelectedFilePath);


                var result = await _validationService.RunSTDNormalizationAsync(
                    SelectedFilePath,
                    StdName,
                    DocNumber,
                    ProjectNumber,
                    TestPlan,
                    PreparedBy,
                    Footer,
                    OnProgressUpdate);

                if (result.IsSuccess)
                {
                    SetStatus("STD template normalized successfully!", false);
                }
                else
                {
                    SetStatus("Failed to normalize STD template.", true);
                    _loggingService.LogError($"STD Normalizer failed: {result.Error}");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Failed to normalize STD template.", true);
                _loggingService.LogError($"STD Normalizer failed: {ex}");
            }
            finally
            {
                IsRunning = false;
            }
        }

        private bool ValidateSTDNormalizerInputs()
        {
            if (string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                SetStatus("Please select a DOCX file.", true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(StdName))
            {
                SetStatus("Please enter STD Name.", true);
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
