using ste_tool_studio.Configuration;
using ste_tool_studio.Constants;
using ste_tool_studio.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using IOPath = System.IO.Path;

namespace ste_tool_studio.ViewModels
{
    /// <summary>
    /// ViewModel specifically for STD Template Normalizer tool
    /// </summary>
    public class STDTemplateNormalizerViewModel : BaseViewModel
    {
        private string _docType;
        private string _stdName;
        private string _docNumber;
        private string _projectNumber;
        private string _testPlan;
        private string _preparedBy;
        private string _footer;
        private bool _isReportMode = false; // Default to Protocol mode
        private const string DefaultCycleOption = "Default";
        private string _selectedCycleId;

        public STDTemplateNormalizerViewModel(
            AppConfiguration config,
            ValidationService validationService,
            IReportService reportService,
            ILoggingService loggingService)
            : base(config, validationService, reportService, loggingService)
        {
            // Configure for XLSX files
            FileFilter = AppConstants.XlsxFileFilter;
            FileDialogTitle = AppConstants.XlsxFileDialogTitle;
            AllowedExtensions = new[] { ".xlsx" };

            // Initialize commands
            SelectFileCommand = new RelayCommand(ExecuteSelectFile);
            RunSTDNormalizerCommand = new AsyncRelayCommand(ExecuteSTDNormalizerCommand, CanRunAutomation);

            // Initialize DocType based on default toggle (Protocol)
            DocType = _isReportMode ? "report" : "protocol";

            InitializeCycleOptions();
        }

        // STD Normalizer specific properties

        public string DocType
        {
            get => _docType;
            set
            {
                if (_docType != value)
                {
                    _docType = value;
                    OnPropertyChanged(nameof(DocType));
                }
            }
        }

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

        public bool IsReportMode
        {
            get => _isReportMode;
            set
            {
                if (_isReportMode != value)
                {
                    _isReportMode = value;
                    DocType = _isReportMode ? "report" : "protocol";
                    OnPropertyChanged(nameof(IsReportMode));
                    OnPropertyChanged(nameof(IsProtocolMode));
                }
            }
        }


        public bool IsProtocolMode
        {
            get => !_isReportMode;
            set
            {
                if (IsProtocolMode != value)
                {
                    IsReportMode = !value;
                }
            }
        }

        public ObservableCollection<string> CycleOptions { get; } = new ObservableCollection<string>();

        public string SelectedCycleId
        {
            get => _selectedCycleId;
            set
            {
                if (_selectedCycleId != value)
                {
                    _selectedCycleId = value;
                    OnPropertyChanged(nameof(SelectedCycleId));
                    ApplyCycleDefaults(value);
                }
            }
        }

        private void InitializeCycleOptions()
        {
            CycleOptions.Clear();

            foreach (var cycleId in _config.GetAvailableCycleIds())
            {
                CycleOptions.Add(cycleId);
            }

            // Default selection is a placeholder; no autofill until user chooses a real cycle
            CycleOptions.Insert(0, DefaultCycleOption);
            SelectedCycleId = DefaultCycleOption;
        }

        private void ApplyCycleDefaults(string cycleId)
        {
            if (string.IsNullOrWhiteSpace(cycleId) || string.Equals(cycleId, DefaultCycleOption, StringComparison.OrdinalIgnoreCase))
            {
                DocNumber = string.Empty;
                ProjectNumber = string.Empty;
                TestPlan = string.Empty;
                Footer = string.Empty;
                return;
            }

            if (_config.TryGetCycleTemplateDefaults(cycleId, out var docNumber, out var projectNumber, out var testPlan, out var footer))
            {
                DocNumber = docNumber;
                ProjectNumber = projectNumber;
                TestPlan = testPlan;
                Footer = footer;
            }
        }

        // Commands
        public ICommand RunSTDNormalizerCommand { get; }

        // Public methods for simple click handlers
        public async void RunSTDNormalizer()
        {
            await ExecuteSTDNormalizerCommand(null);
        }

        // Command implementations
        private void ExecuteSelectFile(object obj)
        {
            SelectFile();
        }

        protected override void OnSelectedFilePathChanged(string filePath)
        {
            // STD Normalizer doesn't need to update ExcelPath in config
            // It uses SelectedFilePath directly in UpdateTemplateNormalizerConfig
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

                _config.UpdateTemplateNormalizerConfig(DocType, StdName, DocNumber, ProjectNumber, TestPlan, PreparedBy, Footer, SelectedFilePath);

                var result = await _validationService.RunSTDNormalizationAsync(
                    SelectedFilePath,
                    StdName,
                    DocNumber,
                    ProjectNumber,
                    TestPlan,
                    PreparedBy,
                    Footer,
                    IsReportMode,
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
                SetStatus("Please select an XLSX file.", true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(StdName) ||
                string.IsNullOrWhiteSpace(DocType) ||
                string.IsNullOrWhiteSpace(DocNumber) ||
                string.IsNullOrWhiteSpace(ProjectNumber) ||
                string.IsNullOrWhiteSpace(TestPlan) ||
                string.IsNullOrWhiteSpace(PreparedBy) ||
                string.IsNullOrWhiteSpace(Footer))
            {
                SetStatus("Please fill all required fields.", true);
                return false;
            }

            return true;
        }
    }
}

