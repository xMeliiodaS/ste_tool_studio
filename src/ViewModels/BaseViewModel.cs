using ste_tool_studio.Configuration;
using ste_tool_studio.Constants;
using ste_tool_studio.Services;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using IOPath = System.IO.Path;

namespace ste_tool_studio.ViewModels
{
    /// <summary>
    /// Base ViewModel with common functionality shared across all tool ViewModels
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected readonly AppConfiguration _config;
        protected readonly ValidationService _validationService;
        protected readonly IReportService _reportService;
        protected readonly ILoggingService _loggingService;

        private string _selectedFilePath;
        private string _statusMessage;
        private bool _isError;
        private bool _isRunning;
        private int _progressCurrent;
        private int _progressTotal;

        // File type configuration - can be set by the window using this ViewModel
        public string FileFilter { get; set; } = AppConstants.ExcelFileFilter;
        public string FileDialogTitle { get; set; } = AppConstants.ExcelFileDialogTitle;
        public string[] AllowedExtensions { get; set; } = new[] { ".xls", ".xlsx" };

        protected BaseViewModel(
            AppConfiguration config,
            ValidationService validationService,
            IReportService reportService,
            ILoggingService loggingService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

            // Initialize with empty values
            StatusMessage = AppConstants.DefaultStatusReady;
        }

        // Common Properties
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
                    OnSelectedFilePathChanged(value);
                }
            }
        }

        public string SelectedFileName => string.IsNullOrEmpty(SelectedFilePath)
            ? AppConstants.NoFileSelected
            : IOPath.GetFileName(SelectedFilePath);

        public bool HasSelectedFile => !string.IsNullOrEmpty(SelectedFilePath);

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

        // Common Commands
        public ICommand SelectFileCommand { get; protected set; }

        // Common Methods
        public void SelectFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = FileFilter,
                Title = FileDialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                HandleFileSelection(openFileDialog.FileName);
            }
        }

        public void HandleFileSelection(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !IsValidFile(filePath))
            {
                SetStatus($"Invalid file format. Expected: {string.Join(", ", AllowedExtensions)}", true);
                return;
            }

            SelectedFilePath = filePath;
            OnFileSelected(filePath);
            SetStatus($"File selected: {IOPath.GetFileName(filePath)}", false);
        }

        protected virtual void OnSelectedFilePathChanged(string filePath)
        {
            // Override in derived classes if needed
        }

        protected virtual void OnFileSelected(string filePath)
        {
            // Override in derived classes for tool-specific file selection logic
        }

        protected bool IsValidFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = IOPath.GetExtension(filePath).ToLowerInvariant();
            return AllowedExtensions.Any(ext => extension == ext.ToLowerInvariant());
        }

        protected void OnProgressUpdate(int current, int total, string message)
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

        public void SetStatus(string message, bool isError)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = message;
                IsError = isError;
            });
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

