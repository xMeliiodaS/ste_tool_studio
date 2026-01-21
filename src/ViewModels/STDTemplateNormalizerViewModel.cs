using ste_tool_studio.Configuration;
using ste_tool_studio.Constants;
using ste_tool_studio.Services;
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
        private string _stdName;
        private string _docNumber;
        private string _projectNumber;
        private string _testPlan;
        private string _preparedBy;
        private string _footer;

        public STDTemplateNormalizerViewModel(
            AppConfiguration config,
            ValidationService validationService,
            IReportService reportService,
            ILoggingService loggingService)
            : base(config, validationService, reportService, loggingService)
        {
            // Configure for DOCX files
            FileFilter = AppConstants.DocxFileFilter;
            FileDialogTitle = AppConstants.DocxFileDialogTitle;
            AllowedExtensions = new[] { ".docx" };

            // Initialize commands
            SelectFileCommand = new RelayCommand(ExecuteSelectFile);
            RunSTDNormalizerCommand = new AsyncRelayCommand(ExecuteSTDNormalizerCommand, CanRunAutomation);
        }

        // STD Normalizer specific properties
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
    }
}

